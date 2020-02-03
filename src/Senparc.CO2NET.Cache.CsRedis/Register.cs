/*----------------------------------------------------------------
    Copyright (C) 2020 Senparc

    文件名：Register.cs
    文件功能描述：Senparc.CO2NET.Cache.Redis 快捷注册流程


    创建标识：Senparc - 20180222

    修改标识：Senparc - 20180606
    修改描述：缓存工厂重命名为 ContainerCacheStrategyFactory

    修改标识：Senparc - 20180802
    修改描述：v3.1.0 1、Register.RegisterCacheRedis 标记为过期
                     2、新增 Register.SetConfigurationOption() 方法
                     3、新增 Register.UseKeyValueRedisNow() 方法
                     4、新增 Register.UseHashRedisNow() 方法

----------------------------------------------------------------*/

//using Senparc.CO2NET.Cache;
using Senparc.CO2NET.RegisterServices;
using System;

namespace Senparc.CO2NET.Cache.CsRedis
{
    /// <summary>
    /// Redis 注册
    /// </summary>
    public static class Register
    {
        /// <summary>
        /// 注册 Redis 缓存信息
        /// </summary>
        /// <param name="registerService">RegisterService</param>
        /// <param name="redisConfigurationString">Redis的连接字符串</param>
        /// <param name="redisObjectCacheStrategyInstance">缓存策略的委托，第一个参数为 redisConfigurationString</param>
        /// <returns></returns>
        [Obsolete("注册过程已经自动化，请改用 Register.SetConfigurationOption() 方法修改连接字符串")]
        public static IRegisterService RegisterCacheRedis(this IRegisterService registerService,
            string redisConfigurationString,
            Func<string, IBaseObjectCacheStrategy> redisObjectCacheStrategyInstance)
        {
            RedisManager.ConfigurationOption = redisConfigurationString;

            //此处先执行一次委托，直接在下方注册结果，提高每次调用的执行效率
            IBaseObjectCacheStrategy objectCacheStrategy = redisObjectCacheStrategyInstance(redisConfigurationString);
            if (objectCacheStrategy != null)
            {
                CacheStrategyFactory.RegisterObjectCacheStrategy(() => objectCacheStrategy);//Redis
            }

            return registerService;
        }

        /// <summary>
        /// 设置连接字符串（不立即启用）
        /// </summary>
        /// <param name="redisConfigurationString"></param>
        public static void SetConfigurationOption(string redisConfigurationString)
        {
            RedisManager.ConfigurationOption = redisConfigurationString;
        }

        /// <summary>
        /// 立即使用键值对方式储存的 Redis（推荐）
        /// </summary>
        public static void UseKeyValueRedisNow()
        {
            CacheStrategyFactory.RegisterObjectCacheStrategy(() => CsRedis.RedisObjectCacheStrategy.Instance);//键值Redis
        }

        /// <summary>
        /// 立即使用 HashSet 方式储存的 Redis 缓存策略
        /// </summary>
        public static void UseHashRedisNow()
        {
            CacheStrategyFactory.RegisterObjectCacheStrategy(() => CsRedis.RedisHashSetObjectCacheStrategy.Instance);//Hash格式储存的Redis
        }
    }
}
