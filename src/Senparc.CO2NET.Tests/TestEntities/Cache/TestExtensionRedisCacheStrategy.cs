using Senparc.CO2NET.Cache;
using Senparc.CO2NET.Cache.Redis;
using System;
using System.Collections.Generic;
using System.Text;

namespace Senparc.CO2NET.Tests.TestEntities
{
    /// <summary>
    /// Extended cache strategy for testing (default uses local cache, can be modified)
    /// </summary>
    public class TestExtensionRedisCacheStrategy : IDomainExtensionCacheStrategy
    {
        #region 单例

        ///<summary>
        /// Constructor of LocalCacheStrategy
        ///</summary>
        TestExtensionRedisCacheStrategy()
        {
            // Register the current cache strategy to the underlying cache
            CacheStrategyDomainWarehouse.RegisterCacheStrategyDomain(this);
        }

        // Static LocalCacheStrategy
        public static TestExtensionRedisCacheStrategy Instance
        {
            get
            {
                return Nested.instance;// Return the static member instance in the Nested class
            }
        }


        class Nested
        {
            static Nested()
            {
            }
            // Set instance to a new initialized instance of LocalCacheStrategy
            internal static readonly TestExtensionRedisCacheStrategy instance = new TestExtensionRedisCacheStrategy();
        }


        #endregion


        // Set this property to specify the domain of the current extended cache
        public ICacheStrategyDomain CacheStrategyDomain => new TestCacheDomain();

        // The underlying cache strategy used (can also be dynamically adjusted within the delegate, but not recommended.)
        public Func<IBaseObjectCacheStrategy> BaseCacheStrategy =>
            () => RedisObjectCacheStrategy.Instance;// Use Redis cache

        /// <summary>
        /// Extend a method
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string GetTestCache(string key)
        {
            return BaseCacheStrategy().Get(key).ToString();
        }

        public void RegisterCacheStrategyDomain(IDomainExtensionCacheStrategy extensionCacheStrategy)
        {
            CacheStrategyDomainWarehouse.RegisterCacheStrategyDomain(extensionCacheStrategy);// For reusable methods, consider creating a base class
        }
    }
}
