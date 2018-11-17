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
        public DateTime DateTime { get; set; }
        /// <summary>
        /// 统计值
        /// </summary>
        public double Value { get; set; }
        /// <summary>
        /// 复杂类型数据
        /// </summary>
        public object Data { get; set; }
    }
}
