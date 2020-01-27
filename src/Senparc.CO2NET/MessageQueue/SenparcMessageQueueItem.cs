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
    
    文件名：SenparcMessageQueueItem.cs
    文件功能描述：SenparcMessageQueue消息队列项
    
    
    创建标识：Senparc - 20151226
    

----  CO2NET   ----
----  split from Senparc.Weixin/MessageQueue/SenparcMessageQueueItem.cs  ----

    修改标识：Senparc - 20180601
    修改描述：v0.1.0 移植 SenparcMessageQueueItem

    修改标识：Senparc - 20181226
    修改描述：v0.4.3 修改 DateTime 为 DateTimeOffset
----------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Senparc.CO2NET.MessageQueue
{
    /// <summary>
    /// SenparcMessageQueue消息队列项
    /// </summary>
    public class SenparcMessageQueueItem
    {
        /// <summary>
        /// 队列项唯一标识
        /// </summary>
        public string Key { get; set; }
        /// <summary>
        /// 队列项目命中触发时执行的委托
        /// </summary>
        public Action Action { get; set; }
        /// <summary>
        /// 此实例对象的创建时间
        /// </summary>
        public DateTimeOffset AddTime { get; set; }
        /// <summary>
        /// 项目说明（主要用于调试）
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 初始化SenparcMessageQueue消息队列项
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
