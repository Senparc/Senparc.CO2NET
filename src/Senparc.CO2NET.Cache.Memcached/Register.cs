/*----------------------------------------------------------------
    Copyright (C) 2024 Senparc

    FileName: Register.cs
    File Function Description: Senparc.CO2NET.Memcached.Redis quick registration process


    Creation Identifier: Senparc - 20180222

    Modification Identifier: Senparc - 20180606
    Modification Description: Cache factory renamed to ContainerCacheStrategyFactory

    Modification Identifier: Senparc - 201800802
    Modification Description: v3.1.0 1. Marked Register.RegisterCacheMemcached as obsolete
                              2. Added Register.SetConfigurationOption() method
                              3. Added Register.UseMemcachedNow() method

----------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using Senparc.CO2NET.RegisterServices;

namespace Senparc.CO2NET.Cache.Memcached
{
    public static class Register
    {

        /// <summary>
        /// Register Memcached cache information
        /// </summary>
        /// <param name="registerService">RegisterService</param>
        /// <param name="memcachedConfig">List of memcached connection strings</param>
        /// <param name="memcachedObjectCacheStrategyInstance">Delegate for cache strategy, the first parameter is memcachedConfig</param>
        /// <returns></returns>
        [Obsolete("注册过程已经自动化，请改用 Register.SetConfigurationOption() 方法修改连接字符串")]
        public static IRegisterService RegisterCacheMemcached(this IRegisterService registerService,
            Dictionary<string, int> memcachedConfig,
            Func<Dictionary<string, int>, IBaseObjectCacheStrategy> memcachedObjectCacheStrategyInstance)
        {
            MemcachedObjectCacheStrategy.RegisterServerList(memcachedConfig);

            // Execute the delegate once here, directly register the result below to improve execution efficiency for each call
            IBaseObjectCacheStrategy objectCacheStrategy = memcachedObjectCacheStrategyInstance(memcachedConfig);
            if (objectCacheStrategy != null)
            {
                CacheStrategyFactory.RegisterObjectCacheStrategy(() => objectCacheStrategy);//Memcached
            }

            return registerService;
        }

        /// <summary>
        /// Set connection information (not enabled immediately)
        /// </summary>
        /// <param name="redisConfigurationString"></param>
        public static void SetConfigurationOption(string redisConfigurationString)
        {
            MemcachedObjectCacheStrategy.RegisterServerList(redisConfigurationString);
        }

        /// <summary>
        /// Use Memcached cache immediately
        /// </summary>
        public static void UseMemcachedNow()
        {
            CacheStrategyFactory.RegisterObjectCacheStrategy(() => MemcachedObjectCacheStrategy.Instance);//Memcached
        }
    }
}
