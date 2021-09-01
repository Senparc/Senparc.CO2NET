using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Senparc.CO2NET.ApiBind;
using Senparc.CO2NET.Trace;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace Senparc.CO2NET.WebApi.WebApiEngines
{
    /// <summary>
    /// WebApiEngine 扩展方法
    /// </summary>
    public static class WebApiEngineExtensions
    {
        /// <summary>
        /// 初始化动态API
        /// </summary>
        /// <param name="docXmlPath">XML 文档文件夹路径，如果传入 null，则不自动生成 XML 说明文件</param>
        /// <param name="builder"></param>
        /// <param name="services"></param>
        /// <param name="defaultRequestMethod">默认请求方式</param>
        /// <param name="baseApiControllerType">全局 ApiController 的基类，默认为 ControllerBase</param>
        /// <param name="showDetailApiLog"></param>
        /// <param name="taskCount"></param>
        /// <param name="additionalAttributes"></param>
        /// <param name="buildXml">是否创建动态 API 对应的 XML 注释文件</param>
        /// <param name="additionalAttributeFunc">是否复制自定义特性（AppBindAttribute 除外）</param>
        public static void AddAndInitDynamicApi(this IServiceCollection services, IMvcCoreBuilder builder,
            string docXmlPath, ApiRequestMethod defaultRequestMethod = ApiRequestMethod.Post, Type baseApiControllerType = null, int taskCount = 4, bool showDetailApiLog = false, bool copyCustomAttributes = true, Func<MethodInfo, IEnumerable<CustomAttributeBuilder>> additionalAttributeFunc = null)
        {
            AddAndInitDynamicApi(services, (builder, null), docXmlPath, defaultRequestMethod, baseApiControllerType, taskCount, showDetailApiLog, copyCustomAttributes, additionalAttributeFunc);
        }


        /// <summary>
        /// 初始化动态API
        /// </summary>
        /// <param name="docXmlPath">App_Data 文件夹路径</param>
        /// <param name="builder"></param>
        /// <param name="services"></param>
        /// <param name="defaultRequestMethod">默认请求方式</param>
        /// <param name="baseApiControllerType">全局 ApiController 的基类，默认为 ControllerBase</param>
        /// <param name="showDetailApiLog"></param>
        /// <param name="taskCount"></param>
        /// <param name="additionalAttributes"></param>
        /// <param name="additionalAttributeFunc">是否复制自定义特性（AppBindAttribute 除外）</param>
        public static void AddAndInitDynamicApi(this IServiceCollection services, IMvcBuilder builder,
            string docXmlPath, ApiRequestMethod defaultRequestMethod = ApiRequestMethod.Post, Type baseApiControllerType = null, int taskCount = 4, bool showDetailApiLog = false, bool copyCustomAttributes = true, Func<MethodInfo, IEnumerable<CustomAttributeBuilder>> additionalAttributeFunc = null)
        {
            AddAndInitDynamicApi(services, (null, builder), docXmlPath, defaultRequestMethod, baseApiControllerType, taskCount, showDetailApiLog, copyCustomAttributes, additionalAttributeFunc);
        }

        /// <summary>
        /// 初始化动态API
        /// </summary>
        /// <param name="docXmlPath">App_Data 文件夹路径</param>
        /// <param name="builder"></param>
        /// <param name="services"></param>
        /// <param name="defaultRequestMethod">默认请求方式</param>
        /// <param name="baseApiControllerType">全局 ApiController 的基类，默认为 ControllerBase</param>
        /// <param name="showDetailApiLog"></param>
        /// <param name="taskCount"></param>
        /// <param name="additionalAttributes"></param>
        /// <param name="additionalAttributeFunc">是否复制自定义特性（AppBindAttribute 除外）</param>
        private static void AddAndInitDynamicApi(this IServiceCollection services, (IMvcCoreBuilder coreBuilder, IMvcBuilder builder) builder,
            string docXmlPath, ApiRequestMethod defaultRequestMethod = ApiRequestMethod.Post, Type baseApiControllerType = null,
            int taskCount = 4, bool showDetailApiLog = false, bool copyCustomAttributes = true, Func<MethodInfo, IEnumerable<CustomAttributeBuilder>> additionalAttributeFunc = null)
        {
            _ = defaultRequestMethod != ApiRequestMethod.GlobalDefault ? true : throw new Exception($"{nameof(defaultRequestMethod)} 不能作为默认请求类型！");

            services.AddScoped<FindApiService>();
            services.AddScoped(s => new WebApiEngine(docXmlPath));

            WebApiEngine.AdditionalAttributeFunc = additionalAttributeFunc;

            var webApiEngine = new WebApiEngine(docXmlPath, defaultRequestMethod, baseApiControllerType, copyCustomAttributes, taskCount, showDetailApiLog);

            bool preLoad = true;

            //确保 ApiBind 已经执行扫描和注册过程
            services.AddApiBind(preLoad);//参数为 true，确保重试绑定成功

            //确保目录存在
            if (webApiEngine.BuildXml)
            {
                webApiEngine.TryCreateDir(docXmlPath);
            }

            var dt1 = SystemTime.Now;

            var apiGroups = ApiBindInfoCollection.Instance.GetGroupedCollection();
            var apiGouupsCount = apiGroups.Count();

            ConcurrentDictionary<string, (int apiCount, double costMs)> assemblyBuildStat = new ConcurrentDictionary<string, (int, double)>();

            List<Task> taskList = new List<Task>();

            //因为模块数量比较少，这里使用异步反而会开销略大
            //WeixinApiAssemblyNames.Keys.AsParallel().ForAll(async category =>
            //WeixinApiAssemblyNames.Keys.ToList().ForEach(category =>
            var keys = WebApiEngine.ApiAssemblyNames.Keys.ToList();
            for (int i = 0; i < keys.Count; i++)
            {
                var category = keys[i];
                var threadIndex = i;
                var wrapperTask = Task.Factory.StartNew(async () =>
                {
                    try
                    {

                        //此处使用 Task 效率并不比 Keys.ToList() 方法快
                        webApiEngine.WriteLog($"Get API Groups: {threadIndex + 1}/{apiGouupsCount}, now dealing with: {category}");
                        var dtStart = SystemTime.Now;
                        var apiBindGroup = apiGroups.FirstOrDefault(z => z.Key == category);

                        var apiCount = await webApiEngine.BuildWebApi(apiBindGroup).ConfigureAwait(false);
                        var apiAssembly = webApiEngine.GetApiAssembly(category);

                        //程序部件：https://docs.microsoft.com/zh-cn/aspnet/core/mvc/advanced/app-parts?view=aspnetcore-2.2
                        if (builder.coreBuilder != null)
                        {
                            builder.coreBuilder.AddApplicationPart(apiAssembly);
                        }
                        else
                        {
                            builder.builder.AddApplicationPart(apiAssembly);
                        }

                        assemblyBuildStat[category] = (apiCount: apiCount, costMs: SystemTime.DiffTotalMS(dtStart));
                    }
                    catch (Exception ex)
                    {
                        SenparcTrace.BaseExceptionLog(ex);
                    }
                });
                taskList.Add(wrapperTask.Unwrap());
            }
            //foreach (var category in WebApiEngine.ApiAssemblyNames.Keys)
            //{

            //}

            Task.WaitAll(taskList.ToArray());

            //保存 XML文件
            webApiEngine.SaveDynamicApiXml();

            #region 统计数据
            var totalCost = SystemTime.DiffTotalMS(dt1);

            //Func<object, int, string> outputResult = (text, length) => string.Format($"{{0,{length}}}", text);

            webApiEngine.WriteLog("");
            webApiEngine.WriteLog(string.Format("{0,25} | {1,15}| {2,15} |{3,15}", "Category Name", "API Count", "Cost Time", "Average"));
            webApiEngine.WriteLog(new string('-', 80));
            foreach (var item in assemblyBuildStat)
            {
                var apiCount = item.Value.apiCount;
                var cost = item.Value.costMs;
                var avg = Math.Round(cost / apiCount, 3);
                webApiEngine.WriteLog(string.Format("{0,25} | {1,15}| {2,15} |{3,15}", item.Key, apiCount, $"{cost}ms", $"{avg}ms"));
            }
            webApiEngine.WriteLog(new string('=', 80));
            var totalApi = assemblyBuildStat.Values.Sum(z => z.apiCount);
            webApiEngine.WriteLog(string.Format("{0,25} | {1,15}| {2,15} |{3,15}", $"Total", $"API Count:{totalApi}", $"Cost:{totalCost}ms", $""));
            webApiEngine.WriteLog($"Total Average Cost: {Math.Round(totalCost / totalApi, 4)} ms \t\tTask Count: {taskCount}");
            webApiEngine.WriteLog("");

            #endregion
        }


    }
}
