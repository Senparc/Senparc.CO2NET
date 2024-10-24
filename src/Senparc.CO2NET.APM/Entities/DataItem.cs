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

    FileName: DataItem.cs
    File Function Description: The smallest unit of log recording


    Creation Identifier: Senparc - 20181117

    Modification Identifier: Senparc - 20181226
    Modification Description: v0.4.3 Changed DateTime to DateTimeOffset

 ----------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Text;

namespace Senparc.CO2NET.APM
{
    /// <summary>
    /// The smallest unit of log recording
    /// </summary>
    public class DataItem
    {
        /// <summary>
        /// Statistical category name
        /// </summary>
        public string KindName { get; set; }
        /// <summary>
        /// Statistical time
        /// </summary>
        public DateTimeOffset DateTime { get; set; }
        /// <summary>
        /// Statistical value
        /// </summary>
        public double Value { get; set; }
        /// <summary>
        /// Complex type data
        /// </summary>
        public object Data { get; set; }
        /// <summary>
        /// Temporary storage (will not be passed externally)
        /// </summary>
        public object TempStorage { get; set; }
    }
}
