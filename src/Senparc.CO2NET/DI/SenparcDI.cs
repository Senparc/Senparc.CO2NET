
/*----------------------------------------------------------------
    Copyright (C) 2024 Senparc

    FileName: SenparcDI.cs
    File Function Description: Dependency injection extension class for .NET Core

    Creation Identifier: Senparc - 20180714


    Modification Identifier: Senparc - 20180802
    Modification Description: v0.2.5 Provide full support for netstandard2.0 in the methods of this class

    Modification Identifier: pengweiqhca - 20180802
    Modification Description: v0.2.8 Add SenparcDI.GetIServiceProvider() method to support other dependency injection frameworks

    Modification Identifier: Senparc - 20190118
    Modification Description: v0.5.2 Add SenparcDI.GetRequiredService() method to provide independent ServiceProvider instances within threads

    Modification Identifier: Senparc - 201901527
    Modification Description: v0.8.2 Add SenparcDI.ResetGlobalIServiceProvider(this IServiceCollection serviceCollection) method

    Modification Identifier: Senparc - 20210702
    Modification Description: 1.4.400.2 Add GetService() method

    Modification Identifier: Senparc - 20210702
    Modification Description: 1.4.400.2 Add GetService() method

----------------------------------------------------------------*/

#if !NET462
using System;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using Senparc.CO2NET.Cache;

namespace Senparc.CO2NET
{
    /// <summary>
    /// Dependency injection extension class for .NET Core
    /// </summary>
    public static class SenparcDI
    {
        //public const string SENPARC_DI_THREAD_SERVICE_PROVIDER = "___SenparcDIThreadServiceProvider";
        public const string SENPARC_DI_THREAD_SERVICE_SCOPE = "___SenparcDIThreadScope";

        /// <summary>
        /// Global ServiceCollection
        /// </summary>
        public static IServiceCollection GlobalServiceCollection { get; set; }

        ///// <summary>
        ///// Create a new ServiceCollection object
        ///// </summary>
        ///// <returns></returns>
        //public static IServiceCollection GetServiceCollection()
        //{
        //    return GlobalServiceCollection;
        //}

        //private static object _globalIServiceProviderLock = new object();
        //private static object _threadIServiceProviderLock = new object();

        private static IServiceProvider _globalServiceProvider;


        /// <summary>
        /// Rebuild from GlobalServiceCollection to generate a new IServiceProvider
        /// </summary>
        /// <returns></returns>
        public static IServiceProvider GetServiceProvider()
        {
            return GetServiceProvider(false);
        }

        /// <summary>
        /// Rebuild from GlobalServiceCollection to generate a new IServiceProvider
        /// </summary>
        /// <returns></returns>
        public static IServiceProvider GetServiceProvider(bool refresh = false)
        {
            if (_globalServiceProvider != null && !refresh)
            {
                return _globalServiceProvider;
            }
            _globalServiceProvider = GlobalServiceCollection.BuildServiceProvider();
            return _globalServiceProvider;
        }

        /// <summary>
        /// Single Scope ServiceScope within the thread
        /// </summary>
        public static IServiceScope ThreadServiceScope
        {
            get
            {
                var threadServiceScope = Thread.GetData(Thread.GetNamedDataSlot(SENPARC_DI_THREAD_SERVICE_SCOPE)) as IServiceScope;
                return threadServiceScope;
            }
        }

        /// <summary>
        /// Execute .GetService() method via GetServiceProvider() method
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static object GetService(Type type)
        {
            return GetServiceProvider().GetService(type);
        }

        /// <summary>
        /// Execute .GetService() method via GetServiceProvider() method
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static T GetService<T>()
        {
            return GetServiceProvider().GetService<T>();
        }


        /// <summary>
        /// Execute .GetRequiredService() method via GetServiceProvider() method
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static T GetRequiredService<T>()
        {
            return GetServiceProvider().GetRequiredService<T>();
        }


        /// <summary>
        /// Execute .GetRequiredKeyedService() method via GetServiceProvider() method
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static object GetRequiredKeyedService(Type serviceType, object? serviceKey)
        {
            return GetServiceProvider().GetRequiredKeyedService(serviceType, serviceKey);
        }

        ///// <summary>
        ///// Use .net core default DI method to get instance (recommended)
        ///// <para>If not registered, throw an exception </para>
        ///// </summary>
        ///// <typeparam name="T"></typeparam>
        ///// <returns></returns>
        //public static T GetService<T>(this IServiceProvider serviceProvider)
        //{
        //    return (T)serviceProvider.GetService(typeof(T));
        //}


        /* 过期方法

        /// <summary>
        /// 全局 IServiceCollection 对象
        /// </summary>
        [Obsolete("已过期，请使用系统的注入方式", true)]
        public static IServiceProvider GlobalServiceProvider
        {
            get
            {
                return _globalServiceProvider ?? throw new Exception("请在 Startup.cs 注册过程中，使用 services.AddSenparcGlobalServices() 方法提供全局统一的 ServiceProvider");
            }
            set
            {
                _globalServiceProvider = value;
            }
        }

        /// <summary>
        /// Execute .GetService() method via GetServiceProvider() method
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static object GetService(Type type)
        {
            return GetServiceProvider().GetService(type);
        }


        ///// <summary>
        ///// Use .net core default DI method to get instance (recommended)
        ///// <para>If not registered, throw an exception </para>
        ///// </summary>
        ///// <typeparam name="T"></typeparam>
        ///// <returns></returns>
        //public static T GetService<T>(this IServiceProvider serviceProvider)
        //{
        //    return (T)serviceProvider.GetService(typeof(T));
        //}


        /* 过期方法
        /// <summary>
        /// 获取 ServiceProvider
        /// </summary>
        /// <param name="useGlobalScope">是否使用全局唯一 ServiceScope 对象。
        /// <para>默认为 true，即使用全局唯一 ServiceScope。</para>
        /// <para>如果为 false，即使用线程内唯一 ServiceScope 对象</para>
        /// </param>
        /// <returns></returns>
        [Obsolete("不再储存此对象，直接使用全局统一的GlobalIServiceProvider", true)]
        public static IServiceProvider GetIServiceProvider(bool useGlobalScope = true)
        {
            if (useGlobalScope)
            {
                if (GlobalServiceProvider == null)
                {
                    //加锁确保唯一
                    lock (_globalIServiceProviderLock)
                    {
                        if (GlobalServiceProvider == null)
                        {
                            //注意：BuildServiceProvider() 方法每次会生成不同的 ServiceProvider 对象！
                            GlobalServiceProvider = GlobalServiceCollection.BuildServiceProvider();
                        }
                    }
                }
                return GlobalServiceProvider;
            }
            else
            {
                if (ThreadServiceScope == null)
                {
                    //加锁确保唯一
                    lock (_threadIServiceProviderLock)
                    {
                        if (ThreadServiceScope == null)
                        {
                            //注意：BuildServiceProvider() 方法每次会生成不同的 ServiceProvider 对象！
                            //GlobalIServiceProvider = GetServiceCollection().BuildServiceProvider();

                            var globalServiceProvider = GetIServiceProvider(true);

                            Thread.SetData(Thread.GetNamedDataSlot(SENPARC_DI_THREAD_SERVICE_Scope), globalServiceProvider.CreateScope());
                        }
                    }
                }
                return ThreadServiceScope.ServiceProvider;
            }
        }


        /// <summary>
        /// 使用 .net core 默认的 DI 方法获得实例
        /// <para>如果未注册，返回 null</para>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <param name="useGlobalScope">是否使用全局唯一 ServiceScope 对象，默认为 false，即使用线程内唯一 ServiceScope 对象</param>
        [Obsolete("不再储存此对象，直接使用全局统一的GlobalIServiceProvider", true)]
        public static T GetService<T>(bool useGlobalScope = true)
        {
            return GetIServiceProvider(useGlobalScope).GetService<T>();
        }

        /// <summary>
        /// 使用 .net core 默认的 DI 方法获得实例（推荐）
        /// <para>如果未注册，抛出异常 </para>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        [Obsolete("已过期", true)]
        public static T GetRequiredService<T>()
        {
            return GlobalServiceProvider.GetRequiredService<T>();
        }

        /// <summary>
        /// 使用 .net core 默认的 DI 方法获得实例（推荐）
        /// <para>如果未注册，抛出异常 </para>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        [Obsolete("已过期", true)]
        public static T GetService<T>()
        {
            return GlobalServiceProvider.GetService<T>();
        }


        /// <summary>
        /// 重置 GlobalIServiceProvider 对象，重新从 serviceCollection.BuildServiceProvider() 生成对象
        /// </summary>
        [Obsolete("已过期", true)]
        public static IServiceProvider ResetGlobalIServiceProvider(this IServiceCollection serviceCollection)
        {
            GlobalServiceProvider = serviceCollection.BuildServiceProvider();
            return GlobalServiceProvider;
        }
        */
    }
}
#endif
