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

    FileName：LocalCacheLock.cs
    File Function Description：Local Lock


    Creation Identifier：Senparc - 20160810

    Modification Identifier：Senparc - 20170205
    Modification Description：v4.11.0 Refactor distributed lock

    Modification Identifier：Senparc - 20190412
    Modification Description：v0.6.0 Provide asynchronous cache interface

    Modification Identifier：Senparc - 20230527
    Modification Description：v2.1.8 Add GetLockCacheKey() method 

----------------------------------------------------------------*/

using System;
using System.Threading.Tasks;

namespace Senparc.CO2NET.Cache
{
    /// <summary>
    /// Base class for cache synchronization lock
    /// </summary>
    public abstract class BaseCacheLock : ICacheLock
    {
        protected string _resourceName;
        protected IBaseCacheStrategy _cacheStrategy;
        protected int _retryCount;
        protected TimeSpan _retryDelay;

        protected bool _isSyncLock;//Whether to use asynchronous method to call the lock

        public bool LockSuccessful { get; set; }

        /// <summary>
        /// Default retry count
        /// </summary>
        public readonly int DefaultRetryCount = 20;
        /// <summary>
        /// Default retry interval time
        /// </summary>
        public readonly TimeSpan DefaultRetryDelay = TimeSpan.FromMilliseconds(10);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="strategy"></param>
        /// <param name="resourceName"></param>
        /// <param name="key"></param>
        /// <param name="retryCount">If null, default is 50</param>
        /// <param name="retryDelay">If null, default is 10 milliseconds</param>
        protected BaseCacheLock(IBaseCacheStrategy strategy, string resourceName, string key, int? retryCount, TimeSpan? retryDelay)
        {
            _cacheStrategy = strategy;
            _resourceName = GetLockCacheKey(resourceName,key);/*Adding Key can lock for a specific AppId*/
            _retryCount = retryCount == null || retryCount == 0 ? DefaultRetryCount : retryCount.Value;
            _retryDelay = retryDelay == null || retryDelay.Value.TotalMilliseconds == 0 ? DefaultRetryDelay : retryDelay.Value;
        }

        /// <summary>
        /// Get the combined Key in the cache
        /// </summary>
        /// <param name="resourceName"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public string GetLockCacheKey(string resourceName, string key)
        {
            return resourceName + key;
        }

        /// <summary>
        /// Get the maximum lock time (maximum lock lifecycle), unit: milliseconds
        /// </summary>
        /// <param name="retryCount">Retry count,</param>
        /// <param name="retryDelay">Minimum lock time period</param>
        /// <returns>Unit: Milliseconds</returns>
        public double GetTotalTtl(int retryCount, TimeSpan retryDelay)
        {
            var ttl = retryDelay.TotalMilliseconds * retryCount;
            return ttl;
        }

        public void Dispose()
        {
            //Must be true
            Dispose(true);
            //Notify the garbage collector not to call the finalizer (destructor)
            GC.SuppressFinalize(this);
        }

        ///<summary>
        /// Must, in case the programmer forgets to explicitly call the Dispose method
        ///</summary>
        ~BaseCacheLock()
        {
            //Must be false
            Dispose(false);
        }

        protected bool disposed = false;

        ///<summary>
        /// Use protected virtual for non-sealed classes
        /// Use private for sealed classes
        ///</summary>
        ///<param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
            {
                return;
            }

            UnLockAsync().ConfigureAwait(false).GetAwaiter().GetResult();

            if (disposing)
            {
                //Clean up managed resources
            }

            //Let the type know it has been released
            disposed = true;
        }

        /// <summary>
        /// Start locking immediately, needs to be executed in the constructor of the subclass
        /// </summary>
        /// <returns></returns>


        #region 同步方法

        //protected ICacheLock LockNow()
        //{
        //    if (_retryCount != 0 && _retryDelay.Ticks != 0)
        //    {
        //        LockSuccessful = Lock(_resourceName, _retryCount, _retryDelay);
        //    }
        //    else
        //    {
        //        LockSuccessful = Lock(_resourceName);
        //    }
        //    return this;
        //}

        public abstract ICacheLock Lock();

        public abstract void UnLock();

        #endregion

        #region 异步方法

        //protected async Task<ICacheLock> LockNowAsync()
        //{
        //    if (_retryCount != 0 && _retryDelay.Ticks != 0)
        //    {
        //        LockSuccessful = await LockAsync(_resourceName, _retryCount, _retryDelay).ConfigureAwait(false);
        //    }
        //    else
        //    {
        //        LockSuccessful = await LockAsync(_resourceName).ConfigureAwait(false);
        //    }
        //    return this;
        //}

        public abstract Task<ICacheLock> LockAsync();

        public abstract Task UnLockAsync();

        #endregion

    }
}
