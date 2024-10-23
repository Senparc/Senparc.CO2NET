using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Senparc.CO2NET.Cache
{
    /// <summary>
    /// <para>Extended domain cache strategy</para>
    /// <para>Note: Classes implementing this interface must use the singleton pattern!</para>
    /// </summary>
    public interface ICacheStrategyDomain
    {
        /// <summary>
        /// Unique name (recommended to use GUID)
        /// </summary>
        string IdentityName { get; }

        /// <summary>
        /// Name of the domain
        /// </summary>
        string DomainName { get; }
    }
}
