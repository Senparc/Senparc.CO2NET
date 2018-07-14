#region Apache License Version 2.0
/*----------------------------------------------------------------

Copyright 2018 Jeffrey Su & Suzhou Senparc Network Technology Co.,Ltd.

Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file
except in compliance with the License. You may obtain a copy of the License at

http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software distributed under the
License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND,
either express or implied. See the License for the specific language governing permissions
and limitations under the License.

Detail: https://github.com/JeffreySu/WeiXinMPSDK/blob/master/license.md

----------------------------------------------------------------*/
#endregion Apache License Version 2.0

/*----------------------------------------------------------------
    Copyright (C) 2018 Senparc

    文件名：LocalContainerCacheStrategy.cs
    文件功能描述：本地容器缓存。


    创建标识：Senparc - 20160308

    修改标识：Senparc - 20160812
    修改描述：v4.7.4  解决Container无法注册的问题

    修改标识：Senparc - 20170205
    修改描述：v0.2.0 重构分布式锁

 ----------------------------------------------------------------*/



using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using Senparc.CO2NET.Cache;

namespace Senparc.CO2NET.Cache
{
    /// <summary>
    /// 全局静态数据源帮助类
    /// </summary>
    public static class LocalObjectCacheHelper
    {
#if NET35 || NET40 || NET45
        /// <summary>
        /// 所有数据集合的列表
        /// </summary>
        public static System.Web.Caching.Cache LocalObjectCache { get; set; }

        static LocalObjectCacheHelper()
        {
            LocalObjectCache = System.Web.HttpRuntime.Cache;
        }
#else
        /// <summary>
        /// 所有数据集合的列表
        /// </summary>
        public static IDictionary<string, object> LocalObjectCache { get; set; }

        static LocalObjectCacheHelper()
        {
            LocalObjectCache = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
        }
#endif
    }

    /// <summary>
    /// 本地容器缓存策略
    /// </summary>
    public class LocalObjectCacheStrategy : BaseCacheStrategy, IBaseObjectCacheStrategy
    {
        #region 数据源

#if NET35 || NET40 || NET45
        private System.Web.Caching.Cache _cache = LocalObjectCacheHelper.LocalObjectCache;
#else
        private IDictionary<string, object> _cache = LocalObjectCacheHelper.LocalObjectCache;

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

        [Obsolete("此方法已过期，请使用 Set(TKey key, TValue value) 方法")]
        public void InsertToCache(string key, object value, TimeSpan? expiry = null)
        {
            Set(key, value, expiry);
        }

        public void Set(string key, object value, TimeSpan? expiry = null)
        {
            if (key == null || value == null)
            {
                return;
            }

            var finalKey = base.GetFinalKey(key);

#if NET35 || NET40 || NET45
            _cache[finalKey] = value;
#else
#endif
        }

        public void RemoveFromCache(string key, bool isFullKey = false)
        {
            var cacheKey = GetFinalKey(key, isFullKey);
            _cache.Remove(cacheKey);
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
            return _cache[cacheKey];
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
            return (T)_cache[cacheKey];
        }

        //public IDictionary<string, TBag> GetAll<TBag>() where TBag : IBaseContainerBag
        //{
        //    var dic = new Dictionary<string, TBag>();
        //    var cacheList = GetAll();
        //    foreach (var baseContainerBag in cacheList)
        //    {
        //        if (baseContainerBag.Value is TBag)
        //        {
        //            dic[baseContainerBag.Key] = (TBag)baseContainerBag.Value;
        //        }
        //    }
        //    return dic;
        //}

        public IDictionary<string, object> GetAll()
        {
            IDictionary<string, object> data = new Dictionary<string, object>();
#if NET35 || NET40 || NET45
            IDictionaryEnumerator cacheEnum = System.Web.HttpRuntime.Cache.GetEnumerator();

            while (cacheEnum.MoveNext())
            {
                data[cacheEnum.Key as string] = cacheEnum.Value;
            }
            return data;
#else
         return _cache;
#endif

        }

        public bool CheckExisted(string key, bool isFullKey = false)
        {
            var cacheKey = GetFinalKey(key, isFullKey);
#if NET35 || NET40 || NET45
            return _cache.Get(cacheKey) != null;
#else
            return _cache.ContainsKey(cacheKey);
#endif
        }

        public long GetCount()
        {
            return _cache.Count;
        }

        public void Update(string key, object value, bool isFullKey = false)
        {
            var cacheKey = GetFinalKey(key, isFullKey);
            _cache[cacheKey] = value;
        }

        //public void UpdateContainerBag(string key, object bag, bool isFullKey = false)
        //{
        //    Update(key, bag, isFullKey);
        //}

#endregion

#region ICacheLock

        public override ICacheLock BeginCacheLock(string resourceName, string key, int retryCount = 0, TimeSpan retryDelay = new TimeSpan())
        {
            return new LocalCacheLock(this, resourceName, key, retryCount, retryDelay);
        }

#endregion

    }
}
