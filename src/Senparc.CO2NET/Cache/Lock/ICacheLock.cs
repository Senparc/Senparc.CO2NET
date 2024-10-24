#region Apache License Version 2.0
/*----------------------------------------------------------------

Copyright 2024 Suzhou Senparc Network Technology Co.,Ltd.

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

    FileName：ICacheLock.cs
    File Function Description：CacheLock Interface


    Creation Identifier：Senparc - 20180106

    Modification Identifier：Senparc - 20230527
    Modification Description：v2.1.8 Added GetLockCacheKey() method 

----------------------------------------------------------------*/

using System;
using System.Threading.Tasks;

namespace Senparc.CO2NET.Cache
{
    /// <summary>
    /// Cache lock interface
    /// </summary>
    public interface ICacheLock : IDisposable
    {
        /// <summary>
        /// Whether the lock was successfully acquired
        /// </summary>
        bool LockSuccessful { get; set; }

        ///// <summary>
        ///// Lock immediately
        ///// </summary>
        ///// <returns></returns>
        //ICacheLock LockNow();

        /// <summary>
        /// Get the maximum lock time (maximum lock lifecycle)
        /// </summary>
        /// <param name="retryCount">Retry count,</param>
        /// <param name="retryDelay">Minimum lock time period</param>
        /// <returns>Unit: Milliseconds</returns>
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
        /// Start lock
        /// </summary>
        ICacheLock Lock();

        /// <summary>
        /// Release lock
        /// </summary>
        void UnLock();

        #endregion

        #region 异步方法

        /// <summary>
        /// Start lock
        /// </summary>
        Task<ICacheLock> LockAsync();

        /// <summary>
        /// Release lock
        /// </summary>
        Task UnLockAsync();

        #endregion

    }
}
