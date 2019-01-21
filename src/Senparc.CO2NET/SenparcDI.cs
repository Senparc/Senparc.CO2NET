
/*----------------------------------------------------------------
    Copyright (C) 2018 Senparc

    文件名：SenparcDI.cs
    文件功能描述：针对 .NET Core 的依赖注入扩展类

    创建标识：Senparc - 20180714


    修改标识：Senparc - 20180802
    修改描述：v0.2.5 提供当前类中的方法对 netstandard2.0 的完整支持

    修改标识：pengweiqhca - 20180802
    修改描述：v0.2.8 添加 SenparcDI.GetIServiceProvider() 方法，以支持其他依赖注入框架

    修改标识：pengweiqhca - 20190118
    修改描述：v0.5.2 添加 SenparcDI.GetRequiredService() 方法，提供线程内独立 ServiceProvider 实例

----------------------------------------------------------------*/

#if NETSTANDARD2_0
using System;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using Senparc.CO2NET.Cache;

namespace Senparc.CO2NET
{
    /// <summary>
    /// 针对 .NET Core 的依赖注入扩展类
    /// </summary>
    public static class SenparcDI
    {
        private static ServiceProvider _globalServiceProvider;

        //public const string SENPARC_DI_THREAD_SERVICE_PROVIDER = "___SenparcDIThreadServiceProvider";
        public const string SENPARC_DI_THREAD_SERVICE_Scope = "___SenparcDIThreadScope";

        /// <summary>
        /// 全局 ServiceCollection
        /// </summary>
        public static IServiceCollection GlobalServiceCollection { get; set; }

        ///// <summary>
        ///// 创建一个新的 ServiceCollection 对象
        ///// </summary>
        ///// <returns></returns>
        //public static IServiceCollection GetServiceCollection()
        //{
        //    return GlobalServiceCollection;
        //}

        private static object _globalIServiceProviderLock = new object();
        private static object _threadIServiceProviderLock = new object();

        /// <summary>
        /// 全局 IServiceCollection 对象
        /// </summary>
        public static IServiceProvider GlobalIServiceProvider { get; set; }

        /// <summary>
        /// 线程内的 单一 Scope 范围 ServiceScope
        /// </summary>
        public static IServiceScope ThreadServiceScope
        {
            get
            {
                var threadServiceScope = Thread.GetData(Thread.GetNamedDataSlot(SENPARC_DI_THREAD_SERVICE_Scope)) as IServiceScope;
                return threadServiceScope;
            }
        }


        /// <summary>
        /// 获取 ServiceProvider
        /// </summary>
        /// <param name="useGlobalServiceProvider">是否使用全局唯一 ServiceProvider 对象，默认为 false，即使用线程内唯一 ServiceProvider 对象</param>
        /// <returns></returns>
        public static IServiceProvider GetIServiceProvider(bool useGlobalServiceProvider = false)
        {
            if (useGlobalServiceProvider)
            {
                if (GlobalIServiceProvider == null)
                {
                    //加锁确保唯一
                    lock (_globalIServiceProviderLock)
                    {
                        if (GlobalIServiceProvider == null)
                        {
                            //注意：BuildServiceProvider() 方法每次会生成不同的 ServiceProvider 对象！
                            GlobalIServiceProvider = GlobalServiceCollection.BuildServiceProvider();
                        }
                    }
                }
                return GlobalIServiceProvider;
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
        /// <param name="useGlobalServiceProvider">是否使用全局唯一 ServiceProvider 对象，默认为 false，即使用线程内唯一 ServiceProvider 对象</param>
        public static T GetService<T>(bool useGlobalServiceProvider = false)
        {
            return GetIServiceProvider(useGlobalServiceProvider).GetService<T>();
        }

        /// <summary>
        /// 使用 .net core 默认的 DI 方法获得实例（推荐）
        /// <para>如果未注册，抛出异常 </para>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <param name="useGlobalServiceProvider">是否使用全局唯一 ServiceProvider 对象，默认为 false，即使用线程内唯一 ServiceProvider 对象</param>
        public static T GetRequiredService<T>(bool useGlobalServiceProvider = false)
        {
            return GetIServiceProvider(useGlobalServiceProvider).GetRequiredService<T>();
        }

        #region 过期方法

        /// <summary>
        /// 已过期，请使用 GlobalIServiceProvider
        /// </summary>
        [Obsolete("Please use GlobalIServiceProvider")]
        public static ServiceProvider GlobalServiceProvider
        {
            get => _globalServiceProvider;
            set
            {
                GlobalIServiceProvider = value;
                _globalServiceProvider = value;
            }
        }

        /// <summary>
        /// 获取 ServiceProvider
        /// </summary>
        /// <returns></returns>
        [Obsolete("Please use GetIServiceProvider")]
        public static ServiceProvider GetServiceProvider()
        {
            if (GlobalServiceProvider == null)
            {
                //注意：BuildServiceProvider() 方法每次会生成不同的 ServiceProvider 对象！
                GlobalServiceProvider = GlobalServiceCollection.BuildServiceProvider();
            }
            return GlobalServiceProvider;
        }

        #endregion
    }
}
#endif
