/*----------------------------------------------------------------
    Copyright (C) 2020 Senparc

    文件名：Register.cs
    文件功能描述：Senparc.CO2NET.Memcached.Redis 快捷注册流程


    创建标识：Senparc - 20180222

    修改标识：Senparc - 20180606
    修改描述：缓存工厂重命名为 ContainerCacheStrategyFactory

    修改标识：Senparc - 201800802
    修改描述：v3.1.0 1、Register.RegisterCacheMemcached 标记为过期
                     2、新增 Register.SetConfigurationOption() 方法
                     3、新增 Register.UseMemcachedNow() 方法

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
        [Obsolete("注册过程已经自动化，请改用 Register.SetConfigurationOption() 方法修改连接字符串")]
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

        /// <summary>
        /// 设置连接信息（不立即启用）
        /// </summary>
        /// <param name="redisConfigurationString"></param>
        public static void SetConfigurationOption(string redisConfigurationString)
        {
            MemcachedObjectCacheStrategy.RegisterServerList(redisConfigurationString);
        }

        /// <summary>
        /// 立即使用 Memcached 缓存
        /// </summary>
        public static void UseMemcachedNow()
        {
            CacheStrategyFactory.RegisterObjectCacheStrategy(() => MemcachedObjectCacheStrategy.Instance);//Memcached
        }
    }
}
