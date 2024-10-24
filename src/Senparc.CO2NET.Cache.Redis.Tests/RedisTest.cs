using MessagePack;
using MessagePack.Formatters;
using MessagePack.Resolvers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StackExchange.Redis;
using System;
using System.Threading;

namespace Senparc.CO2NET.Cache.Redis.Tests
{
    [MessagePackObject(keyAsPropertyName: true)]
    public class ContainerBag
    {
        [Key(0)]
        public string Key { get; set; }
        [Key(1)]
        public string Name { get; set; }
        [Key(2)]
        //[MessagePackFormatter(typeof(DateTimeFormatter))]  
        public DateTimeOffset AddTime { get; set; }
    }

    [TestClass]
    public class RedisTest
    {
        [TestMethod]
        public void SetTest()
        {
            RedisManager.ConfigurationOption = "localhost:6379";
            CacheStrategyFactory.RegisterObjectCacheStrategy(() => RedisObjectCacheStrategy.Instance);
            var cacheStrategy = CacheStrategyFactory.GetObjectCacheStrategyInstance();
            var dt = SystemTime.Now;

            cacheStrategy.Set("RedisTest", new ContainerBag()
            {
                Key = "123",
                Name = "", // Newtonsoft.Json.JsonConvert.SerializeObject(this),  
                AddTime = dt
            });

            var obj = cacheStrategy.Get<ContainerBag>("RedisTest");
            Assert.IsNotNull(obj);
            Assert.IsInstanceOfType(obj, typeof(ContainerBag));

            var containerBag = obj as ContainerBag;
            Assert.IsNotNull(containerBag);
            Assert.AreEqual(dt, containerBag.AddTime);

            Console.WriteLine($"SetTest single test elapsed time: {SystemTime.DiffTotalMS(dt)}ms");
        }

        [TestMethod]
        public void SetAsyncTest()
        {
            RedisManager.ConfigurationOption = "localhost:6379";
            CacheStrategyFactory.RegisterObjectCacheStrategy(() => RedisObjectCacheStrategy.Instance);
            var cacheStrategy = CacheStrategyFactory.GetObjectCacheStrategyInstance();
            var dt = SystemTime.Now;

            cacheStrategy.Set("RedisTest", new ContainerBag()
            {
                Key = "123",
                Name = "", // Newtonsoft.Json.JsonConvert.SerializeObject(this),  
                AddTime = dt
            });

            var obj = cacheStrategy.GetAsync<ContainerBag>("RedisTest").Result;
            Assert.IsNotNull(obj);
            Assert.IsInstanceOfType(obj, typeof(ContainerBag));

            var containerBag = obj as ContainerBag;
            Assert.IsNotNull(containerBag);
            Assert.AreEqual(dt, containerBag.AddTime);

            Console.WriteLine($"SetAsyncTest single test elapsed time: {SystemTime.DiffTotalMS(dt)}ms");
        }

        [TestMethod]
        public void ExpiryTest()
        {
            RedisManager.ConfigurationOption = "localhost:6379";
            CacheStrategyFactory.RegisterObjectCacheStrategy(() => RedisObjectCacheStrategy.Instance);
            var cacheStrategy = CacheStrategyFactory.GetObjectCacheStrategyInstance();
            var dt = SystemTime.Now;
            var key = $"RedisTest-{SystemTime.Now.Ticks}";
            var value = new ContainerBag()
            {
                Key = "123",
                Name = "", // Newtonsoft.Json.JsonConvert.SerializeObject(this),  
                AddTime = dt
            };

            cacheStrategy.Set(key, value, TimeSpan.FromSeconds(100));
            Thread.Sleep(1000); // Wait  
            var entity = cacheStrategy.Get(key);
            Assert.IsNotNull(entity); // Not expired  

            cacheStrategy.Update(key, value, TimeSpan.FromSeconds(1)); // Reset expiry time  
            entity = cacheStrategy.Get(key);
            Assert.IsNotNull(entity);

            var strongEntity = cacheStrategy.Get<ContainerBag>(key);
            Assert.IsNotNull(strongEntity);
            Assert.AreEqual(dt, strongEntity.AddTime);

            Thread.Sleep(1000); // Let the cache expire  
            entity = cacheStrategy.Get(key);
            Assert.IsNull(entity);
        }

        [TestMethod]
        public void ExpiryAsyncTest()
        {
            RedisManager.ConfigurationOption = "localhost:6379";
            CacheStrategyFactory.RegisterObjectCacheStrategy(() => RedisObjectCacheStrategy.Instance);
            var cacheStrategy = CacheStrategyFactory.GetObjectCacheStrategyInstance();
            var dt = SystemTime.Now;
            var key = $"RedisTest-{SystemTime.Now.Ticks}";

            cacheStrategy.Set(key, new ContainerBag()
            {
                Key = "123",
                Name = "", // Newtonsoft.Json.JsonConvert.SerializeObject(this),  
                AddTime = dt
            }, TimeSpan.FromSeconds(1));

            var entity = cacheStrategy.GetAsync(key).Result;
            Assert.IsNotNull(entity);

            var strongEntity = cacheStrategy.Get<ContainerBag>(key);
            Assert.IsNotNull(strongEntity);
            Assert.AreEqual(dt, strongEntity.AddTime);

            Thread.Sleep(1000); // Let the cache expire  
            entity = cacheStrategy.GetAsync(key).Result;
            Assert.IsNull(entity);
        }

        #region Performance Related Tests  

        [TestMethod]
        public void EfficiencyTest()
        {
            var dt1 = SystemTime.Now;
            for (int i = 0; i < 100; i++)
            {
                SetTest();
            }
            Console.WriteLine($"EfficiencyTest total test time (using CacheWrapper): {SystemTime.DiffTotalMS(dt1)}ms");
        }

        [TestMethod]
        public void StackExchangeRedisExtensionsTest()
        {
            Console.WriteLine("Starting asynchronous test");
            var threadCount = 100;
            var finishCount = 0;
            for (int i = 0; i < threadCount; i++)
            {
                var thread = new Thread(() =>
                {
                    var newObj = new ContainerBag()
                    {
                        Key = Guid.NewGuid().ToString(),
                        Name = Newtonsoft.Json.JsonConvert.SerializeObject(this),
                        AddTime = SystemTime.Now
                    };

                    var dtx = SystemTime.Now;
                    var serializedObj = StackExchangeRedisExtensions.Serialize(newObj);
                    Console.WriteLine($"StackExchangeRedisExtensions.Serialize elapsed time: {SystemTime.DiffTotalMS(dtx)}ms");

                    dtx = SystemTime.Now;
                    var containerBag = StackExchangeRedisExtensions.Deserialize<ContainerBag>((RedisValue)serializedObj); // 11ms  
                    Console.WriteLine($"StackExchangeRedisExtensions.Deserialize elapsed time: {SystemTime.DiffTotalMS(dtx)}ms");

                    Assert.AreEqual(containerBag.AddTime.Ticks, newObj.AddTime.Ticks);
                    Assert.AreNotEqual(containerBag.GetHashCode(), newObj.GetHashCode());
                    finishCount++;
                });
                thread.Start();
            }

            while (finishCount < threadCount)
            {
                // Wait  
            }

            Action action = () =>
            {
                var newObj = new ContainerBag()
                {
                    Key = Guid.NewGuid().ToString(),
                    Name = Newtonsoft.Json.JsonConvert.SerializeObject(this),
                    AddTime = SystemTime.Now
                };

                var dtx = SystemTime.Now;
                var serializedObj = StackExchangeRedisExtensions.Serialize(newObj);
                Console.WriteLine($"StackExchangeRedisExtensions.Serialize elapsed time: {SystemTime.DiffTotalMS(dtx)}ms");

                dtx = SystemTime.Now;
                var containerBag = StackExchangeRedisExtensions.Deserialize<ContainerBag>((RedisValue)serializedObj); // 11ms  
                Console.WriteLine($"StackExchangeRedisExtensions.Deserialize elapsed time: {SystemTime.DiffTotalMS(dtx)}ms");

                Assert.AreEqual(containerBag.AddTime.Ticks, newObj.AddTime.Ticks);
                Assert.AreNotEqual(containerBag.GetHashCode(), newObj.GetHashCode());
            };

            Console.WriteLine("Starting synchronous test");
            for (int i = 0; i < 10; i++)
            {
                action();
            }
        }

        [TestMethod]
        public void MessagePackTest()
        {
            Console.WriteLine("Starting asynchronous test");
            var threadCount = 10;
            var finishCount = 0;
            for (int i = 0; i < threadCount; i++)
            {
                var thread = new Thread(() =>
                {
                    var newObj = new ContainerBag()
                    {
                        Key = Guid.NewGuid().ToString(),
                        Name = Newtonsoft.Json.JsonConvert.SerializeObject(this),
                        AddTime = SystemTime.Now.ToUniversalTime()
                    };

                    var dtx = SystemTime.Now;
                    var serializedObj = MessagePackSerializer.Serialize(newObj);
                    Console.WriteLine($"MessagePackSerializer.Serialize elapsed time: {SystemTime.DiffTotalMS(dtx)}ms");

                    dtx = SystemTime.Now;
                    var containerBag = MessagePackSerializer.Deserialize<ContainerBag>(serializedObj); // 11ms  
                    Console.WriteLine($"MessagePackSerializer.Deserialize elapsed time: {SystemTime.DiffTotalMS(dtx)}ms");

                    Console.WriteLine(containerBag.AddTime.ToUniversalTime());
                    Assert.AreNotEqual(containerBag.GetHashCode(), newObj.GetHashCode());
                    finishCount++;
                });
                thread.Start();
            }

            while (finishCount < threadCount)
            {
                // Wait  
            }
        }

        [TestMethod]
        public void NewtonsoftTest()
        {
            Console.WriteLine("Starting asynchronous test");
            var threadCount = 50;
            var finishCount = 0;
            for (int i = 0; i < threadCount; i++)
            {
                var thread = new Thread(() =>
                {
                    var newObj = new ContainerBag()
                    {
                        Key = Guid.NewGuid().ToString(),
                        Name = Newtonsoft.Json.JsonConvert.SerializeObject(this),
                        AddTime = SystemTime.Now.ToUniversalTime()
                    };

                    var dtx = SystemTime.Now;
                    var serializedObj = Newtonsoft.Json.JsonConvert.SerializeObject(newObj);
                    Console.WriteLine($"Newtonsoft.Json.JsonConvert.SerializeObject elapsed time: {SystemTime.DiffTotalMS(dtx)}ms");

                    dtx = SystemTime.Now;
                    var containerBag = Newtonsoft.Json.JsonConvert.DeserializeObject<ContainerBag>(serializedObj); // 11ms  
                    Console.WriteLine($"Newtonsoft.Json.JsonConvert.DeserializeObject elapsed time: {SystemTime.DiffTotalMS(dtx)}ms");

                    Console.WriteLine(containerBag.AddTime.ToUniversalTime());
                    Assert.AreNotEqual(containerBag.GetHashCode(), newObj.GetHashCode());
                    finishCount++;
                });
                thread.Start();
            }

            while (finishCount < threadCount)
            {
                // Wait  
            }
        }

        #endregion
    }
}