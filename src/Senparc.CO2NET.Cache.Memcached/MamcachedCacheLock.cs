/*----------------------------------------------------------------
    Copyright (C) 2020 Senparc

    文件名：RedisCacheLock.cs
    文件功能描述：本地锁


    创建标识：Senparc - 20160810

    修改标识：Senparc - 20170205
    修改描述：v0.2.0 重构分布式锁

    修改标识：spadark - 20170419
    修改描述：v0.3.0 Memcached同步锁改为使用StoreMode.Add方法

----------------------------------------------------------------*/


using System;
using System.Threading;
using System.Threading.Tasks;
using Enyim.Caching.Memcached;
using Senparc.CO2NET.Cache;
using Senparc.CO2NET.Trace;

namespace Senparc.CO2NET.Cache.Memcached
{
    public class MemcachedCacheLock : BaseCacheLock
    {
        private MemcachedObjectCacheStrategy _mamcachedStrategy;
        protected MemcachedCacheLock(MemcachedObjectCacheStrategy strategy, string resourceName, string key, int? retryCount, TimeSpan? retryDelay)
            : base(strategy, resourceName, key, retryCount, retryDelay)
        {
            _mamcachedStrategy = strategy;
            //LockNow();//立即等待并抢夺锁
        }

        private static Random _rnd = new Random();

        private string GetLockKey(string resourceName)
        {
            return string.Format("{0}:{1}", "Lock", resourceName);
        }

        #region 同步方法

        /// <summary>
        /// 创建 MemcachedCacheLock 实例，并立即尝试获得锁
        /// </summary>
        /// <param name="strategy">MemcachedObjectCacheStrategy</param>
        /// <param name="resourceName"></param>
        /// <param name="key"></param>
        /// <param name="retryCount"></param>
        /// <param name="retryDelay"></param>
        /// <returns></returns>
        public static ICacheLock CreateAndLock(IBaseCacheStrategy strategy, string resourceName, string key, int? retryCount = null, TimeSpan? retryDelay = null)
        {
            return new MemcachedCacheLock(strategy as MemcachedObjectCacheStrategy, resourceName, key, retryCount, retryDelay).Lock();
        }


        private ICacheLock RetryLock(string resourceName, int retryCount, TimeSpan retryDelay, Func<bool> action)
        {
            int currentRetry = 0;
            int maxRetryDelay = (int)retryDelay.TotalMilliseconds;
            while (currentRetry++ < retryCount)
            {
                if (action())
                {
                    base.LockSuccessful = true;
                    return this;//取得锁
                }
                Thread.Sleep(_rnd.Next(maxRetryDelay));
            }
            return this;
        }

        public override ICacheLock Lock()
        {
            var key = _mamcachedStrategy.GetFinalKey(_resourceName);
            var lockResult = RetryLock(key, _retryCount, _retryDelay, () =>
            {
                try
                {
                    var ttl = base.GetTotalTtl(_retryCount, _retryDelay);
                    if (_mamcachedStrategy.Cache.Store(StoreMode.Add, key, new object(), TimeSpan.FromMilliseconds(ttl)))
                    {
                        base.LockSuccessful = true;
                        return true;//取得锁 
                    }
                    else
                    {
                        base.LockSuccessful = false;
                        return false;//已被别人锁住，没有取得锁
                    }

                    //if (_mamcachedStrategy._cache.Get(key) != null)
                    //{
                    //    return false;//已被别人锁住，没有取得锁
                    //}
                    //else
                    //{
                    //    _mamcachedStrategy._cache.Store(StoreMode.set, key, new object(), new TimeSpan(0, 0, 10));//创建锁
                    //    return true;//取得锁
                    //}
                }
                catch (Exception ex)
                {
                    SenparcTrace.Log("Memcached同步锁发生异常：" + ex.Message);
                    return false;
                }
            }
              );
            return lockResult;
        }

        public override void UnLock()
        {
            var key = _mamcachedStrategy.GetFinalKey(_resourceName);
            _mamcachedStrategy.Cache.Remove(key);
        }

        #endregion

        #region 异步方法
#if !NET35 && !NET40

        /// <summary>
        /// 【异步方法】创建 MemcachedCacheLock 实例，并立即尝试获得锁
        /// </summary>
        /// <param name="strategy">MemcachedObjectCacheStrategy</param>
        /// <param name="resourceName"></param>
        /// <param name="key"></param>
        /// <param name="retryCount"></param>
        /// <param name="retryDelay"></param>
        /// <returns></returns>
        public static async Task<ICacheLock> CreateAndLockAsync(IBaseCacheStrategy strategy, string resourceName, string key, int? retryCount = null, TimeSpan? retryDelay = null)
        {
            return await new MemcachedCacheLock(strategy as MemcachedObjectCacheStrategy, resourceName, key, retryCount, retryDelay).LockAsync().ConfigureAwait(false);
        }

        private async Task<ICacheLock> RetryLockAsync(string resourceName, int retryCount, TimeSpan retryDelay, Func<Task<bool>> action)
        {
            int currentRetry = 0;
            int maxRetryDelay = (int)retryDelay.TotalMilliseconds;
            while (currentRetry++ < retryCount)
            {
                if (await action().ConfigureAwait(false))
                {
                    base.LockSuccessful = true;
                    return this;//取得锁
                }
                Thread.Sleep(_rnd.Next(maxRetryDelay));
            }
            return this;
        }

        public override async Task<ICacheLock> LockAsync()
        {
            var key = _mamcachedStrategy.GetFinalKey(_resourceName);
            var lockResult = await RetryLockAsync(key, _retryCount, _retryDelay, async () =>
             {
                 try
                 {
                     var ttl = base.GetTotalTtl(_retryCount, _retryDelay);
#if NET45
                     var storeResult = await Task.Factory.StartNew(() => _mamcachedStrategy.Cache.Store(StoreMode.Add, key, new object(), TimeSpan.FromMilliseconds(ttl))).ConfigureAwait(false);
                     if (storeResult)
#else
                    if (await _mamcachedStrategy.Cache.StoreAsync(StoreMode.Add, key, new object(), TimeSpan.FromMilliseconds(ttl)))
#endif
                     {
                         base.LockSuccessful = true;
                         return true;//取得锁 
                     }
                     else
                     {
                         base.LockSuccessful = false;
                         return false;//已被别人锁住，没有取得锁
                     }

                     //if (_mamcachedStrategy._cache.Get(key) != null)
                     //{
                     //    return false;//已被别人锁住，没有取得锁
                     //}
                     //else
                     //{
                     //    _mamcachedStrategy._cache.Store(StoreMode.set, key, new object(), new TimeSpan(0, 0, 10));//创建锁
                     //    return true;//取得锁
                     //}
                 }
                 catch (Exception ex)
                 {
                     SenparcTrace.Log("Memcached同步锁发生异常：" + ex.Message);
                     return false;
                 }
             }
              ).ConfigureAwait(false);
            return lockResult;
        }

        public override async Task UnLockAsync()
        {
            var key = _mamcachedStrategy.GetFinalKey(_resourceName);
#if NET45
            await Task.Factory.StartNew(() => _mamcachedStrategy.Cache.Remove(key)).ConfigureAwait(false);
#else
            await _mamcachedStrategy.Cache.RemoveAsync(key).ConfigureAwait(false);
#endif
        }

#endif
        #endregion
    }
}
