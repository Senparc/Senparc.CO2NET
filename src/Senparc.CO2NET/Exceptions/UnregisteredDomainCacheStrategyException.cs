/*----------------------------------------------------------------
    Copyright(C) 2018 Senparc

    文件名：UnregisteredDomainCacheStrategyException.cs
    文件功能描述：领域缓存未注册异常

    创建标识：Senparc - 20180707

    修改标识：Senparc - 20180721
    修改描述：v0.1.1 修复 RegisterService.Start() 的 isDebug 设置始终为 true 的问题


----------------------------------------------------------------*/

using Senparc.CO2NET.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Senparc.CO2NET.Exceptions
{
    /// <summary>
    /// 领域缓存未注册异常
    /// </summary>
    public class UnregisteredDomainCacheStrategyException : CacheException
    {
        /// <summary>
        /// UnregisteredDomainCacheStrategyException 构造函数
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
