using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Senparc.CO2NET.Cache
{
    /// <summary>
    /// Domain extension (non-CO2NET level cache strategy) interface, for example, as an interface for LocalContainerCacheStrategy
    /// <para>Note: Classes implementing this interface must use the singleton pattern!</para>
    /// </summary>
    public interface IDomainExtensionCacheStrategy
    {
        /// <summary>
        /// Domain cache definition
        /// </summary>
        ICacheStrategyDomain CacheStrategyDomain { get; }

        /// <summary>
        /// Base cache strategy used
        /// </summary>
        Func<IBaseObjectCacheStrategy> BaseCacheStrategy { get; }

        /// <summary>
        /// Register the current cache strategy to the underlying cache
        /// </summary>
        /// <param name="extensionCacheStrategy">Extension cache strategy instance</param>
        void RegisterCacheStrategyDomain(IDomainExtensionCacheStrategy extensionCacheStrategy);
    }
}
