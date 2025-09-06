using MessagePack;
using MessagePack.Formatters;
using MessagePack.Resolvers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StackExchange.Redis;
using System;
using System.Threading;

namespace Senparc.CO2NET.Cache.CsRedis.Tests
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
            Config.SenparcSetting.Cache_Redis_Configuration = "10.37.129.2:6379";
            CsRedis.Register.SetConfigurationOption(Config.SenparcSetting.Cache_Redis_Configuration);
            CsRedis.Register.UseKeyValueRedisNow();

            CacheStrategyFactory.RegisterObjectCacheStrategy(() => RedisObjectCacheStrategy.Instance);
            var cacheStrategy = CacheStrategyFactory.GetObjectCacheStrategyInstance();

            var dt = SystemTime.Now;
            cacheStrategy.Set("RedisTest", new ContainerBag()
            {
                Key = "123",
                Name = "",// Newtonsoft.Json.JsonConvert.SerializeObject(this),
                AddTime = dt
            });

            var obj = cacheStrategy.Get<ContainerBag>("RedisTest");
            Assert.IsNotNull(obj);
            Assert.IsInstanceOfType(obj, typeof(ContainerBag));
            //Console.WriteLine(obj);

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
                Name = "",// Newtonsoft.Json.JsonConvert.SerializeObject(this),
                AddTime = dt
            });

            var obj = cacheStrategy.GetAsync<ContainerBag>("RedisTest").Result;
            Assert.IsNotNull(obj);
            Assert.IsInstanceOfType(obj, typeof(ContainerBag));
            //Console.WriteLine(obj);

            var containerBag = obj as ContainerBag;
            Assert.IsNotNull(containerBag);
            Assert.AreEqual(dt, containerBag.AddTime);

            Console.WriteLine($"SetTest single test elapsed time: {SystemTime.DiffTotalMS(dt)}ms");
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
                Name = "",// Newtonsoft.Json.JsonConvert.SerializeObject(this),
                AddTime = dt
            };
            cacheStrategy.Set(key, value, TimeSpan.FromSeconds(100));
            Thread.Sleep(1000);//Waiting
            var entity = cacheStrategy.Get(key);
            Assert.IsNotNull(entity);//Not implemented

            cacheStrategy.Update(key, value, TimeSpan.FromSeconds(1));//Initialization time
            entity = cacheStrategy.Get(key);
            Assert.IsNotNull(entity);

            var strongEntity = cacheStrategy.Get<ContainerBag>(key);
            Assert.IsNotNull(strongEntity);
            Assert.AreEqual(dt, strongEntity.AddTime);

            Thread.Sleep(1000);//User input
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
                Name = "",// Newtonsoft.Json.JsonConvert.SerializeObject(this),
                AddTime = dt
            }, TimeSpan.FromSeconds(1));

            var entity = cacheStrategy.GetAsync(key).Result;
            Assert.IsNotNull(entity);

            var strongEntity = cacheStrategy.Get<ContainerBag>(key);
            Assert.IsNotNull(strongEntity);
            Assert.AreEqual(dt, strongEntity.AddTime);

            Thread.Sleep(1000);//User input
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

        //[TestMethod]
        //public void ThreadsEfficiencyTest()
        //{
        //    var dt1 = SystemTime.Now;
        //    var threadCount = 10;
        //    var finishCount = 0;
        //    for (int i = 0; i < threadCount; i++)
        //    {
        //        var thread = new Thread(() =>
        //        {
        //            CacheStrategyFactory.RegisterObjectCacheStrategy(() => RedisObjectCacheStrategy.Instance);


        //            var dtx = SystemTime.Now;
        //            var cacheStrategy = CacheStrategyFactory.GetObjectCacheStrategyInstance();


        //            var dt = SystemTime.Now;
        //            cacheStrategy.Set("RedisTest_" + dt.Ticks, new ContainerBag()
        //            {
        //                Key = "123",
        //                Name = "hi",
        //                AddTime = dt
        //            });//37ms

        //            var obj = cacheStrategy.Get("RedisTest_" + dt.Ticks);//14-25ms
        //            Assert.IsNotNull(obj);
        //            Assert.IsInstanceOfType(obj, typeof(RedisValue));
        //            //Console.WriteLine(obj);

        //            var containerBag = StackExchangeRedisExtensions.Deserialize<ContainerBag>((RedisValue)obj);//11ms
        //            Assert.IsNotNull(containerBag);
        //            Assert.AreEqual(dt.Ticks, containerBag.AddTime.Ticks);


        //            Console.WriteLine($"Thread execution time after cache: {SystemTime.DiffTotalMS(dtx)}ms");

        //            finishCount++;
        //        });
        //        thread.Start();
        //    }

        //    while (finishCount < threadCount)
        //    {
        //        //Waiting
        //    }

        //    Console.WriteLine($"EfficiencyTest total time: {SystemTime.DiffTotalMS(dt1)}ms");
        //}

        [TestMethod]
        public void CacheSerializeExtensionTest()
        {
            Console.WriteLine("Starting asynchronous test"); 
            var threadCount = 100;
            var finishCount = 0;
            for (int i = 0; i < threadCount; i++)
            {
                var thread = new Thread(() =>
                {
                    try
                    {
                        var newObj = new ContainerBag()
                        {
                            Key = Guid.NewGuid().ToString(),
                            Name = Newtonsoft.Json.JsonConvert.SerializeObject(this),
                            AddTime = SystemTime.Now
                        };
                        var dtx = SystemTime.Now;
                        var serializedObj = CacheSerializeExtension.SerializeToCache(newObj);
                        Console.WriteLine($"CacheSerializeExtension.SerializeToCache elapsed time: {SystemTime.DiffTotalMS(dtx)}ms");
                        dtx = SystemTime.Now;
                        var containerBag = CacheSerializeExtension.DeserializeFromCache<ContainerBag>((string)serializedObj); // 11ms  
                        Console.WriteLine($"CacheSerializeExtension.DeserializeFromCache elapsed time: {SystemTime.DiffTotalMS(dtx)}ms");
                        Assert.AreEqual(containerBag.AddTime.Ticks, newObj.AddTime.Ticks);
                        Assert.AreNotEqual(containerBag.GetHashCode(), newObj.GetHashCode());
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                    finally
                    {
                        finishCount++;
                    }
                });
                thread.Start();
            }

            while (finishCount < threadCount)
            {
                //Waiting
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
                var serializedObj = CacheSerializeExtension.SerializeToCache(newObj);
                Console.WriteLine($"StackExchangeRedisExtensions.Serialize elapsed time: {SystemTime.DiffTotalMS(dtx)}ms");

                dtx = SystemTime.Now;
                var containerBag = CacheSerializeExtension.DeserializeFromCache<ContainerBag>((string)serializedObj); // 11ms  
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
            //    CompositeResolver.RegisterAndSetAsDefault(
            //new[] { TypelessFormatter.Instance },
            //new[] { NativeDateTimeResolver.Instance, ContractlessStandardResolver.Instance });

            //            CompositeResolver.RegisterAndSetAsDefault(
            //    // Resolve DateTime first
            //    MessagePack.Resolvers.NativeDateTimeResolver.Instance,
            //    MessagePack.Resolvers.StandardResolver.Instance,
            //       MessagePack.Resolvers.BuiltinResolver.Instance,
            //                // use PrimitiveObjectResolver
            //                PrimitiveObjectResolver.Instance
            //);

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
                    var serializedObj = MessagePackSerializer.Serialize(newObj/*, NativeDateTimeResolver.Instance*/);
                    Console.WriteLine($"MessagePackSerializer.Serialize elapsed time: {SystemTime.DiffTotalMS(dtx)}ms");

                    dtx = SystemTime.Now;
                    var containerBag = MessagePackSerializer.Deserialize<ContainerBag>(serializedObj); // 11ms  
                    Console.WriteLine($"MessagePackSerializer.Deserialize elapsed time: {SystemTime.DiffTotalMS(dtx)}ms");

                    Console.WriteLine(containerBag.AddTime.ToUniversalTime());
                    // Assert.AreEqual(containerBag.AddTime.Ticks, newObj.AddTime.Ticks);  
                    Assert.AreNotEqual(containerBag.GetHashCode(), newObj.GetHashCode());

                    finishCount++;
                });

                thread.Start();
            }

            while (finishCount < threadCount)
            {
                //Waiting
            }
        }


        [TestMethod]
        public void NewtonsoftTest()
        {
            //    CompositeResolver.RegisterAndSetAsDefault(
            //new[] { TypelessFormatter.Instance },
            //new[] { NativeDateTimeResolver.Instance, ContractlessStandardResolver.Instance });

            //            CompositeResolver.RegisterAndSetAsDefault(
            //    // Resolve DateTime first
            //    MessagePack.Resolvers.NativeDateTimeResolver.Instance,
            //    MessagePack.Resolvers.StandardResolver.Instance,
            //       MessagePack.Resolvers.BuiltinResolver.Instance,
            //                // use PrimitiveObjectResolver
            //                PrimitiveObjectResolver.Instance
            //);

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

                    //Assert.AreEqual(containerBag.AddTime.Ticks, newObj.AddTime.Ticks);
                    Assert.AreNotEqual(containerBag.GetHashCode(), newObj.GetHashCode());
                    finishCount++;
                });
                thread.Start();
            }

            while (finishCount < threadCount)
            {
                //Waiting
            }
        }

        #endregion

    }
}
