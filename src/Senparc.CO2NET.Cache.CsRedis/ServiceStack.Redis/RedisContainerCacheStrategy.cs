using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Senparc.Weixin.Containers;
using ServiceStack.Redis;
using ServiceStack.Redis.Generic;

namespace Senparc.Weixin.Cache.Redis
{
    /// <summary>
    /// Redis container cache strategy
    /// </summary>
    public sealed class RedisContainerCacheStrategy : IContainerCacheStragegy
    {
        private IRedisClient _client;
        private IRedisTypedClient<IContainerItemCollection> _cache;

        #region 单例

        //Static SearchCache
        public static RedisContainerCacheStrategy Instance
        {
            get
            {
                return Nested.instance;//Return the static member instance in the Nested class
            }
        }

        class Nested
        {
            static Nested()
            {
            }
            //Set instance to a new initialized BaseCacheStrategy instance
            internal static readonly RedisContainerCacheStrategy instance = new RedisContainerCacheStrategy();
        }

        #endregion


        static RedisContainerCacheStrategy()
        {
            var cache = RedisManager.GetClient();

            var testKey = Guid.NewGuid().ToString();
            var testValue = Guid.NewGuid().ToString();
            cache.Set(testKey, testValue);
            var storeValue = cache.Get<string>(testKey);
            if (storeValue != testValue)
            {
                throw new Exception("RedisStrategy失效，没有计入缓存！");
            }
            cache.Remove(testKey);
        }

        public RedisContainerCacheStrategy()
        {
            //_config = RedisConfigInfo.GetConfig();
            _client = RedisManager.GetClient();
            _cache = _client.As<IContainerItemCollection>();
        }

        ~RedisContainerCacheStrategy()
        {
            _client.Dispose();//Release
            //GC.SuppressFinalize(_client);
        }

        private string GetFinalKey(string key)
        {
            return String.Format("{0}:{1}", CacheSetKey, key);
        }

        /// <summary>
        /// Get hash
        /// </summary>
        /// <returns></returns>
        private IRedisHash<string, IContainerItemCollection> GetHash()
        {
            return _cache.GetHash<string>(CacheSetKey);
        }

        #region 实现 IContainerCacheStragegy 接口

        public string CacheSetKey { get; set; }

        public bool CheckExisted(string key)
        {
            return _client.ContainsKey(key);
        }

        public IContainerItemCollection Get(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return null;
            }
            var cacheKey = GetFinalKey(key);
            var hash = GetHash();
            return _cache.GetValueFromHash(hash, cacheKey);
        }

        public IDictionary<string, IContainerItemCollection> GetAll()
        {
            var hash = GetHash();
            var list = _cache.GetHashValues(hash);//.GetAllEntriesFromHash(hash);
            var dic = new Dictionary<string, IContainerItemCollection>();
            list.ForEach(z => dic[z.CacheSetKey] = z);
            return dic;
        }

        public long GetCount()
        {
            return _client.GetAllKeys().Count(z => z.StartsWith(CacheSetKey));
        }

        public void InsertToCache(string key, IContainerItemCollection value)
        {
            if (string.IsNullOrEmpty(key) || value == null)
            {
                return;
            }

            var cacheKey = GetFinalKey(key);

            if (value is IDictionary)
            {
                //Dictionary type
            }

            //TODO: Adding an absolute expiration time will cause immediate invalidation (null upon next retrieval), a bug in lower versions of memcache
            var hash = GetHash();
            _cache.SetEntryInHash(hash, cacheKey, value);
            //_cache.SetEntry(cacheKey, obj);

#if DEBUG
            var value1 = _cache.GetFromHash(cacheKey);//Normally can get //_cache.GetValue(cacheKey);
            var value2 = _cache.GetValueFromHash(hash, cacheKey);//Normally can get
            var value3 = _cache.GetValue(cacheKey);//null
#endif
        }

        public void RemoveFromCache(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return;
            }
            var cacheKey = GetFinalKey(key);
            var hash = GetHash();
            _cache.RemoveEntryFromHash(hash, cacheKey);
        }

        public void Update(string key, IContainerItemCollection value)
        {
            var hash = GetHash();
            _cache.SetEntryInHash(hash, key, value);
        }

        public void UpdateContainerBag(string key, IBaseContainerBag containerBag)
        {
            if (this.CheckExisted(key))
            {
                var containerItemCollection = Get(key);
                containerItemCollection[containerBag.Key] = containerBag;

                var hash = GetHash();
                _cache.SetEntryInHash(hash, key, containerItemCollection);
            }
        }

        #endregion
    }
}
