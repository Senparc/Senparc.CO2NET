#region Apache License Version 2.0
/*----------------------------------------------------------------

Copyright 2019 Suzhou Senparc Network Technology Co.,Ltd.

Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file
except in compliance with the License. You may obtain a copy of the License at

http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software distributed under the
License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND,
either express or implied. See the License for the specific language governing permissions
and limitations under the License.

Detail: https://github.com/Senparc/Senparc.CO2NET/blob/master/LICENSE

----------------------------------------------------------------*/
#endregion Apache License Version 2.0

/*----------------------------------------------------------------
    Copyright (C) 2025 Senparc

    FileName：RedisObjectCacheStrategy.cs
    File Function Description：Redis Object type container cache (Key is String type).


    Creation Identifier：Senparc - 20161024

    Modification Identifier：Senparc - 20170205
    Modification Description：v0.2.0 Refactor distributed lock

    --CO2NET--

    Modification Identifier：Senparc - 20180714
    Modification Description：v3.0.0 Changed to Key-Value implementation

    Modification Identifier：Senparc - 20180715
    Modification Description：v3.0.1 Added GetAllByPrefix() method

    Modification Identifier：Senparc - 20190418
    Modification Description：v3.5.0.1 Added GetAllByPrefixAsync() method

    Modification Identifier：Senparc - 20190914
    Modification Description：v3.5.4 fix bug: GetServer().Keys() method added database index value

    Modification Identifier：Senparc - 20230527
    Modification Description：v1.1.4 RedisObjectCacheStrategy.Get() method added pure string judgment

    Modification Identifier：Senparc - 20240910
    Modification Description：v1.2.5 Fixed GetAllByPrefixAsync(key) method automatically fetching all Keys bug
 ----------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Linq;
using Senparc.CO2NET.Helpers;
using Senparc.CO2NET.MessageQueue;
using Senparc.CO2NET.Cache;
using Senparc.CO2NET.Trace;
using System.Threading.Tasks;

namespace Senparc.CO2NET.Cache.CsRedis
{
    /// <summary>
    /// Redis Object type container cache (Key is String type), Key-Value type storage
    /// </summary>
    public class RedisObjectCacheStrategy : BaseRedisObjectCacheStrategy
    {
        #region Singleton

        /// <summary>
        /// Redis cache strategy
        /// </summary>
        RedisObjectCacheStrategy() : base()
        {

        }

        //Static SearchCache
        public static RedisObjectCacheStrategy Instance
        {
            get
            {
                return Nested.instance;//Return static member instance in Nested class
            }
        }

        class Nested
        {
            static Nested()
            {
            }
            //Set instance to a new initialized BaseCacheStrategy instance
            internal static readonly RedisObjectCacheStrategy instance = new RedisObjectCacheStrategy();
        }

        #endregion

        /// <summary>
        /// Get expiration time (seconds)
        /// </summary>
        /// <param name="expiry"></param>
        /// <returns></returns>
        private int GetExpirySeconds(TimeSpan? expiry)
        {
            var expirySeconds = expiry.HasValue ? (int)Math.Ceiling(expiry.Value.TotalSeconds) : -1;
            return expirySeconds;
        }


        #region Implement IBaseObjectCacheStrategy interface

        #region Synchronous interface

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="isFullKey">Whether it is already a complete Key</param>
        /// <returns></returns>
        public override bool CheckExisted(string key, bool isFullKey = false)
        {
            var cacheKey = GetFinalKey(key, isFullKey);
            return base.Client.Exists(cacheKey);
        }

        public override object Get(string key, bool isFullKey = false)
        {
            if (string.IsNullOrEmpty(key))
            {
                return null;
            }

            if (!CheckExisted(key, isFullKey))
            {
                return null;
            }

            var cacheKey = GetFinalKey(key, isFullKey);

            var value = base.Client.Get(cacheKey);
            if (value != null)
            {
                try
                {
                    return value.ToString().DeserializeFromCache();
                }
                catch
                {
                    return value.ToString();
                }
            }
            return value;
        }

        public override T Get<T>(string key, bool isFullKey = false)
        {
            if (string.IsNullOrEmpty(key))
            {
                return default(T);
            }

            if (!CheckExisted(key, isFullKey))
            {
                return default(T);
                //InsertToCache(key, new ContainerItemCollection());
            }

            var cacheKey = GetFinalKey(key, isFullKey);

            var value = base.Client.Get(cacheKey);
            if (value != null)
            {
                return value.ToString().DeserializeFromCache<T>();
            }

            return default(T);
        }

        /// <summary>
        /// Note: The object obtained by this method is the value directly stored in the cache after serialization
        /// </summary>
        /// <returns></returns>
        public override IDictionary<string, object> GetAll()
        {
            var keyPrefix = GetFinalKey("");//Get Key with Senparc:DefaultCache: prefix ([DefaultCache] configurable)
            var dic = new Dictionary<string, object>();


            var keys = base.Client.Keys(/*database: Client.GetDatabase().Database,*/ pattern: keyPrefix + "*"/*, pageSize: 99999*/);
            foreach (var redisKey in keys)
            {
                dic[redisKey] = Get(redisKey, true);
            }
            return dic;
        }

        //TODO: Provide GetAllKeys() method


        /// <summary>
        /// Get all cache item count
        /// </summary>
        /// <returns></returns>

        public override long GetCount()
        {
            return GetCount(null);
        }

        /// <summary>
        /// Get all cache item count
        /// </summary>
        /// <returns></returns>

        public override long GetCount(string prefix)
        {
            var keyPattern = GetFinalKey(prefix + "*");//Get Key with Senparc:DefaultCache: prefix ([DefaultCache]         
            var count = base.Client.Keys(/*database: Client.GetDatabase().Database,*/ pattern: keyPattern/*, pageSize: 99999*/).Count();
            return count;
        }

        [Obsolete("此方法已过期，请使用 Set(TKey key, TValue value) 方法", true)]
        public override void InsertToCache(string key, object value, TimeSpan? expiry = null)
        {
            Set(key, value, expiry, false);
        }

        public override void Set(string key, object value, TimeSpan? expiry = null, bool isFullKey = false)
        {
            if (string.IsNullOrEmpty(key) || value == null)
            {
                return;
            }

            var cacheKey = GetFinalKey(key, isFullKey);

            var json = value.SerializeToCache();
            base.Client.Set(cacheKey, json, GetExpirySeconds(expiry));
        }

        public override void RemoveFromCache(string key, bool isFullKey = false)
        {
            if (string.IsNullOrEmpty(key))
            {
                return;
            }

            var cacheKey = GetFinalKey(key, isFullKey);

            SenparcMessageQueue.OperateQueue();//Delayed cache takes effect immediately
            base.Client.Del(cacheKey);//Delete key
        }

        public override void Update(string key, object value, TimeSpan? expiry = null, bool isFullKey = false)
        {
            Set(key, value, expiry, isFullKey);
        }

        #endregion


        #region Asynchronous methods

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="isFullKey">Whether it is already a complete Key</param>
        /// <returns></returns>
        public override async Task<bool> CheckExistedAsync(string key, bool isFullKey = false)
        {
            var cacheKey = GetFinalKey(key, isFullKey);
            return await base.Client.ExistsAsync(cacheKey).ConfigureAwait(false);
        }

        public override async Task<object> GetAsync(string key, bool isFullKey = false)
        {
            if (string.IsNullOrEmpty(key))
            {
                return null;
            }

            if (!await CheckExistedAsync(key, isFullKey).ConfigureAwait(false))
            {
                return null;
            }

            var cacheKey = GetFinalKey(key, isFullKey);

            var value = await base.Client.GetAsync(cacheKey).ConfigureAwait(false);
            if (value != null)
            {
                try
                {
                    return value.ToString().DeserializeFromCache();
                }
                catch
                {
                    return value.ToString();
                }
            }
            return value;
        }

        public override async Task<T> GetAsync<T>(string key, bool isFullKey = false)
        {
            if (string.IsNullOrEmpty(key))
            {
                return default(T);
            }

            if (!await CheckExistedAsync(key, isFullKey).ConfigureAwait(false))
            {
                return default(T);
                //InsertToCache(key, new ContainerItemCollection());
            }

            var cacheKey = GetFinalKey(key, isFullKey);

            var value = await base.Client.GetAsync(cacheKey).ConfigureAwait(false);
            if (value != null)
            {
                return value.ToString().DeserializeFromCache<T>();
            }

            return default(T);
        }

        /// <summary>
        /// Note: The object obtained by this method is the value directly stored in the cache after serialization (up to 99999 items)
        /// </summary>
        /// <returns></returns>
        public override async Task<IDictionary<string, object>> GetAllAsync()
        {
            var keyPrefix = GetFinalKey("");//Get Key with Senparc:DefaultCache: prefix ([DefaultCache] configurable)
            var dic = new Dictionary<string, object>();

            var keys = base.Client.Keys(/*database: Client.GetDatabase().Database,*/ pattern: keyPrefix + "*"/*, pageSize: 99999*/);
            foreach (var redisKey in keys)
            {
                dic[redisKey] = await GetAsync(redisKey, true).ConfigureAwait(false);
            }
            return dic;
        }


        public override Task<long> GetCountAsync()
        {
            return Task.Factory.StartNew(() => GetCount());
        }
        public override Task<long> GetCountAsync(string prefix)
        {
            return Task.Factory.StartNew(() => GetCount(prefix));
        }
        public override async Task SetAsync(string key, object value, TimeSpan? expiry = null, bool isFullKey = false)
        {
            if (string.IsNullOrEmpty(key) || value == null)
            {
                return;
            }

            var cacheKey = GetFinalKey(key, isFullKey);

            var json = value.SerializeToCache();
            await base.Client.SetAsync(cacheKey, json, GetExpirySeconds(expiry)).ConfigureAwait(false);
        }

        public override async Task RemoveFromCacheAsync(string key, bool isFullKey = false)
        {
            if (string.IsNullOrEmpty(key))
            {
                return;
            }

            var cacheKey = GetFinalKey(key, isFullKey);

            SenparcMessageQueue.OperateQueue();//Delayed cache takes effect immediately
            await base.Client.DelAsync(cacheKey).ConfigureAwait(false);//Delete key
        }

        public override async Task UpdateAsync(string key, object value, TimeSpan? expiry = null, bool isFullKey = false)
        {
            await SetAsync(key, value, expiry, isFullKey).ConfigureAwait(false);
        }

        #endregion

        #endregion

        /// <summary>
        /// Get object list by key prefix (up to 99999 items)
        /// </summary>
        public IList<T> GetAllByPrefix<T>(string key)
        {
            var keyPattern = GetFinalKey(key + "*");//Get Key with Senparc:DefaultCache: prefix ([DefaultCache]         
            var keys = base.Client.Keys(/*database: Client.GetDatabase().Database,*/ pattern: keyPattern/*, pageSize: 99999*/);
            List<T> list = new List<T>();
            foreach (var fullKey in keys)
            {
                var obj = Get<T>(fullKey, true);
                if (obj != null)
                {
                    list.Add(obj);
                }
            }

            return list;
        }


        /// <summary>
        /// [Async method] Get object list by key prefix (up to 99999 items)
        /// </summary>
        public async Task<IList<T>> GetAllByPrefixAsync<T>(string key)
        {
            var keyPattern = GetFinalKey(key + "*");//Get Key with Senparc:DefaultCache: prefix ([DefaultCache]         
            var keys = base.Client.Keys(/*database: Client.GetDatabase().Database,*/ pattern: keyPattern/*, pageSize: 99999*/);
            List<T> list = new List<T>();
            foreach (var fullKey in keys)
            {
                var obj = await GetAsync<T>(fullKey, true).ConfigureAwait(false);
                if (obj != null)
                {
                    list.Add(obj);
                }
            }

            return list;
        }
    }
}
