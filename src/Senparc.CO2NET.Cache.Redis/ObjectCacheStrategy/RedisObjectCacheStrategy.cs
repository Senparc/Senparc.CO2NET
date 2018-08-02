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

Detail: https://github.com/Senparc/Senparc.CO2NET/blob/master/LICENSE

----------------------------------------------------------------*/
#endregion Apache License Version 2.0

/*----------------------------------------------------------------
    Copyright (C) 2018 Senparc

    文件名：RedisObjectCacheStrategy.cs
    文件功能描述：Redis的Object类型容器缓存（Key为String类型）。


    创建标识：Senparc - 20161024

    修改标识：Senparc - 20170205
    修改描述：v0.2.0 重构分布式锁

    --CO2NET--

    修改标识：Senparc - 20180714
    修改描述：v3.0.0 改为 Key-Value 实现

    修改标识：Senparc - 20180715
    修改描述：v3.0.1 添加 GetAllByPrefix() 方法

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
    /// Redis的Object类型容器缓存（Key为String类型），Key-Value 类型储存
    /// </summary>
    public class RedisObjectCacheStrategy : BaseRedisObjectCacheStrategy
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

        /// <summary>
        /// 注意：此方法获取的object为直接储存在缓存中，序列化之后的Value
        /// </summary>
        /// <returns></returns>
        public override IDictionary<string, object> GetAll()
        {
            var keyPrefix = GetFinalKey("");//获取带Senparc:DefaultCache:前缀的Key（[DefaultCache]可配置）
            var dic = new Dictionary<string, object>();

            var keys = GetServer().Keys(pattern: keyPrefix + "*");
            foreach (var redisKey in keys)
            {
                dic[redisKey] = Get(redisKey, true);
            }
            return dic;
        }


        public override long GetCount()
        {
            var keyPattern = GetFinalKey("*");//获取带Senparc:DefaultCache:前缀的Key（[DefaultCache]         
            var count = GetServer().Keys(pattern: keyPattern).Count();
            return count;
        }

        [Obsolete("此方法已过期，请使用 Set(TKey key, TValue value) 方法")]
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
            _cache.StringSet(cacheKey, json, expiry);
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

        public override void Update(string key, object value, TimeSpan? expiry = null, bool isFullKey = false)
        {
            Set(key, value, expiry, isFullKey);
        }
        #endregion

        public override ICacheLock BeginCacheLock(string resourceName, string key, int retryCount = 0, TimeSpan retryDelay = new TimeSpan())
        {
            return new RedisCacheLock(this, resourceName, key, retryCount, retryDelay);
        }


        /// <summary>
        /// 根据 key 的前缀获取对象列表
        /// </summary>
        public IList<T> GetAllByPrefix<T>(string key)
        {
            var keyPattern = GetFinalKey("*");//获取带Senparc:DefaultCache:前缀的Key（[DefaultCache]         
            var keys = GetServer().Keys(pattern: keyPattern);
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
    }
}
