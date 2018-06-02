using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Senparc.CO2NET
{

    /// <summary>
    /// 缓存类型
    /// </summary>
    public enum CacheType
    {
        /// <summary>
        /// 本地运行时缓存（单机）
        /// </summary>
        Local,
        /// <summary>
        /// Redis缓存（支持分布式）
        /// </summary>
        Redis,
        /// <summary>
        /// Memcached（支持分布式）
        /// </summary>
        Memcached
    }

}
