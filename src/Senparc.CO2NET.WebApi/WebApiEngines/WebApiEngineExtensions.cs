using Microsoft.Extensions.DependencyInjection;
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
    public static class WebApiEngineExtensions
    {
        /// <summary>
        /// 初始化动态API
        /// </summary>
        /// <param name="appDataPath">App_Data 文件夹路径</param>
        /// <param name="builder"></param>
        /// <param name="services"></param>
        /// <param name="showDetailApiLog"></param>
        /// <param name="taskCount"></param>
        /// <param name="additionalAttributes"></param>
        /// <param name="additionalAttributeFunc">是否复制自定义特性（AppBindAttribute 除外）</param>
        public static void UseAndInitDynamicApi(this IServiceCollection services, IMvcCoreBuilder builder,
            string appDataPath, DefaultAction defaultAction = DefaultAction.Post, int taskCount = 4, bool showDetailApiLog = false, bool copyCustomAttributes = true, Func<MethodInfo, IEnumerable<CustomAttributeBuilder>> additionalAttributeFunc = null)
        {
            //预载入程序集，确保在下一步 RegisterApiBind() 可以顺利读取所有接口
            //bool preLoad = typeof(Senparc.Weixin.MP.AdvancedAPIs.AddGroupResult).ToString() != null
            //    && typeof(Senparc.Weixin.WxOpen.AdvancedAPIs.CustomApi).ToString() != null
            //    && typeof(Senparc.Weixin.Open.AccountAPIs.AccountApi).ToString() != null
            //    && typeof(Senparc.Weixin.TenPay.V3.TenPayV3).ToString() != null
            //    && typeof(Senparc.Weixin.Work.AdvancedAPIs.AppApi).ToString() != null;

            services.AddScoped<FindApiService>();
            services.AddScoped<WebApiEngine>(s => new WebApiEngine());

            WebApiEngine.AdditionalAttributeFunc = additionalAttributeFunc;

            var webApiEngine = new WebApiEngine(defaultAction, copyCustomAttributes, taskCount, showDetailApiLog);

            bool preLoad = true;

            //确保 ApiBind 已经执行扫描和注册过程
            Senparc.CO2NET.WebApi.Register.RegisterApiBind(preLoad);//参数为 true，确保重试绑定成功

            //确保目录存在
            webApiEngine.TryCreateDir(appDataPath);

            var dt1 = SystemTime.Now;

            var weixinApis = ApiBind.ApiBindInfoCollection.Instance.GetGroupedCollection();

            ConcurrentDictionary<string, (int apiCount, double costMs)> assemblyBuildStat = new ConcurrentDictionary<string, (int, double)>();

            List<Task> taskList = new List<Task>();

            //因为模块数量比较少，这里使用异步反而会开销略大
            //WeixinApiAssemblyNames.Keys.AsParallel().ForAll(async category =>
            //WeixinApiAssemblyNames.Keys.ToList().ForEach(category =>
            foreach (var category in WebApiEngine.ApiAssemblyNames.Keys)
            {
                var wrapperTask = Task.Factory.StartNew(async () =>
                {
                    //此处使用 Task 效率并不比 Keys.ToList() 方法快
                    webApiEngine.WriteLog($"get weixinApis groups: {weixinApis.Count()}, now dealing with: {category}");
                    var dtStart = SystemTime.Now;
                    var apiBindGroup = weixinApis.FirstOrDefault(z => z.Key == category);

                    var apiCount = await webApiEngine.BuildWebApi(apiBindGroup).ConfigureAwait(false);
                    var apiAssembly = webApiEngine.GetApiAssembly(category);

                    builder.AddApplicationPart(apiAssembly);//程序部件：https://docs.microsoft.com/zh-cn/aspnet/core/mvc/advanced/app-parts?view=aspnetcore-2.2

                    assemblyBuildStat[category] = (apiCount: apiCount, costMs: SystemTime.DiffTotalMS(dtStart));
                });
                taskList.Add(wrapperTask.Unwrap());
            }

            Task.WaitAll(taskList.ToArray());

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
