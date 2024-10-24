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
    Copyright (C) 2024 Senparc

    FileName：IBaseCacheStrategy.cs
    File Function Description：Cache strategy interface.


    Creation Identifier：Senparc - 20160308

    Modification Identifier：Senparc - 20160812
    Modification Description：v4.7.4 Fixed the issue where Container could not be registered

    --CO2NET--
    
    Modification Identifier：Senparc - 20190411
    Modification Description：v0.6.0 Provided asynchronous cache interface

 ----------------------------------------------------------------*/


using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Senparc.CO2NET.Cache
{
    /// <summary>
    /// The most basic cache strategy interface
    /// </summary>
    public interface IBaseCacheStrategy
    {
        ///// <summary>
        ///// Key for the entire Cache collection
        ///// </summary>
        //string CacheSetKey { get; set; }

        /// <summary>
        /// Create a (distributed) lock
        /// </summary>
        /// <param name="resourceName">Resource name</param>
        /// <param name="key">Key identifier</param>
        /// <param name="retryCount">Retry count</param>
        /// <param name="retryDelay">Retry delay</param>
        /// <returns></returns>
        ICacheLock BeginCacheLock(string resourceName, string key, int retryCount = 0, TimeSpan retryDelay = new TimeSpan());

        /// <summary>
        /// [Asynchronous method] Create a (distributed) lock
        /// </summary>
        /// <param name="resourceName">Resource name</param>
        /// <param name="key">Key identifier</param>
        /// <param name="retryCount">Retry count</param>
        /// <param name="retryDelay">Retry delay</param>
        /// <returns></returns>
        Task<ICacheLock> BeginCacheLockAsync(string resourceName, string key, int retryCount = 0, TimeSpan retryDelay = new TimeSpan());
    }

    /// <summary>
    /// Public cache strategy interface
    /// </summary>
    public interface IBaseCacheStrategy<TKey, TValue> /*: IBaseCacheLock*/
    //where TValue : class
    {
        /// <summary>
        /// Get the final key in the cache, such as Container suggested format: return String.Format("{0}:{1}", "SenparcWeixinContainer", key);
        /// </summary>
        /// <param name="key"></param>
        /// <param name="isFullKey">Whether it is already a full key, if not, the GetFinalKey() method will be called once</param>
        /// <returns></returns>
        string GetFinalKey(string key, bool isFullKey = false);

        #region Synchronous methods


        /// <summary>
        /// Add an object with the specified ID
        /// </summary>
        /// <param name="key">Cache key</param>
        /// <param name="value">Cache value</param>
        /// <param name="expiry">Expiration time</param>
        [Obsolete("此方法已过期，请使用 Set(TKey key, TValue value) 方法", true)]
        void InsertToCache(TKey key, TValue value, TimeSpan? expiry = null);

        /// <summary>
        /// Add an object with the specified ID
        /// </summary>
        /// <param name="key">Cache key</param>
        /// <param name="value">Cache value</param>
        /// <param name="isFullKey">Whether it is already a full key, if not, the GetFinalKey() method will be called once</param>
        /// <param name="expiry">Expiration time</param>
        void Set(TKey key, TValue value, TimeSpan? expiry = null, bool isFullKey = false);

        /// <summary>
        /// Remove the object with the specified cache key
        /// </summary>
        /// <param name="key">Cache key</param>
        /// <param name="isFullKey">Whether it is already a full key, if not, the GetFinalKey() method will be called once</param>
        void RemoveFromCache(TKey key, bool isFullKey = false);

        /// <summary>
        /// Return the object with the specified cache key
        /// </summary>
        /// <param name="key">Cache key</param>
        /// <param name="isFullKey">Whether it is already a full key, if not, the GetFinalKey() method will be called once</param>
        /// <returns></returns>
        TValue Get(TKey key, bool isFullKey = false);

        /// <summary>
        /// Return the object with the specified cache key and force the specified type
        /// </summary>
        /// <param name="key">Cache key</param>
        /// <param name="isFullKey">Whether it is already a full key, if not, the GetFinalKey() method will be called once</param>
        /// <returns></returns>
        T Get<T>(TKey key, bool isFullKey = false);

        /// <summary>
        /// Get all cache information collections
        /// </summary>
        /// <returns></returns>
        IDictionary<TKey, TValue> GetAll();

        /// <summary>
        /// Check if the Key and object exist
        /// </summary>
        /// <param name="key">Cache key</param>
        /// <param name="isFullKey">Whether it is already a full key, if not, the GetFinalKey() method will be called once</param>
        /// <returns></returns>
        bool CheckExisted(TKey key, bool isFullKey = false);

        /// <summary>
        /// Get the total number of cache collections (Note: The counting objects of each cache framework may not be consistent!)
        /// </summary>
        /// <returns></returns>
        long GetCount();

        /// <summary>
        /// Update the cache
        /// </summary>
        /// <param name="key">Cache key</param>
        /// <param name="value">Cache value</param>
        /// <param name="isFullKey">Whether it is already a full key, if not, the GetFinalKey() method will be called once</param>
        /// <param name="expiry">Expiration time</param>
        void Update(TKey key, TValue value, TimeSpan? expiry = null, bool isFullKey = false);

        #endregion

        #region Asynchronous methods

        /// <summary>
        /// [Asynchronous method] Add an object with the specified ID
        /// </summary>
        /// <param name="key">Cache key</param>
        /// <param name="value">Cache value</param>
        /// <param name="isFullKey">Whether it is already a full key, if not, the GetFinalKey() method will be called once</param>
        /// <param name="expiry">Expiration time</param>
        Task SetAsync(TKey key, TValue value, TimeSpan? expiry = null, bool isFullKey = false);

        /// <summary>
        /// [Asynchronous method] Remove the object with the specified cache key
        /// </summary>
        /// <param name="key">Cache key</param>
        /// <param name="isFullKey">Whether it is already a full key, if not, the GetFinalKey() method will be called once</param>
        Task RemoveFromCacheAsync(TKey key, bool isFullKey = false);

        /// <summary>
        /// [Asynchronous method] Return the object with the specified cache key
        /// </summary>
        /// <param name="key">Cache key</param>
        /// <param name="isFullKey">Whether it is already a full key, if not, the GetFinalKey() method will be called once</param>
        /// <returns></returns>
        Task<TValue> GetAsync(TKey key, bool isFullKey = false);

        /// <summary>
        /// [Asynchronous method] Return the object with the specified cache key and force the specified type
        /// </summary>
        /// <param name="key">Cache key</param>
        /// <param name="isFullKey">Whether it is already a full key, if not, the GetFinalKey() method will be called once</param>
        /// <returns></returns>
        Task<T> GetAsync<T>(TKey key, bool isFullKey = false);

        /// <summary>
        /// [Asynchronous method] Get all cache information collections
        /// </summary>
        /// <returns></returns>
        Task<IDictionary<TKey, TValue>> GetAllAsync();

        /// <summary>
        /// [Asynchronous method] Check if the Key and object exist
        /// </summary>
        /// <param name="key">Cache key</param>
        /// <param name="isFullKey">Whether it is already a full key, if not, the GetFinalKey() method will be called once</param>
        /// <returns></returns>
        Task<bool> CheckExistedAsync(TKey key, bool isFullKey = false);

        /// <summary>
        /// [Asynchronous method] Get the total number of cache collections (Note: The counting objects of each cache framework may not be consistent!)
        /// </summary>
        /// <returns></returns>
        Task<long> GetCountAsync();

        /// <summary>
        /// [Asynchronous method] Update the cache
        /// </summary>
        /// <param name="key">Cache key</param>
        /// <param name="value">Cache value</param>
        /// <param name="isFullKey">Whether it is already a full key, if not, the GetFinalKey() method will be called once</param>
        /// <param name="expiry">Expiration time</param>
        Task UpdateAsync(TKey key, TValue value, TimeSpan? expiry = null, bool isFullKey = false);
        #endregion
    }
}
