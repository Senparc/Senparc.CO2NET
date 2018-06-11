using Microsoft.VisualStudio.TestTools.UnitTesting;
using StackExchange.Redis;
using System;
using System.Threading;

namespace Senparc.CO2NET.Cache.Redis.Tests
{
    public class ContainerBag
    {
        public string Key { get; set; }
        public string Name { get; set; }
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

            Console.WriteLine($"SetTest单条测试耗时：{(DateTime.Now-dt).TotalMilliseconds}ms");
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
                var thread = new Thread(()=> {


                    CacheStrategyFactory.RegisterObjectCacheStrategy(() => RedisObjectCacheStrategy.Instance);
                    var cacheStrategy = CacheStrategyFactory.GetObjectCacheStrategyInstance();

                    

                    var dt = DateTime.Now;
                    var dtx = DateTime.Now;
                    cacheStrategy.InsertToCache("RedisTest_"+dt.Ticks, new ContainerBag()
                    {
                        Key = "123",
                        Name = Newtonsoft.Json.JsonConvert.SerializeObject(this),
                        AddTime = dt
                    });//37ms

                    var obj = cacheStrategy.Get("RedisTest_"+ dt.Ticks);//14-25ms
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

            while (finishCount< threadCount)
            {
                //等待
            }

            Console.WriteLine($"EfficiencyTest总测试时间：{(DateTime.Now - dt1).TotalMilliseconds}ms");
        }
    }
}
