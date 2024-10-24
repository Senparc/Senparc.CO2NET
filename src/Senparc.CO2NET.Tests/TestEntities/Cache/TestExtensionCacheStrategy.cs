using Senparc.CO2NET.Cache;
using System;
using System.Collections.Generic;
using System.Text;

namespace Senparc.CO2NET.Tests.TestEntities
{
    /// <summary>
    /// Extended cache strategy for testing (default is local cache, can be modified)
    /// </summary>
    public class TestExtensionCacheStrategy : IDomainExtensionCacheStrategy
    {
        #region Singleton

        ///<summary>
        /// Constructor of LocalCacheStrategy
        ///</summary>
        TestExtensionCacheStrategy()
        {
            // Register the current cache strategy to the underlying cache
            CacheStrategyDomainWarehouse.RegisterCacheStrategyDomain(this);
        }

        // Static LocalCacheStrategy
        public static TestExtensionCacheStrategy Instance
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
            internal static readonly TestExtensionCacheStrategy instance = new TestExtensionCacheStrategy();
        }


        #endregion


        // Set this property to specify the domain of the current extended cache
        public ICacheStrategyDomain CacheStrategyDomain => new TestCacheDomain();

        // The base cache strategy used (can also be dynamically adjusted within the delegate, but not recommended.)
        public Func<IBaseObjectCacheStrategy> BaseCacheStrategy =>
            () => LocalObjectCacheStrategy.Instance;// Default to local cache

        /// <summary>
        /// Extend a method
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string GetTestCache(string key)
        {
            return BaseCacheStrategy().Get(key).ToString()+"|ABC";
        }

        public void RegisterCacheStrategyDomain(IDomainExtensionCacheStrategy extensionCacheStrategy)
        {
            CacheStrategyDomainWarehouse.RegisterCacheStrategyDomain(extensionCacheStrategy);// For reusable methods, consider creating a base class
        }
    }
}
