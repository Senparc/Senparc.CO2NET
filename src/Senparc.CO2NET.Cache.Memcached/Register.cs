/*----------------------------------------------------------------
    Copyright (C) 2018 Senparc

    文件名：Register.cs
    文件功能描述：Senparc.CO2NET.Memcached.Redis 快捷注册流程


    创建标识：Senparc - 20180222

    修改标识：Senparc - 20180606
    修改描述：缓存工厂重命名为 ContainerCacheStrategyFactory

----------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using Senparc.CO2NET.RegisterServices;

namespace Senparc.CO2NET.Cache.Memcached
{
    public static class Register
    {

        /// <summary>
        /// 注册 Memcached 缓存信息
        /// </summary>
        /// <param name="registerService">RegisterService</param>
        /// <param name="memcachedConfig">memcached连接字符串列表</param>
        /// <param name="memcachedObjectCacheStrategyInstance">缓存策略的委托，第一个参数为 memcachedConfig</param>
        /// <returns></returns>
        public static IRegisterService RegisterCacheMemcached(this IRegisterService registerService,
            Dictionary<string, int> memcachedConfig,
            Func<Dictionary<string, int>, IBaseObjectCacheStrategy> memcachedObjectCacheStrategyInstance)
        {
            MemcachedObjectCacheStrategy.RegisterServerList(memcachedConfig);

            //此处先执行一次委托，直接在下方注册结果，提高每次调用的执行效率
            IBaseObjectCacheStrategy objectCacheStrategy = memcachedObjectCacheStrategyInstance(memcachedConfig);
            if (objectCacheStrategy != null)
            {
                CacheStrategyFactory.RegisterObjectCacheStrategy(() => objectCacheStrategy);//Memcached
            }

            return registerService;
        }

    }
}
