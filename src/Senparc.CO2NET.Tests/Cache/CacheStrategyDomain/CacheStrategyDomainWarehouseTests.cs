using Microsoft.VisualStudio.TestTools.UnitTesting;
using Senparc.CO2NET.Cache;
using Senparc.CO2NET.Exceptions;
using Senparc.CO2NET.Tests.TestEntities;
using System;
using System.Collections.Generic;

namespace Senparc.CO2NET.Tests.Cache.CacheStrategyDomain
{
    [TestClass]
    public class CacheStrategyDomainWarehouseTests : BaseTest
    {
        [TestMethod]
        public void RegisterAndGetTest()
        {
            // Restore default cache state  
            CacheStrategyFactory.RegisterObjectCacheStrategy(() => LocalObjectCacheStrategy.Instance);

            // Register  
            CacheStrategyDomainWarehouse.RegisterCacheStrategyDomain(TestExtensionCacheStrategy.Instance);

            // Get current cache strategy (default is memory cache)  
            var objectCache = CacheStrategyFactory.GetObjectCacheStrategyInstance();
            var testCacheStrategy = CacheStrategyDomainWarehouse
                .GetDomainExtensionCacheStrategy(objectCache, new TestCacheDomain());
            Assert.IsInstanceOfType(testCacheStrategy, typeof(TestExtensionCacheStrategy));

            var baseCache = testCacheStrategy.BaseCacheStrategy();
            Assert.IsInstanceOfType(baseCache, objectCache.GetType());

            // Write  
            var testStr = Guid.NewGuid().ToString();
            baseCache.Set("TestCache", testStr);

            // Read  
            var result = (testCacheStrategy as TestExtensionCacheStrategy).GetTestCache("TestCache");
            Assert.AreEqual(testStr + "|ABC", result);
            Console.WriteLine(result);
        }

        [TestMethod]
        public void ClearRegisteredDomainExtensionCacheStrategiesTest()
        {
            // Add domain cache  
            CacheStrategyDomainWarehouse.RegisterCacheStrategyDomain(TestExtensionCacheStrategy.Instance);
            var objectCache = CacheStrategyFactory.GetObjectCacheStrategyInstance();
            var testCacheStrategy = CacheStrategyDomainWarehouse
                .GetDomainExtensionCacheStrategy(objectCache, new TestCacheDomain());
            Assert.IsInstanceOfType(testCacheStrategy, typeof(TestExtensionCacheStrategy));

            // Clear domain cache  
            CacheStrategyDomainWarehouse.ClearRegisteredDomainExtensionCacheStrategies();
            try
            {
                testCacheStrategy = CacheStrategyDomainWarehouse
                    .GetDomainExtensionCacheStrategy(objectCache, new TestCacheDomain());
            }
            catch (UnregisteredDomainCacheStrategyException ex)
            {
                Console.WriteLine("The following exception is expected\r\n========\r\n");
                Console.WriteLine(ex); // Not registered  
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
        }

        [TestMethod]
        public void AutoScanDomainCacheStrategyTest()
        {
            Config.IsDebug = true;
            {
                Console.WriteLine("Global auto scan");
                var addedTypes = CacheStrategyDomainWarehouse.AutoScanDomainCacheStrategy(true, null);
                addedTypes.ForEach(z => Console.WriteLine(z));
                Assert.IsTrue(addedTypes.Count > 0);
                Assert.IsTrue(addedTypes.Contains(typeof(TestExtensionCacheStrategy)));
                // Auto scan assemblies: 81, total registration time: 205.7718ms - 598.7549ms  
            }
            {
                Console.WriteLine("No auto scan");
                var addedTypes = CacheStrategyDomainWarehouse.AutoScanDomainCacheStrategy(false, null);
                addedTypes.ForEach(z => Console.WriteLine(z));
                Assert.IsTrue(addedTypes.Count == 0);
                // Total registration time: 0.0021ms  
            }
            {
                Console.WriteLine("Manual specification");
                Func<IList<IDomainExtensionCacheStrategy>> func = () =>
                {
                    var list = new List<IDomainExtensionCacheStrategy>
                    {
                        TestExtensionCacheStrategy.Instance
                    };
                    return list;
                };
                var addedTypes = CacheStrategyDomainWarehouse.AutoScanDomainCacheStrategy(false, func);
                addedTypes.ForEach(z => Console.WriteLine(z));
                Assert.IsTrue(addedTypes.Count > 0);
                Assert.IsTrue(addedTypes.Contains(typeof(TestExtensionCacheStrategy)));
                // Total registration time: 0.574ms  
            }
        }
    }
}