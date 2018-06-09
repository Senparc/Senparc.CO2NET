using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Senparc.CO2NET.Cache
{
    /// <summary>
    /// <para>扩展领域缓存策略</para>
    /// <para>注意：实现此接口的类必须使用单例模式！</para>
    /// </summary>
    public interface ICacheStrategyDomain
    {
        /// <summary>
        /// 唯一名称（建议使用GUID）
        /// </summary>
        string IdentityName { get; }

        /// <summary>
        /// 预的名称
        /// </summary>
        string DomainName { get; }
    }
}
