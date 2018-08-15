/*----------------------------------------------------------------
    Copyright (C) 2018 Senparc

    文件名：BaseRedisObjectCacheStrategy.cs
    文件功能描述：所有Redis基础缓存策略的基类


    创建标识：Senparc - 20180714

    修改标识：Senparc - 20180802
    修改描述：v3.1.0 Redis 缓存服务连接信息实现从 Config.SenparcSetting 自动获取信息并注册）

----------------------------------------------------------------*/

using StackExchange.Redis;
using System;
using System.Collections.Generic;

namespace Senparc.CO2NET.Cache.Redis
{
    /// <summary>
    /// 所有Redis基础缓存策略的基类
    /// </summary>
    public abstract class BaseRedisObjectCacheStrategy : BaseCacheStrategy, IBaseObjectCacheStrategy
    {

        public ConnectionMultiplexer Client { get; set; }
        protected IDatabase _cache;

        protected BaseRedisObjectCacheStrategy()
        {
            Client = RedisManager.Manager;
            _cache = Client.GetDatabase();
        }

        static BaseRedisObjectCacheStrategy()
        {
            //自动注册连接字符串信息
            if (string.IsNullOrEmpty(RedisManager.ConfigurationOption) &&
                !string.IsNullOrEmpty(Config.SenparcSetting.Cache_Redis_Configuration) &&
                Config.SenparcSetting.Cache_Redis_Configuration != "Redis配置")
            {
                RedisManager.ConfigurationOption = Config.SenparcSetting.Cache_Redis_Configuration;
            }

            //全局初始化一次，测试结果为319ms

            //以下为测试代码
            //var manager = RedisManager.Manager;
            //var cache = manager.GetDatabase();


            //var testKey = Guid.NewGuid().ToString();
            //var testValue = Guid.NewGuid().ToString();
            //cache.StringSet(testKey, testValue);
            //var storeValue = cache.StringGet(testKey);
            //if (storeValue != testValue)
            //{
            //    throw new Exception("RedisStrategy失效，没有计入缓存！");
            //}
            //cache.StringSet(testKey, (string)null);
        }

        /// <summary>
        /// Redis 缓存策略析构函数，用于 _client 资源回收
        /// </summary>
        ~BaseRedisObjectCacheStrategy()
        {
            Client.Dispose();//释放
        }


        /// <summary>
        /// 获取 Server 对象
        /// </summary>
        /// <returns></returns>
        protected IServer GetServer()
        {
            //https://github.com/StackExchange/StackExchange.Redis/blob/master/Docs/KeysScan.md
            var server = Client.GetServer(Client.GetEndPoints()[0]);
            return server;
        }



        [Obsolete("此方法已过期，请使用 Set(TKey key, TValue value) 方法")]
        public abstract void InsertToCache(string key, object value, TimeSpan? expiry = null);
        public abstract void Set(string key, object value, TimeSpan? expiry = null, bool isFullKey = false);

        public abstract void RemoveFromCache(string key, bool isFullKey = false);

        public abstract object Get(string key, bool isFullKey = false);

        public abstract T Get<T>(string key, bool isFullKey = false);

        public abstract IDictionary<string, object> GetAll();

        public abstract bool CheckExisted(string key, bool isFullKey = false);

        public abstract long GetCount();

        public abstract void Update(string key, object value, TimeSpan? expiry = null, bool isFullKey = false);
    }
}
