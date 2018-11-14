using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Senparc.CO2NET.APM
{
    /// <summary>
    /// 打包统计的每分钟数据
    /// </summary>
    public class MinuteDataPack
    {
        public string KindName { get; set; }
        /// <summary>
        /// 统计时间段，精确到分钟
        /// </summary>
        public DateTime Time { get; set; }

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
