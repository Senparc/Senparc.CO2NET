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
    Copyright (C) 2020 Senparc

    文件名：LocalContainerCacheStrategy.cs
    文件功能描述：本地容器缓存。


    创建标识：Senparc - 20160308

    修改标识：Senparc - 20160812
    修改描述：v4.7.4  解决Container无法注册的问题

    修改标识：Senparc - 20170205
    修改描述：v0.2.0 重构分布式锁

    修改标识：Senparc - 20181226
    修改描述：v0.4.3 修改 DateTime 为 DateTimeOffset

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
#if NET45
using System.Web;
#else
using Microsoft.Extensions.Caching.Memory;

#endif


namespace Senparc.CO2NET.Cache
{
    /// <summary>
    /// 本地容器缓存策略
    /// </summary>
    public class LocalObjectCacheStrategy : BaseCacheStrategy, IBaseObjectCacheStrategy
    {
        #region 数据源

#if NET45
        private System.Web.Caching.Cache _cache = LocalObjectCacheHelper.LocalObjectCache;
#else
        private IMemoryCache _cache = LocalObjectCacheHelper.LocalObjectCache;
#endif

        #endregion

        #region 单例

        ///<summary>
        /// LocalCacheStrategy的构造函数
        ///</summary>
        LocalObjectCacheStrategy()
        {
        }

        //静态LocalCacheStrategy
        public static LocalObjectCacheStrategy Instance
        {
            get
            {
                return Nested.instance;//返回Nested类中的静态成员instance
            }
        }


        class Nested
        {
            static Nested()
            {
            }
            //将instance设为一个初始化的LocalCacheStrategy新实例
            internal static readonly LocalObjectCacheStrategy instance = new LocalObjectCacheStrategy();
        }


        #endregion

        #region IObjectCacheStrategy 成员

        //public IContainerCacheStrategy ContainerCacheStrategy
        //{
        //    get { return LocalContainerCacheStrategy.Instance; }
        //}

        #region 同步方法

        [Obsolete("此方法已过期，请使用 Set(TKey key, TValue value) 方法")]
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

#if NET45
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

            //由于MemoryCache不支持遍历Keys，所以需要单独储存
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

#if !NET35 && !NET40 && !NET45
            //移除key
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

#if NET45
            return _cache[cacheKey];
#else
            return _cache.Get(cacheKey);
#endif
        }

        /// <summary>
        /// 返回指定缓存键的对象，并强制指定类型
        /// </summary>
        /// <param name="key">缓存键</param>
        /// <param name="isFullKey">是否已经是完整的Key，如果不是，则会调用一次GetFinalKey()方法</param>
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

#if NET45
            return (T)_cache[cacheKey];
#else
            return _cache.Get<T>(cacheKey);
#endif
        }

        public IDictionary<string, object> GetAll()
        {
            IDictionary<string, object> data = new Dictionary<string, object>();
#if NET45
            IDictionaryEnumerator cacheEnum = System.Web.HttpRuntime.Cache.GetEnumerator();

            while (cacheEnum.MoveNext())
            {
                data[cacheEnum.Key as string] = cacheEnum.Value;
            }
#else
            //获取所有Key
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

#if NET45
            return _cache.Get(cacheKey) != null;
#else
            return _cache.Get(cacheKey) != null;
#endif
        }

        public long GetCount()
        {
#if NET45
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
#if !NET35 && !NET40

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
        /// 返回指定缓存键的对象，并强制指定类型
        /// </summary>
        /// <param name="key">缓存键</param>
        /// <param name="isFullKey">是否已经是完整的Key，如果不是，则会调用一次GetFinalKey()方法</param>
        /// <returns></returns>
        public async Task<T> GetAsync<T>(string key, bool isFullKey = false)
        {
            return await Task.Factory.StartNew(() => Get<T>(key, isFullKey)).ConfigureAwait(false);
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
#endif
        #endregion

        #endregion

        #region ICacheLock

        public override ICacheLock BeginCacheLock(string resourceName, string key, int retryCount = 0, TimeSpan retryDelay = new TimeSpan())
        {
            return LocalCacheLock.CreateAndLock(this, resourceName, key, retryCount, retryDelay);
        }

#if !NET35 && !NET40
        public override async Task<ICacheLock> BeginCacheLockAsync(string resourceName, string key, int retryCount = 0, TimeSpan retryDelay = new TimeSpan())
        {
            return await LocalCacheLock.CreateAndLockAsync(this, resourceName, key, retryCount, retryDelay).ConfigureAwait(false);
        }
#endif
        #endregion

    }
}
