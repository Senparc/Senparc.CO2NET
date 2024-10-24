using Microsoft.VisualStudio.TestTools.UnitTesting;
using Senparc.CO2NET.Cache;
using Senparc.CO2NET.Cache.Memcached;
using Senparc.CO2NET.Cache.Redis;
using Senparc.CO2NET.Tests.TestEntities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Senparc.CO2NET.Tests.Cache
{


    [TestClass]
    public class CacheExpireTest : BaseTest
    {
        [TestMethod]
        public void ExpireTest()
        {
            BaseTest.RegisterServiceCollection();

            var caches = new IBaseObjectCacheStrategy[] {
                LocalObjectCacheStrategy.Instance,
                RedisObjectCacheStrategy.Instance,
                RedisHashSetObjectCacheStrategy.Instance,
                CO2NET.Cache.CsRedis.RedisObjectCacheStrategy.Instance,
                CO2NET.Cache.CsRedis.RedisHashSetObjectCacheStrategy.Instance,
              //  MemcachedObjectCacheStrategy.Instance
            };

            RedisManager.ConfigurationOption = "localhost:6379";
            var index = 0;
            foreach (var cache in caches)
            {
                Console.WriteLine($"开始缓存测试;{cache}");
                CacheStrategyFactory.RegisterObjectCacheStrategy(() => cache);
                var cacheStrategy = CacheStrategyFactory.GetObjectCacheStrategyInstance();
                var dt = SystemTime.Now;
                var key = $"RedisTest-{SystemTime.Now.Ticks}";
                cacheStrategy.Set(key, new TestCustomObject()
                {
                    Id = ++index,
                    Name = "",// Newtonsoft.Json.JsonConvert.SerializeObject(this),
                    AddTime = dt
                }, TimeSpan.FromMilliseconds(500));

                var entity = cacheStrategy.Get(key);
                Assert.IsNotNull(entity);

                var strongEntity = cacheStrategy.Get<TestCustomObject>(key);
                Assert.IsNotNull(strongEntity);
                Assert.AreEqual(index, strongEntity.Id);
                Assert.AreEqual(dt, strongEntity.AddTime);

                if (cache is CO2NET.Cache.CsRedis.RedisObjectCacheStrategy || cache is CO2NET.Cache.CsRedis.RedisHashSetObjectCacheStrategy)
                {
                    //CsRedis only supports expiration in whole seconds
                    Thread.Sleep(1000);//Make the cache expire
                }
                else
                {
                    Thread.Sleep(500);//Make the cache expire
                }

                entity = cacheStrategy.Get(key);

                if (cache.GetType() == typeof(RedisHashSetObjectCacheStrategy) ||
                    cache.GetType() == typeof(Senparc.CO2NET.Cache.CsRedis.RedisHashSetObjectCacheStrategy))
                {
                    Assert.IsNotNull(entity);//RedisHashSet does not support expiration
                }
                else
                {
                    Assert.IsNull(entity);
                }
            }
        }
    }
}
