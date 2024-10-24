/*----------------------------------------------------------------
    Copyright (C) 2023 Senparc

    FileName: Register.cs
    File Function Description: WebApi Registration


    Creation Identifier: Senparc - 20210627

----------------------------------------------------------------*/

using Microsoft.Extensions.DependencyInjection;
using Senparc.CO2NET.ApiBind;
using Senparc.CO2NET.Trace;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Senparc.CO2NET.WebApi
{
    /// <summary>
    /// WebApi Registration
    /// </summary>
    public static class Register
    {
        /// <summary>
        /// Whether API binding has been completed
        /// </summary>
        public static bool RegisterApiBindFinished { get; private set; } = false;
        /// <summary>
        /// Category names to be ignored
        /// </summary>
        public static List<string> OmitCategoryList = new List<string>();

        /// <summary>
        /// Additional methods
        /// </summary>
        public static Dictionary<MethodInfo, string> AdditionalMethods = new Dictionary<MethodInfo, string>();

        /// <summary>
        /// Additional methods
        /// </summary>
        public static Dictionary<Type, string> AdditionalClasses = new Dictionary<Type, string>();

        /// <summary>
        /// Whether external access is prohibited, default is true
        /// </summary>
        public static bool ForbiddenExternalAccess { get; set; } = true;

        /// <summary>
        /// RegisterApiBind execution lock
        /// </summary>
        private static object RegisterApiBindLock = new object();


        /// <summary>
        /// Automatically scan and register ApiBind
        /// </summary>
        /// <param name="forceBindAgain">Whether to force refresh</param>
        internal static void AddApiBind(this IServiceCollection serviceCollection, bool forceBindAgain)
        {
            var dt1 = SystemTime.Now;

            //var cacheStragegy = CacheStrategyFactory.GetObjectCacheStrategyInstance();
            //using (cacheStragegy.BeginCacheLock("Senparc.NeuChar.Register", "RegisterApiBind"))
            lock (RegisterApiBindLock)//Since local memory is used for recording, there is no need to use a synchronization lock here, thus no need for prerequisites like "cache registration"
            {
                if (RegisterApiBindFinished == true && forceBindAgain == false)
                {
                    Console.WriteLine($"RegisterApiBind has been finished, and doesn't require [forceBindAgain]. Quit build.");

                    return;
                }

                //Find all extended caches
                var scanTypesCount = 0;

                var assembiles = Microsoft.Extensions.DependencyModel.DependencyContext.Default.RuntimeLibraries.Select(z =>
                {
                    try
                    {
                        return Assembly.Load(new AssemblyName(z.Name));
                    }
                    catch
                    {
                        return null;
                    }
                });

                //AppDomain.CurrentDomain.GetAssemblies();
                //Assembly.GetEntryAssembly().GetReferencedAssemblies().Select(Assembly.Load)
                //DependencyContext.Default.CompileLibraries.Select(z => Assembly.Load(z.Name))

                var errorCount = 0;

                //TODO: Delegate to CO2NET for unified scanning

                foreach (var assembly in assembiles)
                {
                    if (assembly == null)
                    {
                        continue;
                    }
                    var assemblyName = assembly.GetName().Name;
                    try
                    {
                        scanTypesCount++;
                        var classTypes = assembly.GetTypes()
                                    //.Where(z => z.Name.EndsWith("api", StringComparison.OrdinalIgnoreCase) ||
                                    //            z.Name.EndsWith("apis", StringComparison.OrdinalIgnoreCase))
                                    .ToArray();

                        foreach (var classType in classTypes)
                        {
                            if (/*type.IsAbstract || Static classes will be recognized as IsAbstract*/
                                !classType.IsPublic || !classType.IsClass || classType.IsEnum)
                            {
                                continue;
                            }

                            //Check if type has ApiBind attribute
                            var typeAttrs = classType.GetCustomAttributes(typeof(ApiBindAttribute), false)
                                                .Select(z => z as ApiBindAttribute).ToList();
                            var omitType = typeAttrs.FirstOrDefault(z => CheckOmitCategory(z, assemblyName)) != null;//Default to ignore the entire class

                            if (typeAttrs.Count > 0 && typeAttrs[0].Ignore)
                            {
                                omitType = true;//Ignore
                            }

                            if (omitType)
                            {
                                continue;//The entire class is ignored, no need to continue
                            }


                            var coverAllMethods = false;//The class already has the [ApiBind] attribute tag that covers all methods
                            var hasChildApiAndNonStaticClass = false;//The current class has objects that need to be referenced (and the current class can be instantiated)

                            if (typeAttrs.Count() == 0 && CheckAdditionalClass(classType, out string classCategory))
                            {
                                typeAttrs.Add(new ApiBindAttribute(classCategory));//This class is specified at runtime
                            }

                            if (typeAttrs.Count() > 0)
                            {
                                //The type has attributes, all methods are archived
                                coverAllMethods = true;
                            }

                            var methods = classType.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.Static /*| BindingFlags.Static | BindingFlags.InvokeMethod*/);

                            foreach (var method in methods)
                            {
                                var methodAttrs = method.GetCustomAttributes(typeof(ApiBindAttribute), false)
                                                        .Select(z => z as ApiBindAttribute).ToList();

                                var isApiMethod = methodAttrs.Count() > 0;//The current method needs API processing

                                if (!isApiMethod && CheckAdditionalMethod(method, out string methodCategory))
                                {
                                    methodAttrs.Add(new ApiBindAttribute(methodCategory));//This method is specified at runtime, highest priority, can ignore other ignore tags
                                }
                                else if (methodAttrs.FirstOrDefault(z => z.Ignore) != null                              //Contains ignore parameters
                                        || method.GetCustomAttribute(typeof(IgnoreApiBindAttribute)) != null            //Contains ignore tags
                                        || methodAttrs.FirstOrDefault(z => CheckOmitCategory(z, assemblyName)) != null  //Requested to be ignored
                                        )
                                {
                                    //Actively marked to be ignored
                                    continue;
                                }

                                if (omitType
                                    && methodAttrs.FirstOrDefault(z => CheckOmitCategory(z, assemblyName)) != null)
                                {
                                    //The class has already been ignored, and the current method does not provide a Category to bypass the ignore, so it is also ignored
                                    continue;
                                }

                                //Check if the current class contains usable methods, if so, mark and process later
                                if (!hasChildApiAndNonStaticClass)
                                {
                                    hasChildApiAndNonStaticClass = coverAllMethods || isApiMethod;//TODO: Note ignored objects
                                }

                                if (isApiMethod)
                                {
                                    //Override binding information of classType
                                    AddApiBindInfos(ApiBindOn.Method, methodAttrs, assemblyName, method);
                                    //TODO: Check objects to be ignored
                                }
                                else if (coverAllMethods)
                                {
                                    //Use binding information of classType
                                    AddApiBindInfos(ApiBindOn.Method, typeAttrs, assemblyName, method);
                                }
                                else
                                {
                                    //Ignore the current method
                                }
                            }

                            var isStaticClass = classType.IsAbstract && classType.IsSealed;
                            if (hasChildApiAndNonStaticClass && !isStaticClass)
                            {
                                //The current class is not a static class, and internal methods are referenced, DI configuration is needed
                                serviceCollection.AddScoped(classType);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        errorCount++;
                        SenparcTrace.SendCustomLog("RegisterApiBind() 自动扫描程序集报告（即使出现错误，非程序异常）：" + assembly.FullName, ex.ToString());
                    }
                }

                RegisterApiBindFinished = true;

                Console.WriteLine($"RegisterApiBind Time: {SystemTime.DiffTotalMS(dt1)}ms, Api Count:{ApiBindInfoCollection.Instance.Count()}, Error Count: {errorCount}");
            }


        }

        /// <summary>
        /// Add categories to be ignored
        /// </summary>
        /// <param name="categoryName"></param>
        public static void AddOmitCategory(string categoryName)
        {
            OmitCategoryList.Add(categoryName);
        }

        /// <summary>
        /// Add additional methods
        /// </summary>
        /// <param name="categoryName"></param>
        /// <param name="methodInfo"></param>
        public static void AddAdditionalMethod(string categoryName, MethodInfo methodInfo)
        {
            AdditionalMethods.Add(methodInfo, categoryName);
        }

        /// <summary>
        /// Add additional classes
        /// </summary>
        /// <param name="categoryName"></param>
        /// <param name="methodInfo"></param>
        public static void AddAdditionalMethod(string categoryName, Type classType)
        {
            AdditionalClasses.Add(classType, categoryName);
        }


        private static void AddApiBindInfos(ApiBindOn apiBindOn, IEnumerable<ApiBindAttribute> apiBindAttrs, string assemblyName, MethodInfo method)
        {
            foreach (var attr in apiBindAttrs)
            {
                AddApiBindInfo(apiBindOn, attr, assemblyName, method);
            }
        }

        private static void AddApiBindInfo(ApiBindOn apiBindOn, ApiBindAttribute apiBindAttr, string assemblyName, MethodInfo method)
        {
            var categoryName = apiBindAttr.GetCategoryName(assemblyName);
            if (!WebApiEngine.ApiAssemblyNames.ContainsKey(categoryName))
            {
                //TODO: Can add more specific prefixes like cache namespaces
                var dynamicCategory = apiBindAttr.GetDynamicCategory(method, assemblyName);

                var addSuccess = WebApiEngine.ApiAssemblyNames.TryAdd(categoryName, dynamicCategory);
                if (!addSuccess)
                {
                    SenparcTrace.SendCustomLog($"动态API未添加成功！", $"信息：[{categoryName} - {dynamicCategory}]");
                }
            }
            ApiBindInfoCollection.Instance.Add(apiBindOn, categoryName, method, apiBindAttr);
        }

        /// <summary>
        /// Check if the current Category needs to be ignored
        /// </summary>
        /// <param name="categoryName"></param>
        /// <returns></returns>
        private static bool CheckOmitCategory(ApiBindAttribute attr, string realAssemblyName)
        {
            if (string.IsNullOrEmpty(realAssemblyName))
            {
                return true;
            }
            var categoryName = attr.GetCategoryName(realAssemblyName);
            return categoryName == null || OmitCategoryList.Contains(categoryName);
        }

        /// <summary>
        /// Check if it is an additionally added class
        /// </summary>
        /// <param name="classType"></param>
        /// <returns></returns>
        private static bool CheckAdditionalClass(Type classType, out string cagetory)
        {
            if (classType == null)
            {
                cagetory = null;
                return false;
            }

            return AdditionalClasses.TryGetValue(classType, out cagetory);
        }

        /// <summary>
        /// Check if it is an additionally added method
        /// </summary>
        /// <param name="methodInfo"></param>
        /// <returns></returns>
        private static bool CheckAdditionalMethod(MethodInfo methodInfo, out string cagetory)
        {
            if (methodInfo == null)
            {
                cagetory = null;
                return false;
            }

            return AdditionalMethods.TryGetValue(methodInfo, out cagetory);
        }
    }
}
