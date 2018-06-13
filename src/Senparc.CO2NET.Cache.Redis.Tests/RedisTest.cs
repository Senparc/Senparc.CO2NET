using MessagePack;
using MessagePack.Formatters;
using MessagePack.Resolvers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StackExchange.Redis;
using System;
using System.Threading;

namespace Senparc.CO2NET.Cache.Redis.Tests
{
    [MessagePackObject(keyAsPropertyName:true)]
    public class ContainerBag
    {
        [Key(0)]
        public string Key { get; set; }
        [Key(1)]
        public string Name { get; set; }
        [Key(2)]
        //[MessagePackFormatter(typeof(DateTimeFormatter))]
        public DateTime AddTime { get; set; }
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

            var dt = DateTime.Now;
            cacheStrategy.InsertToCache("RedisTest", new ContainerBag()
            {
                Key = "123",
                Name = Newtonsoft.Json.JsonConvert.SerializeObject(this),
                AddTime = dt
            });

            var obj = cacheStrategy.Get("RedisTest");
            Assert.IsNotNull(obj);
            Assert.IsInstanceOfType(obj, typeof(RedisValue));
            //Console.WriteLine(obj);

            var containerBag = StackExchangeRedisExtensions.Deserialize<ContainerBag>((RedisValue)obj);
            Assert.IsNotNull(containerBag);
            Assert.AreEqual(dt, containerBag.AddTime);

            Console.WriteLine($"SetTest单条测试耗时：{(DateTime.Now - dt).TotalMilliseconds}ms");
        }

        [TestMethod]
        public void EfficiencyTest()
        {
            var dt1 = DateTime.Now;
            for (int i = 0; i < 100; i++)
            {
                SetTest();
            }

            Console.WriteLine($"EfficiencyTest总测试时间：{(DateTime.Now - dt1).TotalMilliseconds}ms");
        }

        [TestMethod]
        public void ThreadsEfficiencyTest()
        {
            var dt1 = DateTime.Now;
            var threadCount = 10;
            var finishCount = 0;
            for (int i = 0; i < threadCount; i++)
            {
                var thread = new Thread(() =>
                {
                    CacheStrategyFactory.RegisterObjectCacheStrategy(() => RedisObjectCacheStrategy.Instance);


                    var dtx = DateTime.Now;
                    var cacheStrategy = CacheStrategyFactory.GetObjectCacheStrategyInstance();


                    var dt = DateTime.Now;
                    cacheStrategy.InsertToCache("RedisTest_" + dt.Ticks, new ContainerBag()
                    {
                        Key = "123",
                        Name = "hi",
                        AddTime = dt
                    });//37ms

                    var obj = cacheStrategy.Get("RedisTest_" + dt.Ticks);//14-25ms
                    Assert.IsNotNull(obj);
                    Assert.IsInstanceOfType(obj, typeof(RedisValue));
                    //Console.WriteLine(obj);

                    var containerBag = StackExchangeRedisExtensions.Deserialize<ContainerBag>((RedisValue)obj);//11ms
                    Assert.IsNotNull(containerBag);
                    Assert.AreEqual(dt.Ticks, containerBag.AddTime.Ticks);


                    Console.WriteLine($"Thread内单条测试耗时：{(DateTime.Now - dtx).TotalMilliseconds}ms");

                    finishCount++;
                });
                thread.Start();
            }

            while (finishCount < threadCount)
            {
                //等待
            }

            Console.WriteLine($"EfficiencyTest总测试时间：{(DateTime.Now - dt1).TotalMilliseconds}ms");
        }

        [TestMethod]
        public void StackExchangeRedisExtensionsTest()
        {
            Console.WriteLine("开始异步测试");
            var threadCount = 10;
            var finishCount = 0;
            for (int i = 0; i < threadCount; i++)
            {
                var thread = new Thread(() => {
                    var newObj = new ContainerBag()
                    {
                        Key = Guid.NewGuid().ToString(),
                        Name = Newtonsoft.Json.JsonConvert.SerializeObject(this),
                        AddTime = DateTime.Now
                    };
                    var dtx = DateTime.Now;
                    var serializedObj = StackExchangeRedisExtensions.Serialize(newObj);
                    Console.WriteLine($"StackExchangeRedisExtensions.Serialize耗时：{(DateTime.Now - dtx).TotalMilliseconds}ms");

                    dtx = DateTime.Now;
                    var containerBag = StackExchangeRedisExtensions.Deserialize<ContainerBag>((RedisValue)serializedObj);//11ms
                    Console.WriteLine($"StackExchangeRedisExtensions.Deserialize耗时：{(DateTime.Now - dtx).TotalMilliseconds}ms");

                    Assert.AreEqual(containerBag.AddTime.Ticks, newObj.AddTime.Ticks);
                    Assert.AreNotEqual(containerBag.GetHashCode(), newObj.GetHashCode());
                    finishCount++;
                });
                thread.Start();
            }

            while (finishCount < threadCount)
            {
                //等待
            }


            Action action = () =>
            {
                var newObj = new ContainerBag()
                {
                    Key = Guid.NewGuid().ToString(),
                    Name = Newtonsoft.Json.JsonConvert.SerializeObject(this),
                    AddTime = DateTime.Now
                };
                var dtx = DateTime.Now;
                var serializedObj = StackExchangeRedisExtensions.Serialize(newObj);
                Console.WriteLine($"StackExchangeRedisExtensions.Serialize耗时：{(DateTime.Now - dtx).TotalMilliseconds}ms");

                dtx = DateTime.Now;
                var containerBag = StackExchangeRedisExtensions.Deserialize<ContainerBag>((RedisValue)serializedObj);//11ms
                Console.WriteLine($"StackExchangeRedisExtensions.Deserialize耗时：{(DateTime.Now - dtx).TotalMilliseconds}ms");

                Assert.AreEqual(containerBag.AddTime.Ticks, newObj.AddTime.Ticks);
                Assert.AreNotEqual(containerBag.GetHashCode(), newObj.GetHashCode());
            };

            Console.WriteLine("开始同步测试");
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

            Console.WriteLine("开始异步测试");
            var threadCount = 10;
            var finishCount = 0;
            for (int i = 0; i < threadCount; i++)
            {
                var thread = new Thread(() => {
                    var newObj = new ContainerBag()
                    {
                        Key = Guid.NewGuid().ToString(),
                        Name = Newtonsoft.Json.JsonConvert.SerializeObject(this),
                        AddTime = DateTime.Now.ToUniversalTime()
                    };

                    var dtx = DateTime.Now;
                    var serializedObj = MessagePackSerializer.Serialize(newObj/*, NativeDateTimeResolver.Instance*/);
                    Console.WriteLine($"MessagePackSerializer.Serialize 耗时：{(DateTime.Now - dtx).TotalMilliseconds}ms");

                    dtx = DateTime.Now;
                    var containerBag = MessagePackSerializer.Deserialize<ContainerBag>(serializedObj);//11ms
                    Console.WriteLine($"MessagePackSerializer.Deserialize 耗时：{(DateTime.Now - dtx).TotalMilliseconds}ms");

                    Console.WriteLine(containerBag.AddTime.ToUniversalTime());

                    //Assert.AreEqual(containerBag.AddTime.Ticks, newObj.AddTime.Ticks);
                    Assert.AreNotEqual(containerBag.GetHashCode(), newObj.GetHashCode());
                    finishCount++;
                });
                thread.Start();
            }

            while (finishCount < threadCount)
            {
                //等待
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

            Console.WriteLine("开始异步测试");
            var threadCount = 50;
            var finishCount = 0;
            for (int i = 0; i < threadCount; i++)
            {
                var thread = new Thread(() => {
                    var newObj = new ContainerBag()
                    {
                        Key = Guid.NewGuid().ToString(),
                        Name = Newtonsoft.Json.JsonConvert.SerializeObject(this),
                        AddTime = DateTime.Now.ToUniversalTime()
                    };

                    var dtx = DateTime.Now;
                    var serializedObj = Newtonsoft.Json.JsonConvert.SerializeObject(newObj);
                    Console.WriteLine($"Newtonsoft.Json.JsonConvert.SerializeObject 耗时：{(DateTime.Now - dtx).TotalMilliseconds}ms");

                    dtx = DateTime.Now;
                    var containerBag = Newtonsoft.Json.JsonConvert.DeserializeObject<ContainerBag>(serializedObj);//11ms
                    Console.WriteLine($"Newtonsoft.Json.JsonConvert.DeserializeObject 耗时：{(DateTime.Now - dtx).TotalMilliseconds}ms");

                    Console.WriteLine(containerBag.AddTime.ToUniversalTime());

                    //Assert.AreEqual(containerBag.AddTime.Ticks, newObj.AddTime.Ticks);
                    Assert.AreNotEqual(containerBag.GetHashCode(), newObj.GetHashCode());
                    finishCount++;
                });
                thread.Start();
            }

            while (finishCount < threadCount)
            {
                //等待
            }
        }
    }
}
