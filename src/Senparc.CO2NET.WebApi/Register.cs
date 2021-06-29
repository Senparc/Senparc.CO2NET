using Senparc.CO2NET.ApiBind;
using Senparc.CO2NET.Trace;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace Senparc.CO2NET.WebApi
{
    public class Register
    {
        /// <summary>
        /// 是否API绑定已经执行完
        /// </summary>
        public static bool RegisterApiBindFinished { get; private set; } = false;

        /// <summary>
        /// RegisterApiBind 执行锁
        /// </summary>
        private static object RegisterApiBindLck = new object();

        /// <summary>
        /// 自动扫描并注册 ApiBind
        /// </summary>
        /// <param name="forceBindAgain">是否强制重刷新</param>
        public static void RegisterApiBind(bool forceBindAgain)
        {
            var dt1 = SystemTime.Now;

            //var cacheStragegy = CacheStrategyFactory.GetObjectCacheStrategyInstance();
            //using (cacheStragegy.BeginCacheLock("Senparc.NeuChar.Register", "RegisterApiBind"))
            lock (RegisterApiBindLck)//由于使用的是本地内存进行记录，所以这里不需要使用同步锁，这样就不需要依“缓存注册”等先决条件
            {
                if (RegisterApiBindFinished == true && forceBindAgain == false)
                {
                    Console.WriteLine($"RegisterApiBind has been finished, and doesn't require [forceBindAgain]. Quit build.");

                    return;
                }

                //查找所有扩展缓存
                var scanTypesCount = 0;

                var assembiles = AppDomain.CurrentDomain.GetAssemblies();

                var errorCount = 0;

                //TODO:交给 CO2NET 底层统一扫描

                foreach (var assembly in assembiles)
                {

                    try
                    {
                        scanTypesCount++;
                        var classTypes = assembly.GetTypes()
                                    //.Where(z => z.Name.EndsWith("api", StringComparison.OrdinalIgnoreCase) ||
                                    //            z.Name.EndsWith("apis", StringComparison.OrdinalIgnoreCase))
                                    .ToArray();

                        foreach (var type in classTypes)
                        {

                            if (/*type.IsAbstract || 静态类会被识别为 IsAbstract*/
                                !type.IsPublic || !type.IsClass || type.IsEnum)
                            {
                                continue;
                            }

                            var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly /*| BindingFlags.Static | BindingFlags.InvokeMethod*/);

                            foreach (var method in methods)
                            {
                                var attrs = method.GetCustomAttributes(typeof(ApiBindAttribute), false);
                                foreach (var attr in attrs)
                                {
                                    var apiBindAttr = attr as ApiBindAttribute;
                                    if (!WebApiEngine.WeixinApiAssemblyNames.ContainsKey(apiBindAttr.Category))
                                    {
                                        var newNameSpace = $"Senparc.DynamicWebApi.{Regex.Replace(apiBindAttr.Category,@"[\s\.\(\)]","")}";//TODO:可以换成缓存命名空间等更加特殊的前缀
                                        WebApiEngine.WeixinApiAssemblyNames.TryAdd(apiBindAttr.Category, newNameSpace);
                                    }
                                    ApiBindInfoCollection.Instance.Add(method, apiBindAttr);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        errorCount++;
                        SenparcTrace.SendCustomLog("RegisterApiBind() 自动扫描程序集报告（非程序异常）：" + assembly.FullName, ex.ToString());
                    }
                }

                RegisterApiBindFinished = true;

                Console.WriteLine($"RegisterApiBind Time: {SystemTime.DiffTotalMS(dt1)}ms, Api Count:{ApiBindInfoCollection.Instance.Count()}, Error Count: {errorCount}");
            }
        }
    }
}
