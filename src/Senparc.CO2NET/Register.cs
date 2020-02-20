/*----------------------------------------------------------------
    Copyright (C) 2020 Senparc

    文件名：Register.cs
    文件功能描述：Senparc.CO2NET 快捷注册流程（包括Thread、TraceLog等）


    创建标识：Senparc - 20180222

    修改标识：Senparc - 20180516
    修改描述：优化 RegisterService

    修改标识：Senparc - 20180704
    修改描述：v0.1.6.1 添加 Register.UseSenparcGlobal() 方法

    修改标识：Senparc - 20180707
    修改描述：v0.1.9 UseSenparcGlobal() 方法删除 senparcSetting 参数，因为在 RegisterService.Start 中已经提供

----------------------------------------------------------------*/

using System;
using Senparc.CO2NET.Threads;
using Senparc.CO2NET.RegisterServices;
using Senparc.CO2NET.Cache;
using System.Collections.Generic;
using System.Linq;
using Senparc.CO2NET.Helpers;
using Senparc.CO2NET.Extensions;


namespace Senparc.CO2NET
{
    /// <summary>
    /// Senparc.CO2NET 基础信息注册
    /// </summary>
    public static class Register
    {
        /// <summary>
        /// 修改默认缓存命名空间
        /// </summary>
        /// <param name="registerService">RegisterService</param>
        /// <param name="customNamespace">自定义命名空间名称</param>
        /// <returns></returns>
        public static IRegisterService ChangeDefaultCacheNamespace(this IRegisterService registerService, string customNamespace)
        {
            Config.DefaultCacheNamespace = customNamespace;
            return registerService;
        }


        /// <summary>
        /// 注册 Threads 的方法（如果不注册此线程，则AccessToken、JsTicket等都无法使用SDK自动储存和管理）
        /// </summary>
        /// <param name="registerService">RegisterService</param>
        /// <returns></returns>
        public static IRegisterService RegisterThreads(this IRegisterService registerService)
        {
            ThreadUtility.Register();//如果不注册此线程，则AccessToken、JsTicket等都无法使用SDK自动储存和管理。
            return registerService;
        }

        /// <summary>
        /// 注册 TraceLog 的方法
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
        /// 开始 Senparc.CO2NET 初始化参数流程
        /// </summary>
        /// <param name="registerService"></param>
        /// <param name="autoScanExtensionCacheStrategies">是否自动扫描全局的扩展缓存（会增加系统启动时间）</param>
        /// <param name="extensionCacheStrategiesFunc"><para>需要手动注册的扩展缓存策略</para>
        /// <para>（LocalContainerCacheStrategy、RedisContainerCacheStrategy、MemcacheContainerCacheStrategy已经自动注册），</para>
        /// <para>如果设置为 null（注意：不适委托返回 null，是整个委托参数为 null），则自动使用反射扫描所有可能存在的扩展缓存策略</para></param>
        /// <returns></returns>
        public static IRegisterService UseSenparcGlobal(this IRegisterService registerService, bool autoScanExtensionCacheStrategies = false, Func<IList<IDomainExtensionCacheStrategy>> extensionCacheStrategiesFunc = null)
        {

            //注册扩展缓存策略
            CacheStrategyDomainWarehouse.AutoScanDomainCacheStrategy(autoScanExtensionCacheStrategies, extensionCacheStrategiesFunc);

            return registerService;
        }

#if !NET45

        /// <summary>
        /// 开始 Senparc.CO2NET 初始化参数流程
        /// </summary>
        /// <param name="senparcSetting">SenparcSetting 对象</param>
        /// <param name="registerConfigure">RegisterService 设置</param>
        /// <param name="autoScanExtensionCacheStrategies">是否自动扫描全局的扩展缓存（会增加系统启动时间）</param>
        /// <param name="extensionCacheStrategiesFunc"><para>需要手动注册的扩展缓存策略</para>
        /// <para>（LocalContainerCacheStrategy、RedisContainerCacheStrategy、MemcacheContainerCacheStrategy已经自动注册），</para>
        /// <para>如果设置为 null（注意：不适委托返回 null，是整个委托参数为 null），则自动使用反射扫描所有可能存在的扩展缓存策略</para></param>
        /// <returns></returns>
        public static IRegisterService UseSenparcGlobal(
            SenparcSetting senparcSetting,
            Action<RegisterService> registerConfigure,
            bool autoScanExtensionCacheStrategies = false,
            Func<IList<IDomainExtensionCacheStrategy>> extensionCacheStrategiesFunc = null)
        {
            //初始化全局 RegisterService 对象，并储存 SenparcSetting 信息
            var register = RegisterService.Start(senparcSetting);
            RegisterService.Object = register;

            registerConfigure?.Invoke(register);

            return register.UseSenparcGlobal(autoScanExtensionCacheStrategies, extensionCacheStrategiesFunc);
        }
#endif
    }
}
