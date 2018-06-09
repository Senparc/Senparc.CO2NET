using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Senparc.CO2NET.Cache
{
    /// <summary>
    /// 扩展领域缓存策略
    /// </summary>
    public interface IExtensionDomainCacheStrategy
    {
        /// <summary>
        /// 唯一名称（建议使用GUID）
        /// </summary>
        string IdentityName { get; set; }
        string DomainName { get; set; }
    }
}
