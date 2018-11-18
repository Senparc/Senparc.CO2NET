using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Senparc.CO2NET.APM
{
    /// <summary>
    /// 包含了不同KindName的每分钟数据的集合包
    /// </summary>
    public class MinuteDataPack
    {
        public string KindName { get; set; }
        public List<MinuteData> MinuteDataList { get; set; } = new List<MinuteData>();
    }
}
