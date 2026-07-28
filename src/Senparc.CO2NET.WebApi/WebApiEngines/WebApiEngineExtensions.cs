using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Senparc.CO2NET.ApiBind;
using Senparc.CO2NET.Trace;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading.Tasks;

namespace Senparc.CO2NET.WebApi.WebApiEngines
{
    /// <summary>
    /// WebApiEngine extension methods
    /// </summary>
    public static class WebApiEngineExtensions
    {
        public static object WebApiInitObject = new object();
        public static bool WebApiInitFinished = false;


        /// <summary>
        /// Initializes dynamic APIs
        /// </summary>
        /// <param name="docXmlPath">XML documentation folder path; if null is passed, XML documentation files are not generated automatically</param>
        /// <param name="builder"></param>
        /// <param name="services"></param>
        /// <param name="options">WebApiEngine configuration</param>
        public static void AddAndInitDynamicApi(this IServiceCollection services, IMvcCoreBuilder builder, Action<WebApiEngineOptions> options = null)
        {
            AddAndInitDynamicApi(services, (builder, null), options);
        }


        /// <summary>
        /// Initializes dynamic APIs
        /// </summary>
        /// <param name="docXmlPath">App_Data folder path</param>
        /// <param name="builder"></param>
        /// <param name="services"></param>
        /// <param name="options">WebApiEngine configuration</param>
        public static void AddAndInitDynamicApi(this IServiceCollection services, IMvcBuilder builder, Action<WebApiEngineOptions> options = null)
        {
            AddAndInitDynamicApi(services, (null, builder), options);
        }

        /// <summary>
        /// Initializes dynamic APIs
        /// </summary>
        /// <param name="docXmlPath">App_Data folder path</param>
        /// <param name="builder"></param>
        /// <param name="services"></param>
        /// <param name="options">WebApiEngine configuration</param>
        private static void AddAndInitDynamicApi(this IServiceCollection services,
                                                      (IMvcCoreBuilder coreBuilder, IMvcBuilder builder) builder,
                                                      Action<WebApiEngineOptions> options = null)
        {
            lock (WebApiInitObject)
            {
                if (WebApiInitFinished)
                {
                    return;
                }

                try
                {

                    services.AddScoped<FindApiService>();
                    services.AddScoped(s => new WebApiEngine(options));

                    var webApiEngine = new WebApiEngine(options);

                    bool preLoad = true;

                    // Ensure ApiBind has completed scanning and registration
                    services.AddApiBind(preLoad);// Pass true to ensure retry binding succeeds

                    // Ensure directory exists
                    if (webApiEngine.BuildXml)
                    {
                        webApiEngine.TryCreateDir(webApiEngine.DocXmlPath);
                    }

                    var dt1 = SystemTime.Now;

                    var apiGroups = ApiBindInfoCollection.Instance.GetGroupedCollection();
                    var apiGouupsCount = apiGroups.Count();

                    ConcurrentDictionary<string, (int apiCount, double costMs)> assemblyBuildStat = new ConcurrentDictionary<string, (int, double)>();

                    List<Task> taskList = new List<Task>();

                    // Because the number of modules is relatively small, using async here would add slightly more overhead
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

                                // Using Task here is not faster than Keys.ToList()
                                webApiEngine.WriteLog($"Get API Groups: {threadIndex + 1}/{apiGouupsCount}, now dealing with: {category}");
                                var dtStart = SystemTime.Now;
                                var apiBindGroup = apiGroups.FirstOrDefault(z => z.Key == category);

                                var apiCount = await webApiEngine.BuildWebApi(apiBindGroup).ConfigureAwait(false);
                                var apiAssembly = webApiEngine.GetApiAssembly(category);

                                // Application parts: https://docs.microsoft.com/zh-cn/aspnet/core/mvc/advanced/app-parts?view=aspnetcore-2.2
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

                    // Save XML files
                    webApiEngine.SaveDynamicApiXml();

                    #region Statistics
                    var totalCost = SystemTime.DiffTotalMS(dt1);

                    //Func<object, int, string> outputResult = (text, length) => string.Format($"{{0,{length}}}", text);

                    webApiEngine.WriteLog("");
                    webApiEngine.WriteLog(string.Format("{0,35} | {1,15}| {2,15} |{3,15}", "Category Name", "API Count", "Cost Time", "Average"));
                    webApiEngine.WriteLog(new string('-', 90));
                    foreach (var item in assemblyBuildStat)
                    {
                        var apiCount = item.Value.apiCount;
                        var cost = item.Value.costMs;
                        var avg = Math.Round(cost / apiCount, 3);
                        webApiEngine.WriteLog(string.Format("{0,35} | {1,15}| {2,15} |{3,15}", item.Key, apiCount, $"{cost}ms", $"{avg}ms"));
                    }
                    webApiEngine.WriteLog(new string('=', 90));
                    var totalApi = assemblyBuildStat.Values.Sum(z => z.apiCount);
                    webApiEngine.WriteLog(string.Format("{0,35} | {1,15}| {2,15} |{3,15}", $"Total", $"API Count:{totalApi}", $"Cost:{totalCost}ms", $""));
                    webApiEngine.WriteLog($"Total Average Cost: {Math.Round(totalCost / totalApi, 4)} ms \t\tTask Count: {webApiEngine.TaskCount}");
                    webApiEngine.WriteLog("");

                    #endregion
                }
                catch (Exception ex)
                {
                    SenparcTrace.BaseExceptionLog(ex);
                }
                finally
                {
                    WebApiInitFinished = true;
                }
            }

        }


    }
}
