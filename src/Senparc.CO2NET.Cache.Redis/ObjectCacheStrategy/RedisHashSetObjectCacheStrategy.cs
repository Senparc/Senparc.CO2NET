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

    文件名：RedisObjectCacheStrategy.cs
    文件功能描述：Redis的Object类型容器缓存（Key为String类型）。


    创建标识：Senparc - 20161024

    修改标识：Senparc - 20170205
    修改描述：v0.2.0 重构分布式锁

    修改标识：Senparc - 20170205
    修改描述： v3.0.0 RedisObjectCacheStrategy 重命名为 RedisHashSetObjectCacheStrategy，分离 HashSet 数据结构
      1、分离 HashSet 和 Key-Value 两种不同格式的缓存：RedisHashSetObjectCacheStrategy 以及 RedisObjectCacheStrategy
      2、提供缓存过期新策略

    修改标识：Senparc - 20190418
    修改描述：v3.5.0 提供  HashGetAllAsync() 异步方法

    修改标识：Senparc - 20190914
    修改描述：v3.5.4 fix bug：GetServer().Keys() 方法添加 database 索引值

 ----------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Linq;
using Senparc.CO2NET.Helpers;
using Senparc.CO2NET.MessageQueue;
using Senparc.CO2NET.Cache;
using StackExchange.Redis;
using Senparc.CO2NET.Trace;
using System.Threading.Tasks;

namespace Senparc.CO2NET.Cache.Redis
{
    /// <summary>
    /// Redis的Object类型容器缓存（Key为String类型）
    /// </summary>
    public class RedisHashSetObjectCacheStrategy : BaseRedisObjectCacheStrategy
    //where TContainerBag : class, IBaseContainerBag, new()
    {
        /// <summary>
        /// Hash储存的Key和Field集合
        /// </summary>
        protected class HashKeyAndField
        {
            public string Key { get; set; }
            public string Field { get; set; }
        }

        #region 单例

        /// <summary>
        /// Redis 缓存策略
        /// </summary>
        RedisHashSetObjectCacheStrategy() : base()
        {

        }

        //静态SearchCache
        public static RedisHashSetObjectCacheStrategy Instance
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
            internal static readonly RedisHashSetObjectCacheStrategy instance = new RedisHashSetObjectCacheStrategy();
        }

        #endregion


        /// <summary>
        /// 获取Hash储存的Key和Field
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
                Field = finalFullKey.Substring(index + 1/*排除:号*/, finalFullKey.Length - index - 1)
            };
            return hashKeyAndField;
        }

        #region 实现 IBaseObjectCacheStrategy 接口

        #region 同步方法


        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="isFullKey">是否已经是完整的Key</param>
        /// <returns></returns>
        public override bool CheckExisted(string key, bool isFullKey = false)
        {
            //var cacheKey = GetFinalKey(key, isFullKey);
            var hashKeyAndField = this.GetHashKeyAndField(key, isFullKey);

            //return _cache.KeyExists(cacheKey);
            return _cache.HashExists(hashKeyAndField.Key, hashKeyAndField.Field);
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
            var value = _cache.HashGet(hashKeyAndField.Key, hashKeyAndField.Field);
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

            //var cacheKey = GetFinalKey(key, isFullKey);
            var hashKeyAndField = this.GetHashKeyAndField(key, isFullKey);

            //var value = _cache.StringGet(cacheKey);
            var value = _cache.HashGet(hashKeyAndField.Key, hashKeyAndField.Field);
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
        //    key = GetFinalKey(key);//获取带Senparc:DefaultCache:前缀的Key（[DefaultCache]可配置）

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
        /// 注意：此方法获取的object为直接储存在缓存中，序列化之后的Value（最多 99999 条）
        /// </summary>
        /// <returns></returns>
        public override IDictionary<string, object> GetAll()
        {
            var keyPrefix = GetFinalKey("");//Senparc:DefaultCache:前缀的Key（[DefaultCache]可配置）
            var dic = new Dictionary<string, object>();

            var hashKeys = GetServer().Keys(database: Client.GetDatabase().Database, pattern: keyPrefix + "*", pageSize: 99999);
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

        /// <summary>
        /// 获取所有缓存项计数（最多 99999 条）
        /// </summary>
        /// <returns></returns>

        public override long GetCount()
        {
            var keyPattern = GetFinalKey("*");//获取带Senparc:DefaultCache:前缀的Key（[DefaultCache]         
            var count = GetServer().Keys(database: Client.GetDatabase().Database, pattern: keyPattern, pageSize: 99999).Count();
            return count;
        }

        /// <summary>
        /// 插入对象。注意：过期时间对 HashSet 无效！
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="expiry"></param>
        [Obsolete("此方法已过期，请使用 Set(TKey key, TValue value) 方法")]
        public override void InsertToCache(string key, object value, TimeSpan? expiry = null)
        {
            Set(key, value, expiry, false);
        }

        /// <summary>
        /// 设置对象。注意：过期时间对 HashSet 无效！
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
            //    //Dictionary类型
            //}

            //_cache.StringSet(cacheKey, value.Serialize());
            //_cache.HashSet(hashKeyAndField.Key, hashKeyAndField.Field, value.Serialize());


            //StackExchangeRedisExtensions.Serialize效率非常差
            //_cache.HashSet(hashKeyAndField.Key, hashKeyAndField.Field, StackExchangeRedisExtensions.Serialize(value));

            var json = value.SerializeToCache();
            _cache.HashSet(hashKeyAndField.Key, hashKeyAndField.Field, json);

            //#if DEBUG
            //            var value1 = _cache.HashGet(hashKeyAndField.Key, hashKeyAndField.Field);//正常情况下可以得到 //_cache.GetValue(cacheKey);
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

            SenparcMessageQueue.OperateQueue();//延迟缓存立即生效
            //_cache.KeyDelete(cacheKey);//删除键
            _cache.HashDelete(hashKeyAndField.Key, hashKeyAndField.Field);//删除项
        }

        /// <summary>
        /// 更新对象。注意：过期时间对 HashSet 无效！
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
#if !NET35 && !NET40



        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="isFullKey">是否已经是完整的Key</param>
        /// <returns></returns>
        public override async Task<bool> CheckExistedAsync(string key, bool isFullKey = false)
        {
            //var cacheKey = GetFinalKey(key, isFullKey);
            var hashKeyAndField = this.GetHashKeyAndField(key, isFullKey);

            //return _cache.KeyExists(cacheKey);
            return await _cache.HashExistsAsync(hashKeyAndField.Key, hashKeyAndField.Field).ConfigureAwait(false);
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
            var value = await _cache.HashGetAsync(hashKeyAndField.Key, hashKeyAndField.Field).ConfigureAwait(false);
            if (value.HasValue)
            {
                return value.ToString().DeserializeFromCache();
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
            var value = await _cache.HashGetAsync(hashKeyAndField.Key, hashKeyAndField.Field).ConfigureAwait(false);
            if (value.HasValue)
            {
                return value.ToString().DeserializeFromCache<T>();
            }

            return default(T);
        }

        /// <summary>
        /// 注意：此方法获取的object为直接储存在缓存中，序列化之后的Value（最多 99999 条）
        /// </summary>
        /// <returns></returns>
        public override async Task<IDictionary<string, object>> GetAllAsync()
        {
            var keyPrefix = GetFinalKey("");//Senparc:DefaultCache:前缀的Key（[DefaultCache]可配置）
            var dic = new Dictionary<string, object>();

            var hashKeys = GetServer().Keys(database: Client.GetDatabase().Database, pattern: keyPrefix + "*", pageSize: 99999);
            foreach (var redisKey in hashKeys)
            {
                var list = await _cache.HashGetAllAsync(redisKey).ConfigureAwait(false);

                foreach (var hashEntry in list)
                {
                    var fullKey = redisKey.ToString() + ":" + hashEntry.Name;//最完整的finalKey（可用于LocalCache），还原完整Key，格式：[命名空间]:[Key]
                    dic[fullKey] = hashEntry.Value.ToString().DeserializeFromCache();
                }
            }
            return dic;
        }


        public override Task<long> GetCountAsync()
        {
            return Task.Factory.StartNew(() => GetCount());
        }

        /// <summary>
        /// 设置对象。注意：过期时间对 HashSet 无效！
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
            //    //Dictionary类型
            //}

            //_cache.StringSet(cacheKey, value.Serialize());
            //_cache.HashSet(hashKeyAndField.Key, hashKeyAndField.Field, value.Serialize());


            //StackExchangeRedisExtensions.Serialize效率非常差
            //_cache.HashSet(hashKeyAndField.Key, hashKeyAndField.Field, StackExchangeRedisExtensions.Serialize(value));

            var json = value.SerializeToCache();
            await _cache.HashSetAsync(hashKeyAndField.Key, hashKeyAndField.Field, json).ConfigureAwait(false);

            //#if DEBUG
            //            var value1 = _cache.HashGet(hashKeyAndField.Key, hashKeyAndField.Field);//正常情况下可以得到 //_cache.GetValue(cacheKey);
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

            SenparcMessageQueue.OperateQueue();//延迟缓存立即生效
                                               //_cache.KeyDelete(cacheKey);//删除键
            await _cache.HashDeleteAsync(hashKeyAndField.Key, hashKeyAndField.Field).ConfigureAwait(false);//删除项
        }

        /// <summary>
        /// 更新对象。注意：过期时间对 HashSet 无效！
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="isFullKey"></param>
        /// <param name="expiry"></param>
        public override async Task UpdateAsync(string key, object value, TimeSpan? expiry = null, bool isFullKey = false)
        {
            await SetAsync(key, value, expiry, isFullKey).ConfigureAwait(false);
        }

#endif
        #endregion

        #endregion


        /// <summary>
        /// _cache.HashGetAll()
        /// </summary>
        public HashEntry[] HashGetAll(string key)
        {
            return _cache.HashGetAll(key);
        }


        /// <summary>
        /// _cache.HashGetAll()
        /// </summary>
        public async Task<HashEntry[]> HashGetAllAsync(string key)
        {
            return await _cache.HashGetAllAsync(key).ConfigureAwait(false);
        }
    }
}
