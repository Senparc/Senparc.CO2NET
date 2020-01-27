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

    文件名：MessageQueueDictionary.cs
    文件功能描述：MessageQueueDictionary 记录队列的本地内存字典


    创建标识：Senparc - 20181118

    修改标识：Senparc - 20190812
    修改描述：v4.5.10 改用基类 Dictionary 为 ConcurrentDictionary

----------------------------------------------------------------*/

using System;
using System.Collections.Concurrent;


namespace Senparc.CO2NET.MessageQueue
{
    public class MessageQueueDictionary : ConcurrentDictionary<string, SenparcMessageQueueItem>
    {
        //public List<string> KeyList { get; set; } = new List<string>();
        public MessageQueueDictionary()
            : base(StringComparer.OrdinalIgnoreCase)
        {

        }
    }
}
