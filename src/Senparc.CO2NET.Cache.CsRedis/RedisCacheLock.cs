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

    FileName: RedisCacheLock.cs
    File Function Description: Local lock


    Creation Identifier: Senparc - 20160810

    Modification Identifier: Senparc - 20170205
    Modification Description: v1.2.0 Refactor distributed lock

    Modification Identifier: Senparc - 20191413
    Modification Description: v3.5.0 Provide asynchronous cache interface

----------------------------------------------------------------*/

using System;
using System.Threading.Tasks;
using Redlock.CSharp;

namespace Senparc.CO2NET.Cache.CsRedis
{
    public class RedisCacheLock : BaseCacheLock
    {
        private Redlock.CSharp.Redlock _dlm;
        private Lock _lockObject;

        private BaseRedisObjectCacheStrategy _redisStrategy;

        protected RedisCacheLock(BaseRedisObjectCacheStrategy strategy, string resourceName, string key, int? retryCount, TimeSpan? retryDelay)
            : base(strategy, resourceName, key, retryCount, retryDelay)
        {
            _redisStrategy = strategy;
            //LockNow();//Immediately wait and seize the lock
        }

        #region Synchronous Methods

        /// <summary>
        /// Create a RedisCacheLock instance and immediately attempt to acquire the lock
        /// </summary>
        /// <param name="strategy">BaseRedisObjectCacheStrategy</param>
        /// <param name="resourceName"></param>
        /// <param name="key"></param>
        /// <param name="retryCount"></param>
        /// <param name="retryDelay"></param>
        /// <returns></returns>
        public static ICacheLock CreateAndLock(IBaseCacheStrategy strategy, string resourceName, string key, int? retryCount = null, TimeSpan? retryDelay = null)
        {
            return new RedisCacheLock(strategy as BaseRedisObjectCacheStrategy, resourceName, key, retryCount, retryDelay).Lock();
        }

        public override ICacheLock Lock()
        {
            if (_retryCount != 0)
            {
                _dlm = new Redlock.CSharp.Redlock(_retryCount, _retryDelay, _redisStrategy.Client);
            }
            else if (_dlm == null)
            {
                _dlm = new Redlock.CSharp.Redlock(_redisStrategy.Client);
            }

            var ttl = base.GetTotalTtl(_retryCount, _retryDelay);
            base.LockSuccessful = _dlm.Lock(_resourceName, TimeSpan.FromMilliseconds(ttl), out _lockObject);
            return this;
        }

        public override void UnLock()
        {
            if (_lockObject != null)
            {
                _dlm.Unlock(_lockObject);
            }
        }

        #endregion

        #region Asynchronous Methods

        /// <summary>
        /// [Asynchronous method] Create a RedisCacheLock instance and immediately attempt to acquire the lock
        /// </summary>
        /// <param name="strategy">BaseRedisObjectCacheStrategy</param>
        /// <param name="resourceName"></param>
        /// <param name="key"></param>
        /// <param name="retryCount"></param>
        /// <param name="retryDelay"></param>
        /// <returns></returns>
        public static async Task<ICacheLock> CreateAndLockAsync(IBaseCacheStrategy strategy, string resourceName, string key, int? retryCount = null, TimeSpan? retryDelay = null)
        {
            return await new RedisCacheLock(strategy as BaseRedisObjectCacheStrategy, resourceName, key, retryCount, retryDelay).LockAsync().ConfigureAwait(false);
        }


        public override async Task<ICacheLock> LockAsync()
        {
            if (_retryCount != 0)
            {
                _dlm = new Redlock.CSharp.Redlock(_retryCount, _retryDelay, _redisStrategy.Client);
            }
            else if (_dlm == null)
            {
                _dlm = new Redlock.CSharp.Redlock(_redisStrategy.Client);
            }

            var ttl = base.GetTotalTtl(_retryCount, _retryDelay);

            Tuple<bool, Lock> result = await _dlm.LockAsync(_resourceName, TimeSpan.FromMilliseconds(ttl)).ConfigureAwait(false);
            base.LockSuccessful = result.Item1;
            _lockObject = result.Item2;
            return this;
        }

        public override async Task UnLockAsync()
        {
            if (_lockObject != null)
            {
                await _dlm.UnlockAsync(_lockObject).ConfigureAwait(false);
            }
        }

        #endregion


    }
}
