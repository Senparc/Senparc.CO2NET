/*----------------------------------------------------------------
    Copyright (C) 2025 Senparc

    FileName：BaseRedisObjectCacheStrategy.cs
    File Function Description：Base class for all Redis basic cache strategies


    Creation Identifier：Senparc - 20180714

    Modification Identifier：Senparc - 20180802
    Modification Description：v3.1.0 Redis cache service connection information is automatically obtained from Config.SenparcSetting and registered

    Modification Identifier：Senparc - 20190413
    Modification Description：v3.5.0 Provides asynchronous cache interface

    ======== Transplanted from Senparc.CO2NET.Cache.Redis ========

    Modification Identifier：Senparc - 20210901
    Modification Description：v0.5.1 Destructor performs null value check

----------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Senparc.CO2NET.Cache.CsRedis
{
    /// <summary>
    /// Base class for all Redis basic cache strategies
    /// </summary>
    public abstract class BaseRedisObjectCacheStrategy : BaseCacheStrategy, IBaseObjectCacheStrategy
    {
        public CSRedis.CSRedisClient Client { get; set; }

        protected BaseRedisObjectCacheStrategy()
        {
            Client = new CSRedis.CSRedisClient(Config.SenparcSetting.Cache_Redis_Configuration);
        }

        static BaseRedisObjectCacheStrategy()
        {
            //Automatically register connection string information
            if (string.IsNullOrEmpty(RedisManager.ConfigurationOption) &&
                !string.IsNullOrEmpty(Config.SenparcSetting.Cache_Redis_Configuration) &&
                Config.SenparcSetting.Cache_Redis_Configuration != "Redis配置" &&
                Config.SenparcSetting.Cache_Redis_Configuration != "#{Cache_Redis_Configuration}#")
            {
                RedisManager.ConfigurationOption = Config.SenparcSetting.Cache_Redis_Configuration;
            }

            //Global initialization once, test result is 319ms

            //The following is the test code
            //var manager = RedisManager.Manager;
            //var cache = manager.GetDatabase();


            //var testKey = Guid.NewGuid().ToString();
            //var testValue = Guid.NewGuid().ToString();
            //cache.StringSet(testKey, testValue);
            //var storeValue = cache.StringGet(testKey);
            //if (storeValue != testValue)
            //{
            //    throw new Exception("RedisStrategy failed, not cached!");
            //}
            //cache.StringSet(testKey, (string)null);
        }

        /// <summary>
        /// Redis cache strategy destructor for _client resource recovery
        /// </summary>
        ~BaseRedisObjectCacheStrategy()
        {
            Client?.Dispose();//Release
        }


        #region Synchronous Methods


        [Obsolete("此方法已过期，请使用 Set(TKey key, TValue value) 方法", true)]
        public abstract void InsertToCache(string key, object value, TimeSpan? expiry = null);
        public abstract void Set(string key, object value, TimeSpan? expiry = null, bool isFullKey = false);

        public abstract void RemoveFromCache(string key, bool isFullKey = false);

        public abstract object Get(string key, bool isFullKey = false);

        public abstract T Get<T>(string key, bool isFullKey = false);

        public abstract IDictionary<string, object> GetAll();

        public abstract bool CheckExisted(string key, bool isFullKey = false);

        public abstract long GetCount();
        public abstract long GetCount(string prefix);

        public abstract void Update(string key, object value, TimeSpan? expiry = null, bool isFullKey = false);


        #endregion

        #region Asynchronous Methods

        public abstract Task SetAsync(string key, object value, TimeSpan? expiry = null, bool isFullKey = false);

        public abstract Task RemoveFromCacheAsync(string key, bool isFullKey = false);

        public abstract Task<object> GetAsync(string key, bool isFullKey = false);

        public abstract Task<T> GetAsync<T>(string key, bool isFullKey = false);

        public abstract Task<IDictionary<string, object>> GetAllAsync();

        public abstract Task<bool> CheckExistedAsync(string key, bool isFullKey = false);

        public abstract Task<long> GetCountAsync();
        public abstract Task<long> GetCountAsync(string prefix);

        public abstract Task UpdateAsync(string key, object value, TimeSpan? expiry = null, bool isFullKey = false);

        #endregion


        #region Synchronous Lock

        public override ICacheLock BeginCacheLock(string resourceName, string key, int retryCount = 0, TimeSpan retryDelay = new TimeSpan())
        {
            return RedisCacheLock.CreateAndLock(this, resourceName, key, retryCount, retryDelay);
        }

        public override async Task<ICacheLock> BeginCacheLockAsync(string resourceName, string key, int retryCount = 0, TimeSpan retryDelay = new TimeSpan())
        {
            return await RedisCacheLock.CreateAndLockAsync(this, resourceName, key, retryCount, retryDelay).ConfigureAwait(false);
        }

        #endregion
    }
}
