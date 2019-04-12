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
    Copyright (C) 2018 Senparc

    文件名：LocalCacheLock.cs
    文件功能描述：本地锁


    创建标识：Senparc - 20160810

    修改标识：Senparc - 20170205
    修改描述：v4.11.0 重构分布式锁

    修改标识：Senparc - 20190412
    修改描述：v0.6.0 提供缓存异步接口

----------------------------------------------------------------*/

using System;
using System.Threading.Tasks;

namespace Senparc.CO2NET.Cache
{
    /// <summary>
    /// 缓存同步锁基类
    /// </summary>
    public abstract class BaseCacheLock : ICacheLock
    {
        protected string _resourceName;
        protected IBaseCacheStrategy _cacheStrategy;
        protected int _retryCount;
        protected TimeSpan _retryDelay;
        public bool LockSuccessful { get; set; }

        protected BaseCacheLock(IBaseCacheStrategy strategy, string resourceName, string key, int retryCount, TimeSpan retryDelay)
        {
            _cacheStrategy = strategy;
            _resourceName = resourceName + key;/*加上Key可以针对某个AppId加锁*/
            _retryCount = retryCount;
            _retryDelay = retryDelay;
        }

        /// <summary>
        /// 获取最长锁定时间（锁最长生命周期）
        /// </summary>
        /// <param name="retryCount">重试次数，</param>
        /// <param name="retryDelay">最小锁定时间周期</param>
        /// <returns>单位：Milliseconds，毫秒</returns>
        public double GetTotalTtl(int retryCount, TimeSpan retryDelay)
        {
            var ttl = (retryDelay.TotalMilliseconds > 0 ? retryDelay.TotalMilliseconds : 10)
                  *
                 (retryCount > 0 ? retryCount : 10);
            return ttl;
        }

        public void Dispose()
        {
            UnLock(_resourceName);
        }

        /// <summary>
        /// 立即开始锁定，需要在子类的构造函数中执行
        /// </summary>
        /// <returns></returns>
        protected ICacheLock LockNow()
        {
            if (_retryCount != 0 && _retryDelay.Ticks != 0)
            {
                LockSuccessful = Lock(_resourceName, _retryCount, _retryDelay);
            }
            else
            {
                LockSuccessful = Lock(_resourceName);
            }
            return this;
        }

        #region 同步方法

        public abstract bool Lock(string resourceName);

        public abstract bool Lock(string resourceName, int retryCount, TimeSpan retryDelay);

        public abstract void UnLock(string resourceName);

        #endregion

        #region 异步方法

        public abstract Task<bool> LockAsync(string resourceName);

        public abstract Task<bool> LockAsync(string resourceName, int retryCount, TimeSpan retryDelay);

        public abstract Task UnLockAsync(string resourceName);

        #endregion

    }
}
