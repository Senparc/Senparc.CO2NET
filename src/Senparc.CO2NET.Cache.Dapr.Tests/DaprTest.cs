using MessagePack;
using Senparc.CO2NET.Cache.Dapr.ObjectCacheStrategy;

namespace Senparc.CO2NET.Cache.Dapr.Tests
{
    [TestClass]
    public class DaprTest
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

        [TestMethod]
        public void SetTest()
        {
            DaprStateManager.StoreName = "statestore";
            DaprStateManager.HttpEndPoint = "http://localhost:3500";
            CacheStrategyFactory.RegisterObjectCacheStrategy(() => DaprStateObjectCacheStrategy.Instance);
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

            Console.WriteLine($"SetTest单条测试耗时：{SystemTime.DiffTotalMS(dt)}ms");
        }

        [TestMethod]
        public void SetAsyncTest()
        {
            DaprStateManager.StoreName = "statestore";
            DaprStateManager.HttpEndPoint = "http://localhost:3500";
            CacheStrategyFactory.RegisterObjectCacheStrategy(() => DaprStateObjectCacheStrategy.Instance);
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

            Console.WriteLine($"SetTest单条测试耗时：{SystemTime.DiffTotalMS(dt)}ms");
        }

        [TestMethod]
        public void ExpiryTest()
        {
            DaprStateManager.StoreName = "statestore";
            DaprStateManager.HttpEndPoint = "http://localhost:3500";
            CacheStrategyFactory.RegisterObjectCacheStrategy(() => DaprStateObjectCacheStrategy.Instance);
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
            Thread.Sleep(1000);//等待
            var entity = cacheStrategy.Get(key);
            Assert.IsNotNull(entity);//未过期

            cacheStrategy.Update(key, value, TimeSpan.FromSeconds(1));//重新设置时间
            entity = cacheStrategy.Get(key);
            Assert.IsNotNull(entity);

            var strongEntity = cacheStrategy.Get<ContainerBag>(key);
            Assert.IsNotNull(strongEntity);
            Assert.AreEqual(dt, strongEntity.AddTime);

            Thread.Sleep(1000);//让缓存过期
            entity = cacheStrategy.Get(key);
            Assert.IsNull(entity);
        }

        [TestMethod]
        public void ExpiryAsyncTest()
        {
            DaprStateManager.StoreName = "statestore";
            DaprStateManager.HttpEndPoint = "http://localhost:3500";
            CacheStrategyFactory.RegisterObjectCacheStrategy(() => DaprStateObjectCacheStrategy.Instance);
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

            Thread.Sleep(1000);//让缓存过期
            entity = cacheStrategy.GetAsync(key).Result;
            Assert.IsNull(entity);
        }

        #region 性能相关测试

        [TestMethod]
        public void EfficiencyTest()
        {
            var dt1 = SystemTime.Now;
            for (int i = 0; i < 100; i++)
            {
                SetTest();
            }

            Console.WriteLine($"EfficiencyTest总测试时间（使用CacheWrapper)：{SystemTime.DiffTotalMS(dt1)}ms");
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


        //            Console.WriteLine($"Thread内单条测试耗时：{SystemTime.DiffTotalMS(dtx)}ms");

        //            finishCount++;
        //        });
        //        thread.Start();
        //    }

        //    while (finishCount < threadCount)
        //    {
        //        //等待
        //    }

        //    Console.WriteLine($"EfficiencyTest总测试时间：{SystemTime.DiffTotalMS(dt1)}ms");
        //}
        #endregion

    }
}