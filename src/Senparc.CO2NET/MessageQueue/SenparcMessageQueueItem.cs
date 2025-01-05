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
    
    FileName：SenparcMessageQueueItem.cs
    File Function Description：SenparcMessageQueue message queue item
    
    
    Creation Identifier：Senparc - 20151226
    

----  CO2NET   ----
----  split from Senparc.Weixin/MessageQueue/SenparcMessageQueueItem.cs  ----

    Modification Identifier：Senparc - 20180601
    Modification Description：v0.1.0 migrated SenparcMessageQueueItem

    Modification Identifier：Senparc - 20181226
    Modification Description：v0.4.3 changed DateTime to DateTimeOffset
----------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Senparc.CO2NET.MessageQueue
{
    /// <summary>
    /// SenparcMessageQueue message queue item
    /// </summary>
    public class SenparcMessageQueueItem
    {
        /// <summary>
        /// Unique identifier for the queue item
        /// </summary>
        public string Key { get; set; }
        /// <summary>
        /// Delegate executed when the queue item is triggered
        /// </summary>
        public Action Action { get; set; }
        /// <summary>
        /// Creation time of this instance
        /// </summary>
        public DateTimeOffset AddTime { get; set; }
        /// <summary>
        /// Item description (mainly for debugging)
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Initialize SenparcMessageQueue message queue item
        /// </summary>
        /// <param name="key"></param>
        /// <param name="action"></param>
        /// <param name="description"></param>
        public SenparcMessageQueueItem(string key, Action action, string description = null)
        {
            Key = key;
            Action = action;
            Description = description;
            AddTime = SystemTime.Now;
        }
    }
}
