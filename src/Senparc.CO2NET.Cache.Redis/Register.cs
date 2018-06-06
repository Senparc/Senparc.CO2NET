/*----------------------------------------------------------------
    Copyright (C) 2018 Senparc

    文件名：Register.cs
    文件功能描述：Senparc.CO2NET.Cache.Redis 快捷注册流程


    创建标识：Senparc - 20180222

    修改标识：Senparc - 20180606
    修改描述：缓存工厂重命名为 ContainerCacheStrategyFactory

----------------------------------------------------------------*/

//using Senparc.CO2NET.Cache;
using Senparc.CO2NET.RegisterServices;
using System;

namespace Senparc.CO2NET.Cache.Redis
{
    public static class Register
    {
        /// <summary>
        /// 注册 Redis 缓存信息
        /// </summary>
        /// <param name="registerService">RegisterService</param>
        /// <param name="redisConfigurationString">Redis的连接字符串</param>
        /// <param name="redisObjectCacheStrategyInstance">缓存策略的委托，第一个参数为 redisConfigurationString</param>
        /// <returns></returns>
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


    }
}
