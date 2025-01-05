#region Apache License Version 2.0
/*----------------------------------------------------------------

Copyright 2025 Suzhou Senparc Network Technology Co.,Ltd.

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
    Copyright (C) 2025 Senparc

    FileName：LocalCacheLock.cs
    File Function Description：Local lock


    Creation Identifier：Senparc - 20160810

    Modification Identifier：Senparc - 20170205
    Modification Description：1. Changed default retryDelay to 10 milliseconds, retryCount to 99999, total time to 16.6 minutes
                              2. Updated constructor
                              3. Refactored methods

    Modification Identifier：Senparc - 20210911
    Modification Description：v1.5.2 Added a check for successful lock before releasing LocalCacheLock

    Modification Identifier：Senparc - 20230528
    Modification Description：v2.1.8 Changed LockPool to ConcurrentDictionary

----------------------------------------------------------------*/

using Senparc.CO2NET.Trace;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Senparc.CO2NET.Cache
{
    /// <summary>
    /// Local lock
    /// </summary>
    public class LocalCacheLock : BaseCacheLock
    {
        private LocalObjectCacheStrategy _localStrategy;


        //This must be a non-public constructor, use the Create() method to create
        protected LocalCacheLock(LocalObjectCacheStrategy strategy, string resourceName, string key,
            int? retryCount = null, TimeSpan? retryDelay = null)
            : base(strategy, resourceName, key, retryCount ?? 0, retryDelay ?? TimeSpan.FromMilliseconds(10))
        {
            _localStrategy = strategy;
            //LockNow();//Wait immediately and seize the lock
        }

        /// <summary>
        /// Lock storage container   TODO: Consider distributed scenario — use Redis directly
        /// </summary>
        private static ConcurrentDictionary<string, object> LockPool = new ConcurrentDictionary<string, object>();
        /// <summary>
        /// Random number
        /// </summary>
        private static Random _rnd = new Random();
        /// <summary>
        /// Lock for reading LockPool
        /// </summary>
        private static object lookPoolLock = new object();


        #region 同步方法


        /// <summary>
        /// Create a LocalCacheLock instance and immediately try to acquire the lock
        /// </summary>
        /// <param name="strategy">LocalObjectCacheStrategy</param>
        /// <param name="resourceName"></param>
        /// <param name="key"></param>
        /// <param name="retryCount"></param>
        /// <param name="retryDelay"></param>
        /// <returns></returns>
        public static ICacheLock CreateAndLock(IBaseCacheStrategy strategy, string resourceName, string key, int? retryCount = null, TimeSpan? retryDelay = null)
        {
            return new LocalCacheLock(strategy as LocalObjectCacheStrategy, resourceName, key, retryCount, retryDelay).Lock();
        }

        /// <summary>
        /// Wait immediately and seize the lock
        /// </summary>
        /// <returns></returns>
        public override ICacheLock Lock()
        {
            int currentRetry = 0;
            int maxRetryDelay = (int)_retryDelay.TotalMilliseconds;
            while (currentRetry++ < _retryCount)
            {
                #region 尝试获得锁

                var getLock = false;
                try
                {
                    lock (lookPoolLock)
                    {
                        if (LockPool.ContainsKey(_resourceName))
                        {
                            getLock = false;//Locked by someone else, failed to acquire the lock
                        }
                        else
                        {
                            LockPool.TryAdd(_resourceName, new object());//Create lock
                            getLock = true;//Acquire lock
                        }
                    }
                }
                catch (Exception ex)
                {
                    SenparcTrace.Log("本地同步锁发生异常：" + ex.Message);
                    getLock = false;
                }

                #endregion

                if (getLock)
                {
                    LockSuccessful = true;
                    return this;//Acquire lock
                }
                Thread.Sleep(_rnd.Next(maxRetryDelay));
            }
            LockSuccessful = false;
            return this;
        }

        public override void UnLock()
        {
            if (LockSuccessful)
            {
                lock (lookPoolLock)
                {
                    LockPool.TryRemove(_resourceName, out _);
                }
            }
        }

        #endregion

        #region 异步方法

        /// <summary>
        /// [Async method] Create a LocalCacheLock instance and immediately try to acquire the lock
        /// </summary>
        /// <param name="strategy">LocalObjectCacheStrategy</param>
        /// <param name="resourceName"></param>
        /// <param name="key"></param>
        /// <param name="retryCount"></param>
        /// <param name="retryDelay"></param>
        /// <returns></returns>
        public static async Task<ICacheLock> CreateAndLockAsync(IBaseCacheStrategy strategy, string resourceName, string key, int? retryCount = null, TimeSpan? retryDelay = null)
        {
            return await new LocalCacheLock(strategy as LocalObjectCacheStrategy, resourceName, key, retryCount, retryDelay).LockAsync().ConfigureAwait(false);
        }

        public override async Task<ICacheLock> LockAsync()
        {
            //TODO: Exception handling

            //return await Task.Factory.StartNew(() => Lock()).ConfigureAwait(false);
            return Lock();//Use synchronous method here to complete the lock
        }

        public override async Task UnLockAsync()
        {
            UnLock();
            //return System.Threading.Tasks.TaskExtension.CompletedTask();
            return;
        }
        #endregion
    }
}
