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

    文件名：IBaseCacheStrategy.cs
    文件功能描述：缓存策略接口。


    创建标识：Senparc - 20160308

    修改标识：Senparc - 20160812
    修改描述：v4.7.4 解决Container无法注册的问题

    --CO2NET--
    
    修改标识：Senparc - 20190411
    修改描述：v0.6.0 提供缓存异步接口

 ----------------------------------------------------------------*/


using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Senparc.CO2NET.Cache
{
    /// <summary>
    /// 最底层的缓存策略接口
    /// </summary>
    public interface IBaseCacheStrategy
    {
        ///// <summary>
        ///// 整个Cache集合的Key
        ///// </summary>
        //string CacheSetKey { get; set; }

        /// <summary>
        /// 创建一个（分布）锁
        /// </summary>
        /// <param name="resourceName">资源名称</param>
        /// <param name="key">Key标识</param>
        /// <param name="retryCount">重试次数</param>
        /// <param name="retryDelay">重试延时</param>
        /// <returns></returns>
        ICacheLock BeginCacheLock(string resourceName, string key, int retryCount = 0, TimeSpan retryDelay = new TimeSpan());

#if !NET35 && !NET40
        /// <summary>
        /// 【异步方法】创建一个（分布）锁
        /// </summary>
        /// <param name="resourceName">资源名称</param>
        /// <param name="key">Key标识</param>
        /// <param name="retryCount">重试次数</param>
        /// <param name="retryDelay">重试延时</param>
        /// <returns></returns>
        Task<ICacheLock> BeginCacheLockAsync(string resourceName, string key, int retryCount = 0, TimeSpan retryDelay = new TimeSpan());
#endif
    }

    /// <summary>
    /// 公共缓存策略接口
    /// </summary>
    public interface IBaseCacheStrategy<TKey, TValue> /*: IBaseCacheLock*/
    //where TValue : class
    {
        /// <summary>
        /// 获取缓存中最终的键，如Container建议格式： return String.Format("{0}:{1}", "SenparcWeixinContainer", key);
        /// </summary>
        /// <param name="key"></param>
        /// <param name="isFullKey">是否已经是完整的Key，如果不是，则会调用一次GetFinalKey()方法</param>
        /// <returns></returns>
        string GetFinalKey(string key, bool isFullKey = false);

        #region 同步方法


        /// <summary>
        /// 添加指定ID的对象
        /// </summary>
        /// <param name="key">缓存键</param>
        /// <param name="value">缓存值</param>
        /// <param name="expiry">过期时间</param>
        [Obsolete("此方法已过期，请使用 Set(TKey key, TValue value) 方法")]
        void InsertToCache(TKey key, TValue value, TimeSpan? expiry = null);

        /// <summary>
        /// 添加指定ID的对象
        /// </summary>
        /// <param name="key">缓存键</param>
        /// <param name="value">缓存值</param>
        /// <param name="isFullKey">是否已经是完整的Key，如果不是，则会调用一次GetFinalKey()方法</param>
        /// <param name="expiry">过期时间</param>
        void Set(TKey key, TValue value, TimeSpan? expiry = null, bool isFullKey = false);

        /// <summary>
        /// 移除指定缓存键的对象
        /// </summary>
        /// <param name="key">缓存键</param>
        /// <param name="isFullKey">是否已经是完整的Key，如果不是，则会调用一次GetFinalKey()方法</param>
        void RemoveFromCache(TKey key, bool isFullKey = false);

        /// <summary>
        /// 返回指定缓存键的对象
        /// </summary>
        /// <param name="key">缓存键</param>
        /// <param name="isFullKey">是否已经是完整的Key，如果不是，则会调用一次GetFinalKey()方法</param>
        /// <returns></returns>
        TValue Get(TKey key, bool isFullKey = false);

        /// <summary>
        /// 返回指定缓存键的对象，并强制指定类型
        /// </summary>
        /// <param name="key">缓存键</param>
        /// <param name="isFullKey">是否已经是完整的Key，如果不是，则会调用一次GetFinalKey()方法</param>
        /// <returns></returns>
        T Get<T>(TKey key, bool isFullKey = false);

        /// <summary>
        /// 获取所有缓存信息集合
        /// </summary>
        /// <returns></returns>
        IDictionary<TKey, TValue> GetAll();

        /// <summary>
        /// 检查是否存在Key及对象
        /// </summary>
        /// <param name="key">缓存键</param>
        /// <param name="isFullKey">是否已经是完整的Key，如果不是，则会调用一次GetFinalKey()方法</param>
        /// <returns></returns>
        bool CheckExisted(TKey key, bool isFullKey = false);

        /// <summary>
        /// 获取缓存集合总数（注意：每个缓存框架的计数对象不一定一致！）
        /// </summary>
        /// <returns></returns>
        long GetCount();

        /// <summary>
        /// 更新缓存
        /// </summary>
        /// <param name="key">缓存键</param>
        /// <param name="value">缓存值</param>
        /// <param name="isFullKey">是否已经是完整的Key，如果不是，则会调用一次GetFinalKey()方法</param>
        /// <param name="expiry">过期时间</param>
        void Update(TKey key, TValue value, TimeSpan? expiry = null, bool isFullKey = false);

        #endregion

        #region 异步方法
#if !NET35 && !NET40

        /// <summary>
        /// 【异步方法】添加指定ID的对象
        /// </summary>
        /// <param name="key">缓存键</param>
        /// <param name="value">缓存值</param>
        /// <param name="isFullKey">是否已经是完整的Key，如果不是，则会调用一次GetFinalKey()方法</param>
        /// <param name="expiry">过期时间</param>
        Task SetAsync(TKey key, TValue value, TimeSpan? expiry = null, bool isFullKey = false);

        /// <summary>
        /// 【异步方法】移除指定缓存键的对象
        /// </summary>
        /// <param name="key">缓存键</param>
        /// <param name="isFullKey">是否已经是完整的Key，如果不是，则会调用一次GetFinalKey()方法</param>
        Task RemoveFromCacheAsync(TKey key, bool isFullKey = false);

        /// <summary>
        /// 【异步方法】返回指定缓存键的对象
        /// </summary>
        /// <param name="key">缓存键</param>
        /// <param name="isFullKey">是否已经是完整的Key，如果不是，则会调用一次GetFinalKey()方法</param>
        /// <returns></returns>
        Task<TValue> GetAsync(TKey key, bool isFullKey = false);

        /// <summary>
        /// 【异步方法】返回指定缓存键的对象，并强制指定类型
        /// </summary>
        /// <param name="key">缓存键</param>
        /// <param name="isFullKey">是否已经是完整的Key，如果不是，则会调用一次GetFinalKey()方法</param>
        /// <returns></returns>
        Task<T> GetAsync<T>(TKey key, bool isFullKey = false);

        /// <summary>
        /// 【异步方法】获取所有缓存信息集合
        /// </summary>
        /// <returns></returns>
        Task<IDictionary<TKey, TValue>> GetAllAsync();

        /// <summary>
        /// 【异步方法】检查是否存在Key及对象
        /// </summary>
        /// <param name="key">缓存键</param>
        /// <param name="isFullKey">是否已经是完整的Key，如果不是，则会调用一次GetFinalKey()方法</param>
        /// <returns></returns>
        Task<bool> CheckExistedAsync(TKey key, bool isFullKey = false);

        /// <summary>
        /// 【异步方法】获取缓存集合总数（注意：每个缓存框架的计数对象不一定一致！）
        /// </summary>
        /// <returns></returns>
        Task<long> GetCountAsync();

        /// <summary>
        /// 【异步方法】更新缓存
        /// </summary>
        /// <param name="key">缓存键</param>
        /// <param name="value">缓存值</param>
        /// <param name="isFullKey">是否已经是完整的Key，如果不是，则会调用一次GetFinalKey()方法</param>
        /// <param name="expiry">过期时间</param>
        Task UpdateAsync(TKey key, TValue value, TimeSpan? expiry = null, bool isFullKey = false);
#endif
        #endregion
    }
}
