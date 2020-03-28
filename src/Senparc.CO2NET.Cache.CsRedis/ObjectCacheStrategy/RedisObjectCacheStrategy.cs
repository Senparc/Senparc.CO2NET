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

    --CO2NET--

    修改标识：Senparc - 20180714
    修改描述：v3.0.0 改为 Key-Value 实现

    修改标识：Senparc - 20180715
    修改描述：v3.0.1 添加 GetAllByPrefix() 方法

    修改标识：Senparc - 20190418
    修改描述：v3.5.0.1 添加 GetAllByPrefixAsync() 方法

    修改标识：Senparc - 20190914
    修改描述：v3.5.4 fix bug：GetServer().Keys() 方法添加 database 索引值

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

        /// <summary>
        /// 获得过期时间（秒）
        /// </summary>
        /// <param name="expiry"></param>
        /// <returns></returns>
        private int GetExpirySeconds(TimeSpan? expiry)
        {
            var expirySeconds = expiry.HasValue ? (int)Math.Ceiling(expiry.Value.TotalSeconds) : -1;
            return expirySeconds;
        }


        #region 实现 IBaseObjectCacheStrategy 接口

        #region 同步接口

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="isFullKey">是否已经是完整的Key</param>
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

            var value = base.Client.Get(cacheKey);
            if (value != null)
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


            var keys = base.Client.Keys(/*database: Client.GetDatabase().Database,*/ pattern: keyPrefix + "*"/*, pageSize: 99999*/);
            foreach (var redisKey in keys)
            {
                dic[redisKey] = Get(redisKey, true);
            }
            return dic;
        }

        //TODO: 提供 GetAllKeys() 方法


        public override long GetCount()
        {
            var keyPattern = GetFinalKey("*");//获取带Senparc:DefaultCache:前缀的Key（[DefaultCache]         
            var count = base.Client.Keys(/*database: Client.GetDatabase().Database,*/ pattern: keyPattern/*, pageSize: 99999*/).Count();
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
            base.Client.Set(cacheKey, json, GetExpirySeconds(expiry));
        }

        public override void RemoveFromCache(string key, bool isFullKey = false)
        {
            if (string.IsNullOrEmpty(key))
            {
                return;
            }

            var cacheKey = GetFinalKey(key, isFullKey);

            SenparcMessageQueue.OperateQueue();//延迟缓存立即生效
            base.Client.Del(cacheKey);//删除键
        }

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

            var cacheKey = GetFinalKey(key, isFullKey);

            var value = await base.Client.GetAsync(cacheKey).ConfigureAwait(false);
            if (value != null)
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
            var keyPrefix = GetFinalKey("");//获取带Senparc:DefaultCache:前缀的Key（[DefaultCache]可配置）
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

            SenparcMessageQueue.OperateQueue();//延迟缓存立即生效
            await base.Client.DelAsync(cacheKey).ConfigureAwait(false);//删除键
        }

        public override async Task UpdateAsync(string key, object value, TimeSpan? expiry = null, bool isFullKey = false)
        {
            await SetAsync(key, value, expiry, isFullKey).ConfigureAwait(false);
        }

#endif
        #endregion

        #endregion

        /// <summary>
        /// 根据 key 的前缀获取对象列表（最多 99999 条）
        /// </summary>
        public IList<T> GetAllByPrefix<T>(string key)
        {
            var keyPattern = GetFinalKey("*");//获取带Senparc:DefaultCache:前缀的Key（[DefaultCache]         
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
        /// 【异步方法】根据 key 的前缀获取对象列表（最多 99999 条）
        /// </summary>
        public async Task<IList<T>> GetAllByPrefixAsync<T>(string key)
        {
            var keyPattern = GetFinalKey("*");//获取带Senparc:DefaultCache:前缀的Key（[DefaultCache]         
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
