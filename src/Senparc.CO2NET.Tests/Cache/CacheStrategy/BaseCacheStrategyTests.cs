using Microsoft.VisualStudio.TestTools.UnitTesting;
using Senparc.CO2NET.Cache;
using System;

namespace Senparc.CO2NET.Tests.Cache.CacheStrategy
{
    [TestClass]
    public class BaseCacheStrategyTests : BaseTest
    {
        [TestMethod]
        public void BaseCacheStrategyTest()
        {
            // For caching purposes, do not modify

            CacheStrategyFactory.RegisterObjectCacheStrategy(() => LocalObjectCacheStrategy.Instance);

            var testCache = CacheStrategyFactory.GetObjectCacheStrategyInstance();

            var shortKey = "ShortKey";
            var finalKey = testCache.GetFinalKey(shortKey);
            Console.WriteLine($"FinalKey: {finalKey}");

            Assert.IsTrue(finalKey.EndsWith(":" + shortKey));
            Assert.IsTrue(finalKey.StartsWith("Senparc:"));
            Assert.IsTrue(finalKey.Contains($":{Config.DefaultCacheNamespace}:"));
        }
    }
}
