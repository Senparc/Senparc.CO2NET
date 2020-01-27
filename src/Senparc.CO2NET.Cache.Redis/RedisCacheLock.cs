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

    文件名：RedisCacheLock.cs
    文件功能描述：本地锁


    创建标识：Senparc - 20160810

    修改标识：Senparc - 20170205
    修改描述：v1.2.0 重构分布式锁

    修改标识：Senparc - 20191413
    修改描述：v3.5.0 提供缓存异步接口

----------------------------------------------------------------*/

using System;
using System.Threading.Tasks;
using Redlock.CSharp;

namespace Senparc.CO2NET.Cache.Redis
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
            //LockNow();//立即等待并抢夺锁
        }

        #region 同步方法

        /// <summary>
        /// 创建 RedisCacheLock 实例，并立即尝试获得锁
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

        #region 异步方法
#if !NET35 && !NET40

        /// <summary>
        /// 【异步方法】创建 RedisCacheLock 实例，并立即尝试获得锁
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

#endif
        #endregion


    }
}
