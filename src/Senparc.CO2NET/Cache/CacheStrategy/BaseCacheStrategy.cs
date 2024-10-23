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

    FileName：BaseCacheStrategy.cs
    File Function Description：Generic cache strategy base class.


    Creation Identifier：Senparc - 20160813 v4.7.7 


    --CO2NET--

    Modification Identifier：Senparc - 20180606
    Modification Description：Added virtual method keyword to GetFinalKey() method

 ----------------------------------------------------------------*/


using System;
using System.Threading.Tasks;

namespace Senparc.CO2NET.Cache
{
    /// <summary>
    /// Generic cache strategy base class
    /// </summary>
    public abstract class BaseCacheStrategy : IBaseCacheStrategy
    {
        ///// <summary>
        ///// Default child namespace
        ///// </summary>
        //public virtual string ChildNamespace { get; set; }

        /// <summary>
        /// Get the assembled FinalKey
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="isFullKey">Whether it is already the concatenated FullKey</param>
        /// <returns></returns>
        public virtual string GetFinalKey(string key, bool isFullKey = false)
        {
            return isFullKey ? key : $"Senparc:{Config.DefaultCacheNamespace}:{key}";
        }

        /// <summary>
        /// Get a synchronization lock
        /// </summary>
        /// <param name="resourceName"></param>
        /// <param name="key"></param>
        /// <param name="retryCount"></param>
        /// <param name="retryDelay"></param>
        /// <returns></returns>
        public abstract ICacheLock BeginCacheLock(string resourceName, string key, int retryCount = 0, TimeSpan retryDelay = new TimeSpan());


        /// <summary>
        /// [Async method] Create a (distributed) lock
        /// </summary>
        /// <param name="resourceName">Resource name</param>
        /// <param name="key">Key identifier</param>
        /// <param name="retryCount">Retry count</param>
        /// <param name="retryDelay">Retry delay</param>
        /// <returns></returns>
        public abstract Task<ICacheLock> BeginCacheLockAsync(string resourceName, string key, int retryCount = 0, TimeSpan retryDelay = new TimeSpan());
    }
}
