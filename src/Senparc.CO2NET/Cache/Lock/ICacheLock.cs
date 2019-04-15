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


#region Apache License Version 2.0

#endregion Apache License Version 2.0

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Senparc.CO2NET.Cache
{
    /// <summary>
    /// 缓存锁接口
    /// </summary>
    public interface ICacheLock : IDisposable
    {
        /// <summary>
        /// 是否成功获得锁
        /// </summary>
        bool LockSuccessful { get; set; }

        ///// <summary>
        ///// 立即开始锁定
        ///// </summary>
        ///// <returns></returns>
        //ICacheLock LockNow();

        /// <summary>
        /// 获取最长锁定时间（锁最长生命周期）
        /// </summary>
        /// <param name="retryCount">重试次数，</param>
        /// <param name="retryDelay">最小锁定时间周期</param>
        /// <returns>单位：Milliseconds，毫秒</returns>
        double GetTotalTtl(int retryCount, TimeSpan retryDelay);

        /// <summary>
        /// 创建 ICacheLock 实例
        /// </summary>
        /// <param name="strategy"></param>
        /// <param name="resourceName"></param>
        /// <param name="key"></param>
        /// <param name="retryCount"></param>
        /// <param name="retryDelay"></param>
        /// <returns></returns>
        ICacheLock Create(IBaseCacheStrategy strategy, string resourceName, string key,
            int? retryCount = null, TimeSpan? retryDelay = null);

        #region 同步方法

        /// <summary>
        /// 开始锁
        /// </summary>
        ICacheLock Lock();

        /// <summary>
        /// 释放锁
        /// </summary>
        void UnLock();

        #endregion

        #region 异步方法
#if !NET35 && !NET40

        /// <summary>
        /// 开始锁
        /// </summary>
        Task<ICacheLock> LockAsync();

        /// <summary>
        /// 释放锁
        /// </summary>
        Task UnLockAsync();

#endif
        #endregion

    }
}
