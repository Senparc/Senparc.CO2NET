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

    /// <summary>
    /// 用户信息中的性别（sex）
    /// </summary>
    public enum Sex
    {
#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释   
        未知 = 0,
        男 = 1,
        女 = 2,
        其他 = 3
#pragma warning restore CS1591 // 缺少对公共可见类型或成员的 XML 注释
    }

}
