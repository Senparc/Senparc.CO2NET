using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Senparc.CO2NET.Cache;
using Senparc.CO2NET.Helpers;
using Senparc.CO2NET.Tests.TestEntities;

namespace Senparc.CO2NET.Tests.Helpers
{
    [TestClass]
    public class MutipleCacheTestHelperTests
    {
        [TestMethod]
        public void MutipleCacheTestHelperTest()
        {
           var exCache = TestExtensionCacheStrategy.Instance;//完成领域缓存注册

            MutipleCacheTestHelper.RunMutipleCache(() =>
            {
                try
                {
                    var currentCache = CacheStrategyFactory.GetObjectCacheStrategyInstance();
                    Console.WriteLine("当前缓存策略：" + currentCache.GetType());

                    var testExCache = CacheStrategyFactory.GetExtensionCacheStrategyInstance(new TestCacheDomain());
                    var baseCache = testExCache.BaseCacheStrategy();

                    Console.WriteLine("当前扩展缓存策略："+ baseCache.GetType());
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);//Local已经注册，Redis未注册
                }

            }, CacheType.Local, CacheType.Redis);
        }
    }
}
