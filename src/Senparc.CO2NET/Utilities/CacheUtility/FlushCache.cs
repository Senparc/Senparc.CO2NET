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
    Copyright(C) 2024 Senparc

    FileName：FlushCache.cs
    File Function Description：Method for immediate cache effect


    Creation Identifier：Senparc - 20160318

----------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Senparc.CO2NET.MessageQueue;

namespace Senparc.CO2NET.CacheUtility
{
    /// <summary>
    /// Method for immediate cache effect
    /// </summary>
    public class FlushCache : IDisposable
    {
        /// <summary>
        /// Whether to update the cache immediately
        /// </summary>
        public bool DoFlush { get; set; }

        /// <summary>
        ///
        /// </summary>
        /// <param name="doFlush">Whether to update the cache immediately</param>
        public FlushCache(bool doFlush = true)
        {
            DoFlush = doFlush;
        }

        /// <summary>
        /// Release, start updating all caches immediately
        /// </summary>
        public void Dispose()
        {
            if (DoFlush)
            {
                SenparcMessageQueue.OperateQueue();
            }
        }

        /// <summary>
        /// Create a FlushCache instance
        /// </summary>
        /// <param name="doFlush">Whether to update the cache immediately</param>
        /// <returns></returns>
        public static FlushCache CreateInstance(bool doFlush = true)
        {
            return new FlushCache(doFlush);
        }
    }
}
