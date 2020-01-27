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

    文件名：MinuteData.cs
    文件功能描述：打包统计的每分钟数据


    创建标识：Senparc - 20181117

    修改标识：Senparc - 20181226
    修改描述：v0.4.3 修改 DateTime 为 DateTimeOffset

 ----------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Senparc.CO2NET.APM
{
    /// <summary>
    /// 打包统计的每分钟数据
    /// </summary>
    public class MinuteData
    {
        public string KindName { get; set; }
        /// <summary>
        /// 统计时间段，精确到分钟
        /// </summary>
        public DateTimeOffset Time { get; set; }

        /// <summary>
        /// 开始数值
        /// </summary>
        public double StartValue { get; set; }
        /// <summary>
        /// 末尾数值
        /// </summary>
        public double EndValue { get; set; }
        /// <summary>
        /// 最高值
        /// </summary>
        public double HighestValue { get; set; }
        /// <summary>
        /// 最低值
        /// </summary>
        public double LowestValue { get; set; }
        /// <summary>
        /// 数值总和
        /// </summary>
        public double SumValue { get; set; }
        /// <summary>
        /// 统计到的数值样本数量
        /// </summary>
        public int SampleSize { get; set; }
    }
}
