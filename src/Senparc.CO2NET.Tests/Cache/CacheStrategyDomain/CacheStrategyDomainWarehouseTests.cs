using Microsoft.VisualStudio.TestTools.UnitTesting;
using Senparc.CO2NET.Cache;
using Senparc.CO2NET.Tests.TestEntiies;
using System;

namespace Senparc.CO2NET.Tests.Cache.CacheStrategyDomain
{


    [TestClass]
    public class CacheStrategyDomainWarehouseTests
    {
        [TestMethod]
        public void RegisterAndGetTest()
        {
            //注册
            CacheStrategyDomainWarehouse.RegisterCacheStrategyDomain(TestCacheStrategy.Instance);

            //获取

            //获取当前缓存策略（默认为内存缓存）
            var objectCache = CacheStrategyFactory.GetObjectCacheStrategyInstance();
            var testCacheStrategy = CacheStrategyDomainWarehouse
                .GetDomainExtensionCacheStrategy(objectCache, new TestCacheDomain());

            Assert.IsInstanceOfType(testCacheStrategy, typeof(TestCacheStrategy));

            var baseCache = testCacheStrategy.BaseCacheStrategy();
            Assert.IsInstanceOfType(baseCache, objectCache.GetType());


            //写入
            var testStr = Guid.NewGuid().ToString();
            baseCache.Set("TestCache", testStr);

            //读取
            var result = (testCacheStrategy as TestCacheStrategy).GetTestCache("TestCache");
            Assert.AreEqual(testStr, result);
        }
    }
}
