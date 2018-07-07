/*----------------------------------------------------------------
    Copyright (C) 2018 Senparc

    文件名：Register.cs
    文件功能描述：Senparc.Weixin 快捷注册流程（包括Thread、TraceLog等）


    创建标识：Senparc - 20180222

    修改标识：Senparc - 20180516
    修改描述：优化 RegisterService

    修改标识：Senparc - 20180704
    修改描述：v0.1.6.1 添加 Register.UseSenparcGlobal() 方法

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
        /// 开始 Senparc.Weixin SDK 初始化参数流程
        /// </summary>
        /// <param name="registerService"></param>
        /// <param name="senparcSetting"></param>
        /// <param name="extensionCacheStrategiesFunc"><para>需要注册的扩展缓存策略</para>
        /// <para>（LocalContainerCacheStrategy、RedisContainerCacheStrategy、MemcacheContainerCacheStrategy已经自动注册），</para>
        /// <para>如果设置为 null（注意：不适委托返回 null，是整个委托参数为 null），则自动使用反射扫描所有可能存在的扩展缓存策略</para></param>
        /// <param name="autoScanExtensionCacheStrategies">是否自动扫描全局的扩展缓存（会增加系统启动时间）</param>
        /// <returns></returns>
        public static IRegisterService UseSenparcGlobal(this IRegisterService registerService, SenparcSetting senparcSetting, Func<IList<IDomainExtensionCacheStrategy>> extensionCacheStrategiesFunc = null, bool autoScanExtensionCacheStrategies = false)
        {
            //Senparc.CO2NET 配置

            Senparc.CO2NET.Config.SenparcSetting = senparcSetting ?? new SenparcSetting();


            DateTime dt1 = DateTime.Now;

            var cacheTypes = "";//所有注册的扩展缓存

            if (extensionCacheStrategiesFunc != null)
            {
                var containerCacheStrategies = extensionCacheStrategiesFunc();
                if (containerCacheStrategies != null)
                {
                    foreach (var cacheStrategy in containerCacheStrategies)
                    {
                        var exCache = cacheStrategy;//确保能运行到就行，会自动注册
                        cacheTypes += "\r\n" + exCache.GetType();
                    }
                }
            }

            var scanTypesCount = 0;
            if (autoScanExtensionCacheStrategies)
            {
                //查找所有扩展缓存
                var types = AppDomain.CurrentDomain.GetAssemblies()
                            .SelectMany(a =>
                            {
                                try
                                {
                                    scanTypesCount++;
                                    var aTypes = a.GetTypes();
                                    return aTypes.Where(t => !t.IsAbstract &&/* !officialTypes.Contains(t) &&*/ t.GetInterfaces().Contains(typeof(IDomainExtensionCacheStrategy)));
                                }
                                catch (Exception ex)
                                {
                                    Trace.SenparcTrace.SendCustomLog("UseSenparcGlobal() 自动扫描程序集异常：" + a.FullName, ex.ToString());
                                    return new List<Type>();//不能 return null
                                }
                            });

                if (types != null)
                {
                    foreach (var type in types)
                    {
                        if (type == null)
                        {
                            continue;
                        }
                        try
                        {
                            var exCache = ReflectionHelper.GetStaticMember(type, "Instance");
                            cacheTypes += "\r\n" + type;//由于数量不多，这里使用String，不使用StringBuilder
                        }
                        catch (Exception ex)
                        {
                            Trace.SenparcTrace.BaseExceptionLog(new Exceptions.BaseException(ex.Message, ex));
                        }
                    }
                }
            }

            DateTime dt2 = DateTime.Now;
            var exCacheLog = "注册总用时：{0}ms\r\n自动扫描程序集：{1}个\r\n扩展缓存：{2}".FormatWith((dt2 - dt1).TotalMilliseconds, scanTypesCount, cacheTypes);
            Trace.SenparcTrace.SendCustomLog("自动注册扩展缓存完成", exCacheLog);

            return registerService;
        }
    }
}
