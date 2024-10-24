using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Senparc.CO2NET.Cache;
using Senparc.CO2NET.Helpers;
using Senparc.CO2NET.Tests.TestEntities;

namespace Senparc.CO2NET.Tests.Helpers
{
    [TestClass]
    public class MutipleCacheTestHelperTests : BaseTest
    {
        [TestMethod]
        public void MutipleCacheTestHelperTest()
        {
            //BaseTest.RegisterServiceStart();//Automatically register Redis, can also be manually registered
            //BaseTest.RegisterServiceCollection();

            var exCache = TestExtensionCacheStrategy.Instance;//Register local cache
            var exRedisCache = TestExtensionRedisCacheStrategy.Instance;//Register Redis cache

            MutipleCacheTestHelper.RunMutipleCache(() =>
            {
                try
                {
                    var currentCache = CacheStrategyFactory.GetObjectCacheStrategyInstance();
                    Console.WriteLine("Current cache strategy: " + currentCache.GetType());
                    var testExCache = CacheStrategyFactory.GetExtensionCacheStrategyInstance(new TestCacheDomain());
                    var baseCache = testExCache.BaseCacheStrategy();
                    Console.WriteLine("Current extended cache strategy: " + baseCache.GetType());
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);//Local already registered, Redis not registered
                }

            }, CacheType.Local, CacheType.Redis);
        }
    }
}
