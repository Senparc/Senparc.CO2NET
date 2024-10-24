using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Senparc.CO2NET.APM
{
    /// <summary>
    /// Contains a collection of per-minute data for different KindName
    /// </summary>
    public class MinuteDataPack
    {
        public string KindName { get; set; }
        public List<MinuteData> MinuteDataList { get; set; } = new List<MinuteData>();
    }
}
