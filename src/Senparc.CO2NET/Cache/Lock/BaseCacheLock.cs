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

        protected bool _isSyncLock;//是否使用异步步的方法调用锁

        public bool LockSuccessful { get; set; }

        /// <summary>
        /// 默认重试次数
        /// </summary>
        public readonly int DefaultRetryCount = 20;
        /// <summary>
        /// 默认每次重试间隔时间
        /// </summary>
        public readonly TimeSpan DefaultRetryDelay = TimeSpan.FromMilliseconds(10);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="strategy"></param>
        /// <param name="resourceName"></param>
        /// <param name="key"></param>
        /// <param name="retryCount">如果传入null，默认为50</param>
        /// <param name="retryDelay">如果传入null，默认为10毫秒</param>
        protected BaseCacheLock(IBaseCacheStrategy strategy, string resourceName, string key, int? retryCount, TimeSpan? retryDelay)
        {
            _cacheStrategy = strategy;
            _resourceName = resourceName + key;/*加上Key可以针对某个AppId加锁*/
            _retryCount = retryCount == null || retryCount == 0 ? DefaultRetryCount : retryCount.Value;
            _retryDelay = retryDelay == null || retryDelay.Value.TotalMilliseconds == 0 ? DefaultRetryDelay : retryDelay.Value;
        }

        /// <summary>
        /// 获取最长锁定时间（锁最长生命周期），单位：毫秒
        /// </summary>
        /// <param name="retryCount">重试次数，</param>
        /// <param name="retryDelay">最小锁定时间周期</param>
        /// <returns>单位：Milliseconds，毫秒</returns>
        public double GetTotalTtl(int retryCount, TimeSpan retryDelay)
        {
            var ttl = retryDelay.TotalMilliseconds * retryCount;
            return ttl;
        }

        public void Dispose()
        {
            //必须为true
            Dispose(true);
            //通知垃圾回收机制不再调用终结器（析构器）
            GC.SuppressFinalize(this);
        }

        ///<summary>
        /// 必须，以备程序员忘记了显式调用Dispose方法
        ///</summary>
        ~BaseCacheLock()
        {
            //必须为false
            Dispose(false);
        }

        protected bool disposed = false;

        ///<summary>
        /// 非密封类修饰用protected virtual
        /// 密封类修饰用private
        ///</summary>
        ///<param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
            {
                return;
            }

#if !NET35 && !NET40
            UnLockAsync();
#else
            UnLock();//TODO：这里使用使用的是同步方法，可以改成根据调用 LockAsync 方法的情况主动调用异步方法
#endif

            if (disposing)
            {
                //清理托管资源
            }

            //让类型知道自己已经被释放
            disposed = true;
        }

        /// <summary>
        /// 立即开始锁定，需要在子类的构造函数中执行
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
#if !NET35 && !NET40

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

#endif
        #endregion

    }
}
