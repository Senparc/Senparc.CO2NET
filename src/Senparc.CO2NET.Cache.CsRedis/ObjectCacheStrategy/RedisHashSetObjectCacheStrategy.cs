#region Apache License Version 2.0
/*----------------------------------------------------------------

Copyright 2024 Suzhou Senparc Network Technology Co.,Ltd.

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
    Copyright (C) 2020 Senparc

    FileName: RedisObjectCacheStrategy.cs
    File Function Description: Redis Object type container cache (Key is of String type).


    Creation Identifier: Senparc - 20161024

    Modification Identifier: Senparc - 20170205
    Modification Description: v0.2.0 Refactor distributed lock

    Modification Identifier: Senparc - 20170205
    Modification Description: v3.0.0 RedisObjectCacheStrategy renamed to RedisHashSetObjectCacheStrategy, separating HashSet data structure
      1. Separate HashSet and Key-Value two different formats of cache: RedisHashSetObjectCacheStrategy and RedisObjectCacheStrategy
      2. Provide new cache expiration strategy

    Modification Identifier: Senparc - 20190418
    Modification Description: v3.5.0 Provide HashGetAllAsync() asynchronous method

    Modification Identifier: Senparc - 20190914
    Modification Description: v3.5.4 fix bug: GetServer().Keys() method adds database index value

    Modification Identifier: Senparc - 20230527
    Modification Description: v1.1.4 RedisHashSetObjectCacheStrategy.Get() method adds pure string judgment

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
    /// Redis Object type container cache (Key is of String type)
    /// </summary>
    public class RedisHashSetObjectCacheStrategy : BaseRedisObjectCacheStrategy
    //where TContainerBag : class, IBaseContainerBag, new()
    {
        /// <summary>
        /// Key and Field collection stored in Hash
        /// </summary>
        protected class HashKeyAndField
        {
            public string Key { get; set; }
            public string Field { get; set; }
        }

        #region 单例

        /// <summary>
        /// Redis cache strategy
        /// </summary>
        RedisHashSetObjectCacheStrategy() : base()
        {

        }

        //Static SearchCache
        public static RedisHashSetObjectCacheStrategy Instance
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
            internal static readonly RedisHashSetObjectCacheStrategy instance = new RedisHashSetObjectCacheStrategy();
        }

        #endregion


        /// <summary>
        /// Get Key and Field stored in Hash
        /// </summary>
        /// <param name="key"></param>
        /// <param name="isFullKey"></param>
        /// <returns></returns>
        protected HashKeyAndField GetHashKeyAndField(string key, bool isFullKey = false)
        {
            var finalFullKey = base.GetFinalKey(key, isFullKey);
            var index = finalFullKey.LastIndexOf(":");

            if (index == -1)
            {
                index = 0;
            }

            var hashKeyAndField = new HashKeyAndField()
            {
                Key = finalFullKey.Substring(0, index),
                Field = finalFullKey.Substring(index + 1/*Exclude colon*/, finalFullKey.Length - index - 1)
            };
            return hashKeyAndField;
        }

        #region 实现 IBaseObjectCacheStrategy 接口

        #region 同步方法


        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="isFullKey">Whether it is already a complete Key</param>
        /// <returns></returns>
        public override bool CheckExisted(string key, bool isFullKey = false)
        {
            //var cacheKey = GetFinalKey(key, isFullKey);
            var hashKeyAndField = this.GetHashKeyAndField(key, isFullKey);

            //return _cache.KeyExists(cacheKey);
            return base.Client.HExists(hashKeyAndField.Key, hashKeyAndField.Field);
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
                //InsertToCache(key, new ContainerItemCollection());
            }

            //var cacheKey = GetFinalKey(key, isFullKey);
            var hashKeyAndField = this.GetHashKeyAndField(key, isFullKey);

            //var value = _cache.StringGet(cacheKey);
            var value = base.Client.HGet(hashKeyAndField.Key, hashKeyAndField.Field);
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

            //var cacheKey = GetFinalKey(key, isFullKey);
            var hashKeyAndField = this.GetHashKeyAndField(key, isFullKey);

            //var value = _cache.StringGet(cacheKey);
            var value = base.Client.HGet(hashKeyAndField.Key, hashKeyAndField.Field);
            if (value != null)
            {
                return value.ToString().DeserializeFromCache<T>();
            }

            return default(T);
        }



        //public IDictionary<string, TBag> GetAll<TBag>() where TBag : IBaseContainerBag
        //{
        //    #region Old method (before using Hash)

        //    //var itemCacheKey = ContainerHelper.GetItemCacheKey(typeof(TBag), "*");   
        //    ////var keyPattern = string.Format("*{0}", itemCacheKey);
        //    //var keyPattern = GetFinalKey(itemCacheKey);

        //    //var keys = GetServer().Keys(pattern: keyPattern);
        //    //var dic = new Dictionary<string, TBag>();
        //    //foreach (var redisKey in keys)
        //    //{
        //    //    try
        //    //    {
        //    //        var bag = Get(redisKey, true);
        //    //        dic[redisKey] = (TBag)bag;
        //    //    }
        //    //    catch (Exception)
        //    //    {

        //    //    }

        //    //}

        //    #endregion

        //    var key = ContainerHelper.GetItemCacheKey(typeof(TBag), "");
        //    key = key.Substring(0, key.Length - 1);//Remove colon
        //    key = GetFinalKey(key);//Get Key with Senparc:DefaultCache: prefix ([DefaultCache] configurable)

        //    var list = _cache.HashGetAll(key);
        //    var dic = new Dictionary<string, TBag>();

        //    foreach (var hashEntry in list)
        //    {
        //        var fullKey = key + ":" + hashEntry.Name;//Most complete finalKey (can be used for LocalCache), restore complete Key, format: [namespace]:[Key]
        //        dic[fullKey] = StackExchangeRedisExtensions.Deserialize<TBag>(hashEntry.Value);
        //    }

        //    return dic;
        //}

        /// <summary>
        /// Note: The object obtained by this method is directly stored in the cache, serialized Value (up to 99999 items)
        /// </summary>
        /// <returns></returns>
        public override IDictionary<string, object> GetAll()
        {
            var keyPrefix = GetFinalKey("");//Senparc:DefaultCache: prefix Key ([DefaultCache] configurable)
            var dic = new Dictionary<string, object>();

            var hashKeys = base.Client.Keys(/*database: Client.GetDatabase().Database,*/ pattern: keyPrefix + "*"/*, pageSize: 99999*/);
            foreach (var redisKey in hashKeys)
            {
                var list = base.Client.HGetAll(redisKey);

                foreach (var hashEntry in list)
                {
                    var fullKey = redisKey.ToString() + ":" + hashEntry.Key;//Most complete finalKey (can be used for LocalCache), restore complete Key, format: [namespace]:[Key]
                    dic[fullKey] = hashEntry.Value.ToString().DeserializeFromCache();
                }
            }
            return dic;
        }

        /// <summary>
        /// Get all cache item counts
        /// </summary>
        /// <returns></returns>

        public override long GetCount()
        {
            return GetCount(null);
        }

        /// <summary>
        /// Get all cache item counts
        /// </summary>
        /// <returns></returns>

        public override long GetCount(string prefix)
        {
            var keyPattern = GetFinalKey(prefix + "*");//Get Key with Senparc:DefaultCache: prefix ([DefaultCache]         
            var count = base.Client.Keys(/*database: Client.GetDatabase().Database,*/ pattern: keyPattern/*, pageSize: 99999*/).Count();
            return count;
        }

        /// <summary>
        /// Insert object. Note: Expiration time is invalid for HashSet!
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="expiry"></param>
        [Obsolete("此方法已过期，请使用 Set(TKey key, TValue value) 方法", true)]
        public override void InsertToCache(string key, object value, TimeSpan? expiry = null)
        {
            Set(key, value, expiry, false);
        }

        /// <summary>
        /// Set object. Note: Expiration time is invalid for HashSet!
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="isFullKey"></param>
        /// <param name="expiry"></param>
        public override void Set(string key, object value, TimeSpan? expiry = null, bool isFullKey = false)
        {
            if (string.IsNullOrEmpty(key) || value == null)
            {
                return;
            }

            //var cacheKey = GetFinalKey(key);
            var hashKeyAndField = this.GetHashKeyAndField(key, isFullKey);

            //if (value is IDictionary)
            //{
            //    //Dictionary type
            //}

            //_cache.StringSet(cacheKey, value.Serialize());
            //_cache.HashSet(hashKeyAndField.Key, hashKeyAndField.Field, value.Serialize());


            //StackExchangeRedisExtensions.Serialize is very inefficient
            //_cache.HashSet(hashKeyAndField.Key, hashKeyAndField.Field, StackExchangeRedisExtensions.Serialize(value));

            var json = value.SerializeToCache();
            base.Client.HSet(hashKeyAndField.Key, hashKeyAndField.Field, json);

            //#if DEBUG
            //            var value1 = _cache.HashGet(hashKeyAndField.Key, hashKeyAndField.Field);//Normally can get //_cache.GetValue(cacheKey);
            //#endif
        }

        public override void RemoveFromCache(string key, bool isFullKey = false)
        {
            if (string.IsNullOrEmpty(key))
            {
                return;
            }

            //var cacheKey = GetFinalKey(key, isFullKey);
            var hashKeyAndField = this.GetHashKeyAndField(key);

            SenparcMessageQueue.OperateQueue();//Delayed cache takes effect immediately
            //_cache.KeyDelete(cacheKey);//Delete key
            base.Client.HDel(hashKeyAndField.Key, hashKeyAndField.Field);//Delete item
        }

        /// <summary>
        /// Update object. Note: Expiration time is invalid for HashSet!
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="isFullKey"></param>
        /// <param name="expiry"></param>
        public override void Update(string key, object value, TimeSpan? expiry = null, bool isFullKey = false)
        {
            Set(key, value, expiry, isFullKey);
        }


        #endregion


        #region 异步方法

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="isFullKey">Whether it is already a complete Key</param>
        /// <returns></returns>
        public override async Task<bool> CheckExistedAsync(string key, bool isFullKey = false)
        {
            //var cacheKey = GetFinalKey(key, isFullKey);
            var hashKeyAndField = this.GetHashKeyAndField(key, isFullKey);

            //return _cache.KeyExists(cacheKey);
            return await base.Client.HExistsAsync(hashKeyAndField.Key, hashKeyAndField.Field).ConfigureAwait(false);
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
                //InsertToCache(key, new ContainerItemCollection());
            }

            //var cacheKey = GetFinalKey(key, isFullKey);
            var hashKeyAndField = this.GetHashKeyAndField(key, isFullKey);

            //var value = _cache.StringGet(cacheKey);
            var value = await base.Client.HGetAsync(hashKeyAndField.Key, hashKeyAndField.Field).ConfigureAwait(false);
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

            //var cacheKey = GetFinalKey(key, isFullKey);
            var hashKeyAndField = this.GetHashKeyAndField(key, isFullKey);

            //var value = _cache.StringGet(cacheKey);
            var value = await base.Client.HGetAsync(hashKeyAndField.Key, hashKeyAndField.Field).ConfigureAwait(false);
            if (value != null)
            {
                return value.ToString().DeserializeFromCache<T>();
            }

            return default(T);
        }

        /// <summary>
        /// Note: The object obtained by this method is directly stored in the cache, serialized Value (up to 99999 items)
        /// </summary>
        /// <returns></returns>
        public override async Task<IDictionary<string, object>> GetAllAsync()
        {
            var keyPrefix = GetFinalKey("");//Senparc:DefaultCache: prefix Key ([DefaultCache] configurable)
            var dic = new Dictionary<string, object>();

            var hashKeys = base.Client.Keys(/*database: Client.GetDatabase().Database,*/ pattern: keyPrefix + "*"/*, pageSize: 99999*/);
            foreach (var redisKey in hashKeys)
            {
                var list = await base.Client.HGetAllAsync(redisKey).ConfigureAwait(false);

                foreach (var hashEntry in list)
                {
                    var fullKey = redisKey.ToString() + ":" + hashEntry.Key;//Most complete finalKey (can be used for LocalCache), restore complete Key, format: [namespace]:[Key]
                    dic[fullKey] = hashEntry.Value.ToString().DeserializeFromCache();
                }
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

        /// <summary>
        /// Set object. Note: Expiration time is invalid for HashSet!
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="isFullKey"></param>
        /// <param name="expiry"></param>
        public override async Task SetAsync(string key, object value, TimeSpan? expiry = null, bool isFullKey = false)
        {
            if (string.IsNullOrEmpty(key) || value == null)
            {
                return;
            }

            //var cacheKey = GetFinalKey(key);
            var hashKeyAndField = this.GetHashKeyAndField(key, isFullKey);

            //if (value is IDictionary)
            //{
            //    //Dictionary type
            //}

            //_cache.StringSet(cacheKey, value.Serialize());
            //_cache.HashSet(hashKeyAndField.Key, hashKeyAndField.Field, value.Serialize());


            //StackExchangeRedisExtensions.Serialize is very inefficient
            //_cache.HashSet(hashKeyAndField.Key, hashKeyAndField.Field, StackExchangeRedisExtensions.Serialize(value));

            var json = value.SerializeToCache();
            await base.Client.HSetAsync(hashKeyAndField.Key, hashKeyAndField.Field, json).ConfigureAwait(false);

            //#if DEBUG
            //            var value1 = _cache.HashGet(hashKeyAndField.Key, hashKeyAndField.Field);//Normally can get //_cache.GetValue(cacheKey);
            //#endif
        }

        public override async Task RemoveFromCacheAsync(string key, bool isFullKey = false)
        {
            if (string.IsNullOrEmpty(key))
            {
                return;
            }

            //var cacheKey = GetFinalKey(key, isFullKey);
            var hashKeyAndField = this.GetHashKeyAndField(key);

            SenparcMessageQueue.OperateQueue();//Delayed cache takes effect immediately
                                               //_cache.KeyDelete(cacheKey);//Delete key
            await base.Client.HDelAsync(hashKeyAndField.Key, hashKeyAndField.Field).ConfigureAwait(false);//Delete item
        }

        /// <summary>
        /// Update object. Note: Expiration time is invalid for HashSet!
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="isFullKey"></param>
        /// <param name="expiry"></param>
        public override async Task UpdateAsync(string key, object value, TimeSpan? expiry = null, bool isFullKey = false)
        {
            await SetAsync(key, value, expiry, isFullKey).ConfigureAwait(false);
        }

        #endregion

        #endregion


        /// <summary>
        /// _cache.HashGetAll()
        /// </summary>
        public Dictionary<string, string> HashGetAll(string key)
        {
            return base.Client.HGetAll(key);
        }


        /// <summary>
        /// _cache.HashGetAll()
        /// </summary>
        public async Task<Dictionary<string, string>> HashGetAllAsync(string key)
        {
            return await base.Client.HGetAllAsync(key).ConfigureAwait(false);
        }
    }
}
