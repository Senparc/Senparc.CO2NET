/*----------------------------------------------------------------
    Copyright (C) 2025 Senparc

    FileName: Register.cs
    File Function Description: Senparc.CO2NET quick registration process (including Thread, TraceLog, etc.)


    Creation Identifier: Senparc - 20180222

    Modification Identifier: Senparc - 20180516
    Modification Description: Optimize RegisterService

    Modification Identifier: Senparc - 20180704
    Modification Description: v0.1.6.1 Add Register.UseSenparcGlobal() method

    Modification Identifier: Senparc - 20180707
    Modification Description: v0.1.9 Remove senparcSetting parameter from UseSenparcGlobal() method, as it is already provided in RegisterService.Start

----------------------------------------------------------------*/




using System;
using Senparc.CO2NET.Threads;
using Senparc.CO2NET.RegisterServices;
using Senparc.CO2NET.Cache;
using System.Collections.Generic;
using System.Linq;
using Senparc.CO2NET.Helpers;
using Senparc.CO2NET.Extensions;
using Microsoft.Extensions.Configuration;


namespace Senparc.CO2NET
{
    /// <summary>
    /// Senparc.CO2NET basic information registration
    /// </summary>
    public static class Register
    {
        /// <summary>
        /// Modify the default cache namespace
        /// </summary>
        /// <param name="registerService">RegisterService</param>
        /// <param name="customNamespace">Custom namespace name</param>
        /// <returns></returns>
        public static IRegisterService ChangeDefaultCacheNamespace(this IRegisterService registerService, string customNamespace)
        {
            Config.DefaultCacheNamespace = customNamespace;
            return registerService;
        }


        /// <summary>
        /// Method to register Threads (if this thread is not registered, AccessToken, JsTicket, etc. cannot use SDK for automatic storage and management)
        /// </summary>
        /// <param name="registerService">RegisterService</param>
        /// <returns></returns>
        public static IRegisterService RegisterThreads(this IRegisterService registerService)
        {
            ThreadUtility.Register();// If this thread is not registered, AccessToken, JsTicket, etc. cannot use SDK for automatic storage and management.
            return registerService;
        }

        /// <summary>
        /// Method to register TraceLog
        /// </summary>
        /// <param name="registerService">RegisterService</param>
        /// <param name="action"></param>
        /// <returns></returns>
        public static IRegisterService RegisterTraceLog(this IRegisterService registerService, Action action)
        {
            action();
            return registerService;
        }

        /// <summary>
        /// Start Senparc.CO2NET initialization parameter process
        /// </summary>
        /// <param name="registerService"></param>
        /// <param name="autoScanExtensionCacheStrategies">Whether to automatically scan global extension caches (will increase system startup time)</param>
        /// <param name="extensionCacheStrategiesFunc"><para>Extension cache strategies that need to be manually registered</para>
        /// <para>(LocalContainerCacheStrategy, RedisContainerCacheStrategy, MemcacheContainerCacheStrategy are already automatically registered),</para>
        /// <para>If set to null (note: not delegate returning null, but the entire delegate parameter is null), it will automatically use reflection to scan all possible extension cache strategies</para></param>
        /// <returns></returns>
        public static IRegisterService UseSenparcGlobal(this IRegisterService registerService, bool autoScanExtensionCacheStrategies = false, Func<IList<IDomainExtensionCacheStrategy>> extensionCacheStrategiesFunc = null)
        {
            // Register extension cache strategies
            CacheStrategyDomainWarehouse.AutoScanDomainCacheStrategy(autoScanExtensionCacheStrategies, extensionCacheStrategiesFunc);

            return registerService;
        }



#if !NET462

        /// <summary>
        /// Start Senparc.CO2NET initialization parameter process
        /// </summary>
        /// <param name="senparcSetting">SenparcSetting object</param>
        /// <param name="registerConfigure">RegisterService settings</param>
        /// <param name="autoScanExtensionCacheStrategies">Whether to automatically scan global extension caches (will increase system startup time)</param>
        /// <param name="extensionCacheStrategiesFunc"><para>Extension cache strategies that need to be manually registered</para>
        /// <para>(LocalContainerCacheStrategy, RedisContainerCacheStrategy, MemcacheContainerCacheStrategy are already automatically registered),</para>
        /// <para>If set to null (note: not delegate returning null, but the entire delegate parameter is null), it will automatically use reflection to scan all possible extension cache strategies</para></param>
        /// <returns></returns>
        public static IRegisterService UseSenparcGlobal(
            SenparcSetting senparcSetting,
            Action<RegisterService> registerConfigure,
            bool autoScanExtensionCacheStrategies = false,
            Func<IList<IDomainExtensionCacheStrategy>> extensionCacheStrategiesFunc = null)
        {
            // Initialize global RegisterService object and store SenparcSetting information
            var register = RegisterService.Start(senparcSetting);
            RegisterService.Object = register;

            registerConfigure?.Invoke(register);

            return register.UseSenparcGlobal(autoScanExtensionCacheStrategies, extensionCacheStrategiesFunc);
        }


        /// <summary>
        /// Start Senparc.CO2NET initialization parameter process
        /// </summary>
        /// <param name="app">configuration source</param>
        /// <param name="senparcSetting">SenparcSetting object</param>
        /// <param name="registerConfigure">RegisterService settings</param>
        /// <param name="autoScanExtensionCacheStrategies">Whether to automatically scan global extension caches (will increase system startup time)</param>
        /// <param name="extensionCacheStrategiesFunc"><para>Extension cache strategies that need to be manually registered</para>
        /// <para>(LocalContainerCacheStrategy, RedisContainerCacheStrategy, MemcacheContainerCacheStrategy are already automatically registered),</para>
        /// <para>If set to null (note: not delegate returning null, but the entire delegate parameter is null), it will automatically use reflection to scan all possible extension cache strategies</para></param>
        /// <returns></returns>
        public static (IConfigurationRoot app, IRegisterService registerService) UseSenparcGlobal(
            this IConfigurationRoot app,
            SenparcSetting senparcSetting,
            Action<RegisterService> registerConfigure,
            bool autoScanExtensionCacheStrategies = false,
            Func<IList<IDomainExtensionCacheStrategy>> extensionCacheStrategiesFunc = null)
        {
            return (app, UseSenparcGlobal(senparcSetting, registerConfigure, autoScanExtensionCacheStrategies, extensionCacheStrategiesFunc));
        }
#endif
    }
}
