
/*----------------------------------------------------------------
    Copyright (C) 2018 Senparc

    文件名：SenparcDI.cs
    文件功能描述：针对 .NET Core 的依赖注入扩展类

    创建标识：Senparc - 20180714


    修改标识：Senparc - 20180802
    修改描述：v0.2.5 提供当前类中的方法对 netstandard2.0 的完整支持

    修改标识：pengweiqhca - 20180802
    修改描述：v0.2.8 添加 SenparcDI.GetIServiceProvider() 方法，以支持其他依赖注入框架

----------------------------------------------------------------*/

#if NETSTANDARD2_0 || NETCOREAPP2_0 || NETCOREAPP2_1
using System;
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

        /// <summary>
        /// 全局 ServiceCollection
        /// </summary>
        public static IServiceCollection GlobalServiceCollection { get; set; }

        /// <summary>
        /// 创建一个新的 ServiceCollection 对象
        /// </summary>
        /// <returns></returns>
        public static IServiceCollection GetServiceCollection()
        {
            return GlobalServiceCollection;
        }


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
        public static IServiceProvider GlobalIServiceProvider { get; set; }

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
                GlobalServiceProvider = GetServiceCollection().BuildServiceProvider();
            }
            return GlobalServiceProvider;
        }

        /// <summary>
        /// 获取 ServiceProvider
        /// </summary>
        /// <returns></returns>
        public static IServiceProvider GetIServiceProvider()
        {
            if (GlobalIServiceProvider == null)
            {
                var cache = CacheStrategyFactory.GetObjectCacheStrategyInstance();
                //加锁确保唯一
                using (cache.BeginCacheLock("Senparc.CO2NET.SenparcDI", "GetIServiceProvider"))
                {
                    if (GlobalIServiceProvider == null)
                    {
                        //注意：BuildServiceProvider() 方法每次会生成不同的 ServiceProvider 对象！
                        GlobalIServiceProvider = GetServiceCollection().BuildServiceProvider();
                    }
                }

            }
            return GlobalIServiceProvider;
        }


        /// <summary>
        /// 使用 .net core 默认的 DI 方法获得实例
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T GetService<T>()
        {
            return GetIServiceProvider().GetService<T>();
        }
    }
}
#endif
