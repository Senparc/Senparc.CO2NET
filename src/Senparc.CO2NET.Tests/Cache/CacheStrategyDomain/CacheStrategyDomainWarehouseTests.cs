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
            //还原默认缓存状态
            CacheStrategyFactory.RegisterObjectCacheStrategy(() => LocalObjectCacheStrategy.Instance);

            //注册
            CacheStrategyDomainWarehouse.RegisterCacheStrategyDomain(TestExtensionCacheStrategy.Instance);

            //获取

            //获取当前缓存策略（默认为内存缓存）
            var objectCache = CacheStrategyFactory.GetObjectCacheStrategyInstance();
            var testCacheStrategy = CacheStrategyDomainWarehouse
                .GetDomainExtensionCacheStrategy(objectCache, new TestCacheDomain());

            Assert.IsInstanceOfType(testCacheStrategy, typeof(TestExtensionCacheStrategy));

            var baseCache = testCacheStrategy.BaseCacheStrategy();
            Assert.IsInstanceOfType(baseCache, objectCache.GetType());


            //写入
            var testStr = Guid.NewGuid().ToString();
            baseCache.Set("TestCache", testStr);

            //读取
            var result = (testCacheStrategy as TestExtensionCacheStrategy).GetTestCache("TestCache");
            Assert.AreEqual(testStr + "|ABC", result);
            Console.WriteLine(result);
        }

        [TestMethod]
        public void ClearRegisteredDomainExtensionCacheStrategiesTest()
        {
            //添加领域缓存
            CacheStrategyDomainWarehouse.RegisterCacheStrategyDomain(TestExtensionCacheStrategy.Instance);
            var objectCache = CacheStrategyFactory.GetObjectCacheStrategyInstance();

            var testCacheStrategy = CacheStrategyDomainWarehouse
             .GetDomainExtensionCacheStrategy(objectCache, new TestCacheDomain());

            Assert.IsInstanceOfType(testCacheStrategy, typeof(TestExtensionCacheStrategy));

            //清除领域缓存
            CacheStrategyDomainWarehouse.ClearRegisteredDomainExtensionCacheStrategies();
            try
            {
                testCacheStrategy = CacheStrategyDomainWarehouse
                                .GetDomainExtensionCacheStrategy(objectCache, new TestCacheDomain());
            }
            catch (UnregisteredDomainCacheStrategyException ex)
            {
                Console.WriteLine("以下异常抛出才是正确的\r\n========\r\n");
                Console.WriteLine(ex);//未注册
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
                Console.WriteLine("全局自动扫描");
                var addedTypes = CacheStrategyDomainWarehouse.AutoScanDomainCacheStrategy(true, null);
                addedTypes.ForEach(z => Console.WriteLine(z));
                Assert.IsTrue(addedTypes.Count > 0);
                Assert.IsTrue(addedTypes.Contains(typeof(TestExtensionCacheStrategy)));
                //自动扫描程序集：81个，注册总用时：205.7718ms - 598.7549ms
            }
            {
                Console.WriteLine("不自动扫描");//
                var addedTypes = CacheStrategyDomainWarehouse.AutoScanDomainCacheStrategy(false, null);
                addedTypes.ForEach(z => Console.WriteLine(z));
                Assert.IsTrue(addedTypes.Count == 0);
                //注册总用时：0.0021ms
            }

            {
                Console.WriteLine("手动指定");
                Func<IList<IDomainExtensionCacheStrategy>> func = () =>
                {
                    var list = new List<IDomainExtensionCacheStrategy>();
                    list.Add(TestExtensionCacheStrategy.Instance);
                    return list;
                };

                var addedTypes = CacheStrategyDomainWarehouse.AutoScanDomainCacheStrategy(false, func);
                addedTypes.ForEach(z => Console.WriteLine(z));
                Assert.IsTrue(addedTypes.Count > 0);
                Assert.IsTrue(addedTypes.Contains(typeof(TestExtensionCacheStrategy)));
                //注册总用时：0.574ms
            }
        }
    }
}
