using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Senparc.CO2NET.Cache
{
    /// <summary>
    /// 领域扩展（非 CO2NET 级别的缓存策略）接口，例如作为 LocalContainerCacheStrategy 的接口
    /// <para>注意：实现此接口的类必须使用单例模式！</para>
    /// </summary>
    public interface IDomainExtensionCacheStrategy
    {
        /// <summary>
        /// 领域缓存定义
        /// </summary>
        ICacheStrategyDomain CacheStrategyDomain { get; }

        /// <summary>
        /// 使用的基础缓存策略
        /// </summary>
        Func<IBaseObjectCacheStrategy> BaseCacheStrategy { get; }

        /// <summary>
        /// 向底层缓存注册当前缓存策略
        /// </summary>
        /// <param name="extensionCacheStrategy">扩展缓存策略实例</param>
        void RegisterCacheStrategyDomain(IDomainExtensionCacheStrategy extensionCacheStrategy);
    }
}
