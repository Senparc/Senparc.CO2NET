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

    文件名：DataItem.cs
    文件功能描述：日志记录的最小单位


    创建标识：Senparc - 20181117

    修改标识：Senparc - 20181226
    修改描述：v0.4.3 修改 DateTime 为 DateTimeOffset

 ----------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Text;

namespace Senparc.CO2NET.APM
{
    /// <summary>
    /// 日志记录的最小单位
    /// </summary>
    public class DataItem
    {
        /// <summary>
        /// 统计类别名称
        /// </summary>
        public string KindName { get; set; }
        /// <summary>
        /// 统计时间
        /// </summary>
        public DateTimeOffset DateTime { get; set; }
        /// <summary>
        /// 统计值
        /// </summary>
        public double Value { get; set; }
        /// <summary>
        /// 复杂类型数据
        /// </summary>
        public object Data { get; set; }
        /// <summary>
        /// 临时储存（不会对外传递）
        /// </summary>
        public object TempStorage { get; set; }
    }
}
