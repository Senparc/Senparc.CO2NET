/*----------------------------------------------------------------
    Copyright(C) 2024 Senparc

    FileName：UnregisteredDomainCacheStrategyException.cs
    File Function Description：Unregistered domain cache exception

    Creation Identifier：Senparc - 20180707

    Modification Identifier：Senparc - 20180721
    Modification Description：v0.1.1 Fixed the issue where isDebug in RegisterService.Start() was always set to true


----------------------------------------------------------------*/

using Senparc.CO2NET.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Senparc.CO2NET.Exceptions
{
    /// <summary>
    /// Unregistered domain cache exception
    /// </summary>
    public class UnregisteredDomainCacheStrategyException : CacheException
    {
        /// <summary>
        /// UnregisteredDomainCacheStrategyException constructor
        /// </summary>
        /// <param name="domainCacheStrategyType"></param>
        /// <param name="objectCacheStrategyType"></param>
        public UnregisteredDomainCacheStrategyException(Type domainCacheStrategyType, Type objectCacheStrategyType)
            : base($"当前扩展缓存策略没有进行注册：{domainCacheStrategyType.ToString()}，{objectCacheStrategyType.ToString()}，解决方案请参考：https://weixin.senparc.com/QA-551", null, true)
        {
            Trace.SenparcTrace.SendCustomLog("当前扩展缓存策略没有进行注册",
                $"当前扩展缓存策略没有进行注册，CacheStrategyDomain：{domainCacheStrategyType.ToString()}，IBaseObjectCacheStrategy：{objectCacheStrategyType.ToString()}");
        }
    }
}
