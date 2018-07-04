using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Senparc.CO2NET
{
    /// <summary>
    /// 全局设置
    /// </summary>
    public class SenparcSetting
    {
        /// <summary>
        /// 是否出于Debug状态
        /// </summary>
        public bool IsDebug { get; set; }

        /// <summary>
        /// 默认缓存键的第一级命名空间，默认值：DefaultCache
        /// </summary>
        public string DefaultCacheNamespace { get; set; }
    }
}
