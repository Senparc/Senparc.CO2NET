/*----------------------------------------------------------------
    Copyright (C) 2025 Senparc

    File Name: Register.cs
    File Function Description: Senparc.CO2NET.Cache.Redis Quick Registration Process


    Creation Identifier: Senparc - 20180222

    Modification Identifier: Senparc - 20180606
    Modification Description: Cache factory renamed to ContainerCacheStrategyFactory

    Modification Identifier: Senparc - 20180802
    Modification Description: v3.1.0 1. Marked Register.RegisterCacheRedis as obsolete
                     2. Added Register.SetConfigurationOption() method
                     3. Added Register.UseKeyValueRedisNow() method
                     4. Added Register.UseHashRedisNow() method

----------------------------------------------------------------*/

//using Senparc.CO2NET.Cache;
using Senparc.CO2NET.RegisterServices;
using System;

namespace Senparc.CO2NET.Cache.CsRedis
{
    /// <summary>
    /// Redis Registration
    /// </summary>
    public static class Register
    {
        /// <summary>
        /// Register Redis cache information
        /// </summary>
        /// <param name="registerService">RegisterService</param>
        /// <param name="redisConfigurationString">Redis connection string</param>
        /// <param name="redisObjectCacheStrategyInstance">Cache strategy delegate, the first parameter is redisConfigurationString</param>
        /// <returns></returns>
        [Obsolete("The registration process has been automated. Please use the Register.SetConfigurationOption() method to modify the connection string.")]
        public static IRegisterService RegisterCacheRedis(this IRegisterService registerService,
            string redisConfigurationString,
            Func<string, IBaseObjectCacheStrategy> redisObjectCacheStrategyInstance)
        {
            RedisManager.ConfigurationOption = redisConfigurationString;

            //Execute the delegate once here, register the result below to improve execution efficiency for each call
            IBaseObjectCacheStrategy objectCacheStrategy = redisObjectCacheStrategyInstance(redisConfigurationString);
            if (objectCacheStrategy != null)
            {
                CacheStrategyFactory.RegisterObjectCacheStrategy(() => objectCacheStrategy);//Redis
            }

            return registerService;
        }

        /// <summary>
        /// Set connection string (not enabled immediately)
        /// </summary>
        /// <param name="redisConfigurationString"></param>
        public static void SetConfigurationOption(string redisConfigurationString)
        {
            RedisManager.ConfigurationOption = redisConfigurationString;
        }

        /// <summary>
        /// Immediately use key-value pair storage Redis (recommended)
        /// </summary>
        public static void UseKeyValueRedisNow()
        {
            CacheStrategyFactory.RegisterObjectCacheStrategy(() => CsRedis.RedisObjectCacheStrategy.Instance);//Key-Value Redis
        }

        /// <summary>
        /// Immediately use HashSet storage Redis cache strategy
        /// </summary>
        public static void UseHashRedisNow()
        {
            CacheStrategyFactory.RegisterObjectCacheStrategy(() => CsRedis.RedisHashSetObjectCacheStrategy.Instance);//Hash format storage Redis

        }
    }
}
