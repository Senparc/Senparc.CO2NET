/*----------------------------------------------------------------
    Copyright (C) 2023 Senparc

    文件名：Register.cs
    文件功能描述：WebApi 注册


    创建标识：Senparc - 20210627

----------------------------------------------------------------*/

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyModel;
using Senparc.CO2NET.ApiBind;
using Senparc.CO2NET.Trace;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Senparc.CO2NET.WebApi
{
    /// <summary>
    /// WebApi 注册
    /// </summary>
    public static class Register
    {
        /// <summary>
        /// 是否API绑定已经执行完
        /// </summary>
        public static bool RegisterApiBindFinished { get; private set; } = false;
        /// <summary>
        /// 需要忽略处理的 Catetory 名称
        /// </summary>
        public static List<string> OmitCategoryList = new List<string>();

        /// <summary>
        /// 额外的方法
        /// </summary>
        public static Dictionary<MethodInfo, string> AdditionalMethods = new Dictionary<MethodInfo, string>();

        /// <summary>
        /// 额外的方法
        /// </summary>
        public static Dictionary<Type, string> AdditionalClasses = new Dictionary<Type, string>();

        /// <summary>
        /// 是否禁止外部访问，默认为 true
        /// </summary>
        public static bool ForbiddenExternalAccess { get; set; } = true;

        /// <summary>
        /// RegisterApiBind 执行锁
        /// </summary>
        private static object RegisterApiBindLock = new object();


        /// <summary>
        /// 自动扫描并注册 ApiBind
        /// </summary>
        /// <param name="forceBindAgain">是否强制重刷新</param>
        internal static void AddApiBind(this IServiceCollection serviceCollection, bool forceBindAgain)
        {
            var dt1 = SystemTime.Now;

            //var cacheStragegy = CacheStrategyFactory.GetObjectCacheStrategyInstance();
            //using (cacheStragegy.BeginCacheLock("Senparc.NeuChar.Register", "RegisterApiBind"))
            lock (RegisterApiBindLock)//由于使用的是本地内存进行记录，所以这里不需要使用同步锁，这样就不需要依“缓存注册”等先决条件
            {
                if (RegisterApiBindFinished == true && forceBindAgain == false)
                {
                    Console.WriteLine($"RegisterApiBind has been finished, and doesn't require [forceBindAgain]. Quit build.");

                    return;
                }

                //查找所有扩展缓存
                var scanTypesCount = 0;
                var assembiles = DependencyContext.Default.RuntimeLibraries.Select(z =>
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

                //TODO:交给 CO2NET 底层统一扫描

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
                            if (/*type.IsAbstract || 静态类会被识别为 IsAbstract*/
                                !classType.IsPublic || !classType.IsClass || classType.IsEnum)
                            {
                                continue;
                            }

                            //判断 type 是否有 ApiBind 标记
                            var typeAttrs = classType.GetCustomAttributes(typeof(ApiBindAttribute), false)
                                                .Select(z => z as ApiBindAttribute).ToList();
                            var omitType = typeAttrs.FirstOrDefault(z => CheckOmitCategory(z, assemblyName)) != null;//默认忽略整个类

                            if (typeAttrs.Count > 0 && typeAttrs[0].Ignore)
                            {
                                omitType = true;//忽略
                            }

                            if (omitType)
                            {
                                continue;//整个类忽略，不用在继续
                            }


                            var coverAllMethods = false;//class 上已经有覆盖所有方法的 [ApiBind] 特性标签
                            var hasChildApiAndNonStaticClass = false;//当前 class 内有需要被引用的对象（且当前 class 可以被实例化）

                            if (typeAttrs.Count() == 0 && CheckAdditionalClass(classType, out string classCategory))
                            {
                                typeAttrs.Add(new ApiBindAttribute(classCategory));//此类被运行时指定
                            }

                            if (typeAttrs.Count() > 0)
                            {
                                //type 中具有标记，所有的方法都归档
                                coverAllMethods = true;
                            }

                            var methods = classType.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.Static /*| BindingFlags.Static | BindingFlags.InvokeMethod*/);

                            foreach (var method in methods)
                            {
                                var methodAttrs = method.GetCustomAttributes(typeof(ApiBindAttribute), false)
                                                        .Select(z => z as ApiBindAttribute).ToList();

                                var isApiMethod = methodAttrs.Count() > 0;//当前方法需要进行 API 处理

                                if (!isApiMethod && CheckAdditionalMethod(method, out string methodCategory))
                                {
                                    methodAttrs.Add(new ApiBindAttribute(methodCategory));//此方法被运行时指定，优先级最高，可以忽略其他忽略标签
                                }
                                else if (methodAttrs.FirstOrDefault(z => z.Ignore) != null                              //包含忽略参数
                                        || method.GetCustomAttribute(typeof(IgnoreApiBindAttribute)) != null            //包含忽略标签
                                        || methodAttrs.FirstOrDefault(z => CheckOmitCategory(z, assemblyName)) != null  //被要求忽略
                                        )
                                {
                                    //主动标记忽略
                                    continue;
                                }

                                if (omitType
                                    && methodAttrs.FirstOrDefault(z => CheckOmitCategory(z, assemblyName)) != null)
                                {
                                    //类已经被忽略，当前方法没有提供可以避开忽略的 Category，因此也被忽略
                                    continue;
                                }

                                //判断当前 class 是否包含可用的方法，如果包含，需要标记并进行后期处理
                                if (!hasChildApiAndNonStaticClass)
                                {
                                    hasChildApiAndNonStaticClass = coverAllMethods || isApiMethod;//TODO 注意忽略的对象
                                }

                                if (isApiMethod)
                                {
                                    //覆盖 classType 的绑定信息
                                    AddApiBindInfos(ApiBindOn.Method, methodAttrs, assemblyName, method);
                                    //TODO:检查需要忽略的对象
                                }
                                else if (coverAllMethods)
                                {
                                    //使用 classType 的绑定信息
                                    AddApiBindInfos(ApiBindOn.Method, typeAttrs, assemblyName, method);
                                }
                                else
                                {
                                    //忽略当前方法
                                }
                            }

                            var isStaticClass = classType.IsAbstract && classType.IsSealed;
                            if (hasChildApiAndNonStaticClass && !isStaticClass)
                            {
                                //当前类不是静态类，且内部方法被引用，需要进行 DI 配置
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
        /// 添加需要忽略的分类
        /// </summary>
        /// <param name="categoryName"></param>
        public static void AddOmitCategory(string categoryName)
        {
            OmitCategoryList.Add(categoryName);
        }

        /// <summary>
        /// 添加额外方法
        /// </summary>
        /// <param name="categoryName"></param>
        /// <param name="methodInfo"></param>
        public static void AddAdditionalMethod(string categoryName, MethodInfo methodInfo)
        {
            AdditionalMethods.Add(methodInfo, categoryName);
        }

        /// <summary>
        /// 添加额外类
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
                //TODO:可以增加缓存命名空间等更加特殊的前缀
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
        /// 检查当前 Catetory 是否需要忽略
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
        /// 检查是否为额外增加的类
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
        /// 检查是否为额外增加的方法
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
