using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            //全局初始化一次，测试结果为319ms

            var manager = RedisManager.Manager;
            var cache = manager.GetDatabase();


            var testKey = Guid.NewGuid().ToString();
            var testValue = Guid.NewGuid().ToString();
            cache.StringSet(testKey, testValue);
            var storeValue = cache.StringGet(testKey);
            if (storeValue != testValue)
            {
                throw new Exception("RedisStrategy失效，没有计入缓存！");
            }
            cache.StringSet(testKey, (string)null);
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



        public abstract void InsertToCache(string key, object value);
        public abstract void Set(string key, object value);

        public abstract void RemoveFromCache(string key, bool isFullKey = false);

        public abstract object Get(string key, bool isFullKey = false);

        public abstract T Get<T>(string key, bool isFullKey = false);

        public abstract IDictionary<string, object> GetAll();

        public abstract bool CheckExisted(string key, bool isFullKey = false);

        public abstract long GetCount();

        public abstract void Update(string key, object value, bool isFullKey = false);
    }
}
