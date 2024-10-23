#region Apache License Version 2.0
/*----------------------------------------------------------------

Copyright 2023 Suzhou Senparc Network Technology Co.,Ltd.

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

    FileName：LocalContainerCacheStrategy.cs
    File Function Description：Local container cache.


    Creation Identifier：Senparc - 20160308

    Modification Identifier：Senparc - 20160812
    Modification Description：v4.7.4  Fixed the issue where Container could not be registered

    Modification Identifier：Senparc - 20170205
    Modification Description：v0.2.0 Refactored distributed lock

    Modification Identifier：Senparc - 20181226
    Modification Description：v0.4.3 Changed DateTime to DateTimeOffset

 ----------------------------------------------------------------*/


using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using Senparc.CO2NET.Cache;
using Senparc.CO2NET.Exceptions;
using System.Threading.Tasks;
#if NET462
using System.Web;
#else
using Microsoft.Extensions.Caching.Memory;

#endif


namespace Senparc.CO2NET.Cache
{
    /// <summary>
    /// Local container cache strategy
    /// </summary>
    public class LocalObjectCacheStrategy : BaseCacheStrategy, IBaseObjectCacheStrategy
    {
        #region 数据源

#if NET462
        private System.Web.Caching.Cache _cache = LocalObjectCacheHelper.LocalObjectCache;
#else
        private IMemoryCache _cache = LocalObjectCacheHelper.LocalObjectCache;
#endif

        #endregion

        #region 单例

        ///<summary>
        /// Constructor of LocalCacheStrategy
        ///</summary>
        LocalObjectCacheStrategy()
        {
        }

        //Static LocalCacheStrategy
        public static LocalObjectCacheStrategy Instance
        {
            get
            {
                return Nested.instance;//Returns the static member instance in the Nested class
            }
        }


        class Nested
        {
            static Nested()
            {
            }
            //Sets instance to a new initialized instance of LocalCacheStrategy
            internal static readonly LocalObjectCacheStrategy instance = new LocalObjectCacheStrategy();
        }


        #endregion

        #region IObjectCacheStrategy 成员

        //public IContainerCacheStrategy ContainerCacheStrategy
        //{
        //    get { return LocalContainerCacheStrategy.Instance; }
        //}

        #region 同步方法

        [Obsolete("此方法已过期，请使用 Set(TKey key, TValue value) 方法", true)]
        public void InsertToCache(string key, object value, TimeSpan? expiry = null)
        {
            Set(key, value, expiry, false);
        }


        public void Set(string key, object value, TimeSpan? expiry = null, bool isFullKey = false)
        {
            if (key == null || value == null)
            {
                return;
            }

            var finalKey = base.GetFinalKey(key, isFullKey);

#if NET462
            _cache[finalKey] = value;
#else
            var newKey = !CheckExisted(finalKey, true);

            if (expiry.HasValue)
            {
                _cache.Set(finalKey, value, expiry.Value);
            }
            else
            {
                _cache.Set(finalKey, value);
            }

            //Since MemoryCache does not support traversing Keys, it needs to be stored separately
            if (newKey)
            {
                var keyStoreFinalKey = LocalObjectCacheHelper.GetKeyStoreKey(this);
                List<string> keys;
                if (!CheckExisted(keyStoreFinalKey, true))
                {
                    keys = new List<string>();
                }
                else
                {
                    keys = _cache.Get<List<string>>(keyStoreFinalKey);
                }
                keys.Add(finalKey);
                _cache.Set(keyStoreFinalKey, keys);
            }

#endif
        }

        public void RemoveFromCache(string key, bool isFullKey = false)
        {
            var cacheKey = GetFinalKey(key, isFullKey);
            _cache.Remove(cacheKey);

#if !NET462
            //Remove key
            var keyStoreFinalKey = LocalObjectCacheHelper.GetKeyStoreKey(this);
            if (CheckExisted(keyStoreFinalKey, true))
            {
                var keys = _cache.Get<List<string>>(keyStoreFinalKey);
                keys.Remove(cacheKey);
                _cache.Set(keyStoreFinalKey, keys);
            }
#endif

        }

        public object Get(string key, bool isFullKey = false)
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

            var cacheKey = GetFinalKey(key, isFullKey);

#if NET462
            return _cache[cacheKey];
#else
            return _cache.Get(cacheKey);
#endif
        }

        /// <summary>
        /// Returns the object of the specified cache key and forces the specified type
        /// </summary>
        /// <param name="key">Cache key</param>
        /// <param name="isFullKey">Whether it is already a complete Key, if not, the GetFinalKey() method will be called once</param>
        /// <returns></returns>
        public T Get<T>(string key, bool isFullKey = false)
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

#if NET462
            return (T)_cache[cacheKey];
#else
            return _cache.Get<T>(cacheKey);
#endif
        }

        public IDictionary<string, object> GetAll()
        {
            IDictionary<string, object> data = new Dictionary<string, object>();
#if NET462
            IDictionaryEnumerator cacheEnum = System.Web.HttpRuntime.Cache.GetEnumerator();

            while (cacheEnum.MoveNext())
            {
                data[cacheEnum.Key as string] = cacheEnum.Value;
            }
#else
            //Get all Keys
            var keyStoreFinalKey = LocalObjectCacheHelper.GetKeyStoreKey(this);
            if (CheckExisted(keyStoreFinalKey, true))
            {
                var keys = _cache.Get<List<string>>(keyStoreFinalKey);
                foreach (var key in keys)
                {
                    data[key] = Get(key, true);
                }
            }
#endif
            return data;

        }

        public bool CheckExisted(string key, bool isFullKey = false)
        {
            var cacheKey = GetFinalKey(key, isFullKey);

#if NET462
            return _cache.Get(cacheKey) != null;
#else
            return _cache.Get(cacheKey) != null;
#endif
        }

        public long GetCount()
        {
#if NET462
            return _cache.Count;
#else
            var keyStoreFinalKey = LocalObjectCacheHelper.GetKeyStoreKey(this);
            if (CheckExisted(keyStoreFinalKey, true))
            {
                var keys = _cache.Get<List<string>>(keyStoreFinalKey);
                return keys.Count;
            }
            else
            {
                return 0;
            }
#endif
        }

        public void Update(string key, object value, TimeSpan? expiry = null, bool isFullKey = false)
        {
            Set(key, value, expiry, isFullKey);
        }

        #endregion

        #region 异步方法
        public async Task SetAsync(string key, object value, TimeSpan? expiry = null, bool isFullKey = false)
        {
            await Task.Factory.StartNew(() => Set(key, value, expiry, isFullKey)).ConfigureAwait(false);
        }

        public async Task RemoveFromCacheAsync(string key, bool isFullKey = false)
        {
            await Task.Factory.StartNew(() => RemoveFromCache(key, isFullKey)).ConfigureAwait(false);
        }

        public async Task<object> GetAsync(string key, bool isFullKey = false)
        {
            return await Task.Factory.StartNew(() => Get(key, isFullKey)).ConfigureAwait(false);
        }

        /// <summary>
        /// Returns the object of the specified cache key and forces the specified type
        /// </summary>
        /// <param name="key">Cache key</param>
        /// <param name="isFullKey">Whether it is already a complete Key, if not, the GetFinalKey() method will be called once</param>
        /// <returns></returns>
        public async Task<T> GetAsync<T>(string key, bool isFullKey = false)
        {
#if NET462
            return await Task.Factory.StartNew(() => Get<T>(key, isFullKey)).ConfigureAwait(false);
#else
            return await Task.Factory.StartNew(() => Get<T>(key, isFullKey)).ConfigureAwait(false);

            //TODO: Use _cache.GetOrCreateAsync
#endif

        }

        public async Task<IDictionary<string, object>> GetAllAsync()
        {
            return await Task.Factory.StartNew(() => GetAll()).ConfigureAwait(false);
        }


        public async Task<bool> CheckExistedAsync(string key, bool isFullKey = false)
        {
            return await Task.Factory.StartNew(() => CheckExisted(key, isFullKey)).ConfigureAwait(false);

        }

        public async Task<long> GetCountAsync()
        {
            return await Task.Factory.StartNew(() => GetCount()).ConfigureAwait(false);
        }


        public async Task UpdateAsync(string key, object value, TimeSpan? expiry = null, bool isFullKey = false)
        {
            await Task.Factory.StartNew(() => Update(key, value, expiry, isFullKey)).ConfigureAwait(false);
        }
        #endregion

        #endregion

        #region ICacheLock

        public override ICacheLock BeginCacheLock(string resourceName, string key, int retryCount = 0, TimeSpan retryDelay = new TimeSpan())
        {
            return LocalCacheLock.CreateAndLock(this, resourceName, key, retryCount, retryDelay);
        }

        public override async Task<ICacheLock> BeginCacheLockAsync(string resourceName, string key, int retryCount = 0, TimeSpan retryDelay = new TimeSpan())
        {
            return await LocalCacheLock.CreateAndLockAsync(this, resourceName, key, retryCount, retryDelay).ConfigureAwait(false);
        }
        #endregion

    }
}
