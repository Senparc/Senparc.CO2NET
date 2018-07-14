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

    文件名：RedisObjectCacheStrategy.cs
    文件功能描述：Redis的Object类型容器缓存（Key为String类型）。


    创建标识：Senparc - 20161024

    修改标识：Senparc - 20170205
    修改描述：v0.2.0 重构分布式锁

 ----------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Linq;
using Senparc.CO2NET.Helpers;
using Senparc.CO2NET.MessageQueue;
using Senparc.CO2NET.Cache;
using StackExchange.Redis;
using Senparc.CO2NET.Trace;

namespace Senparc.CO2NET.Cache.Redis
{
    /// <summary>
    /// Redis的Object类型容器缓存（Key为String类型）
    /// </summary>
    public class RedisObjectCacheStrategy : RedisBaseObjectCacheStrategy
    {
        #region 单例

        /// <summary>
        /// Redis 缓存策略
        /// </summary>
        RedisObjectCacheStrategy() : base()
        {

        }

        //静态SearchCache
        public static RedisObjectCacheStrategy Instance
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
            //将instance设为一个初始化的BaseCacheStrategy新实例
            internal static readonly RedisObjectCacheStrategy instance = new RedisObjectCacheStrategy();
        }

        #endregion


        #region 实现 IBaseObjectCacheStrategy 接口

        //public string CacheSetKey { get; set; }

        //public IContainerCacheStrategy ContainerCacheStrategy
        //{
        //    get { return RedisContainerCacheStrategy.Instance; }
        //}



        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="isFullKey">是否已经是完整的Key</param>
        /// <returns></returns>
        public override bool CheckExisted(string key, bool isFullKey = false)
        {
            var cacheKey = GetFinalKey(key, isFullKey);
            return _cache.KeyExists(cacheKey);
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

            var value = _cache.StringGet(cacheKey);
            if (value.HasValue)
            {
                return value.ToString().DeserializeFromCache();
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

            var value = _cache.StringGet(cacheKey);
            if (value.HasValue)
            {
                return value.ToString().DeserializeFromCache<T>();
            }

            return default(T);
        }



        //public IDictionary<string, TBag> GetAll<TBag>() where TBag : IBaseContainerBag
        //{
        //    #region 旧方法（没有使用Hash之前）

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
        //    key = key.Substring(0, key.Length - 1);//去掉:号
        //    key = GetFinalKey(key);//获取带SenparcWeixin:DefaultCache:前缀的Key（[DefaultCache]可配置）

        //    var list = _cache.HashGetAll(key);
        //    var dic = new Dictionary<string, TBag>();

        //    foreach (var hashEntry in list)
        //    {
        //        var fullKey = key + ":" + hashEntry.Name;//最完整的finalKey（可用于LocalCache），还原完整Key，格式：[命名空间]:[Key]
        //        dic[fullKey] = StackExchangeRedisExtensions.Deserialize<TBag>(hashEntry.Value);
        //    }

        //    return dic;
        //}

        /// <summary>
        /// 注意：此方法获取的object为直接储存在缓存中，序列化之后的Value
        /// </summary>
        /// <returns></returns>
        public override IDictionary<string, object> GetAll()
        {
            var keyPrefix = GetFinalKey("");//获取带SenparcWeixin:DefaultCache:前缀的Key（[DefaultCache]可配置）
            var dic = new Dictionary<string, object>();

            var hashKeys = GetServer().Keys(pattern: keyPrefix + "*");
            foreach (var redisKey in hashKeys)
            {
                var list = _cache.HashGetAll(redisKey);

                foreach (var hashEntry in list)
                {
                    var fullKey = redisKey.ToString() + ":" + hashEntry.Name;//最完整的finalKey（可用于LocalCache），还原完整Key，格式：[命名空间]:[Key]
                    dic[fullKey] = hashEntry.Value.ToString().DeserializeFromCache();
                }
            }
            return dic;
        }


        public override long GetCount()
        {
            var keyPattern = GetFinalKey("*");//获取带SenparcWeixin:DefaultCache:前缀的Key（[DefaultCache]         
            var count = GetServer().Keys(pattern: keyPattern).Count();
            return count;
        }

        [Obsolete("此方法已过期，请使用 Set(TKey key, TValue value) 方法")]
        public override void InsertToCache(string key, object value)
        {
            Set(key, value);
        }

        public override void Set(string key, object value)
        {
            if (string.IsNullOrEmpty(key) || value == null)
            {
                return;
            }

            var cacheKey = GetFinalKey(key);

            //if (value is IDictionary)
            //{
            //    //Dictionary类型
            //}

            //_cache.StringSet(cacheKey, value.Serialize());
            //_cache.HashSet(hashKeyAndField.Key, hashKeyAndField.Field, value.Serialize());


            //StackExchangeRedisExtensions.Serialize效率非常差
            //_cache.HashSet(hashKeyAndField.Key, hashKeyAndField.Field, StackExchangeRedisExtensions.Serialize(value));

            var json = value.SerializeToCache();
            _cache.StringSet(cacheKey, json);
        }

        public override void RemoveFromCache(string key, bool isFullKey = false)
        {
            if (string.IsNullOrEmpty(key))
            {
                return;
            }

            var cacheKey = GetFinalKey(key, isFullKey);

            SenparcMessageQueue.OperateQueue();//延迟缓存立即生效
            _cache.KeyDelete(cacheKey);//删除键
        }

        public override void Update(string key, object value, bool isFullKey = false)
        {
            var cacheKey = GetFinalKey(key, isFullKey);

            //value.Key = cacheKey;//储存最终的键

            //_cache.StringSet(cacheKey, value.Serialize());

            //_cache.HashSet(hashKeyAndField.Key, hashKeyAndField.Field, value.Serialize());

            //StackExchangeRedisExtensions.Serialize效率非常差
            //_cache.HashSet(hashKeyAndField.Key, hashKeyAndField.Field, StackExchangeRedisExtensions.Serialize(value));
            var json = value.SerializeToCache();
            _cache.StringSet(cacheKey, json);
        }
        #endregion

        public override ICacheLock BeginCacheLock(string resourceName, string key, int retryCount = 0, TimeSpan retryDelay = new TimeSpan())
        {
            return new RedisCacheLock(this, resourceName, key, retryCount, retryDelay);
        }


        /// <summary>
        /// _cache.HashGetAll()
        /// </summary>
        public HashEntry[] HashGetAll(string key)
        {
            return _cache.HashGetAll(key);
        }
    }
}
