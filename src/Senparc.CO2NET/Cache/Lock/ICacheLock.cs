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

    文件名：ICacheLock.cs
    文件功能描述：CacheLock 接口


    创建标识：Senparc - 20180106

    修改标识：Senparc - 20230527
    修改描述：v2.1.8 添加 GetLockCacheKey() 方法 

----------------------------------------------------------------*/

using System;
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
        /// <inheritdoc/>
        /// </summary>
        /// <param name="resourceName">resourceName</param>
        /// <param name="key">key</param>
        /// <returns></returns>
        string GetLockCacheKey(string resourceName, string key);

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

        /// <summary>
        /// 开始锁
        /// </summary>
        Task<ICacheLock> LockAsync();

        /// <summary>
        /// 释放锁
        /// </summary>
        Task UnLockAsync();

        #endregion

    }
}
