using MessagePack;
using System.Runtime.CompilerServices;

namespace Senparc.CO2NET.Cache.Dapr.Tests
{
    [TestClass]
    public class DaprTest
    {
        [TestMethod]
        public void SetTest()
        {
            var cacheStrategy = DaprTestConfig.GetCacheStrategy();

            var key = Me();
            var bag = MakeBag();

            cacheStrategy.Set(key, bag);

            var result = cacheStrategy.Get<ContainerBag>(key);
            Assert.IsNotNull(result);
            Assert.AreEqual(bag.AddTime, result.AddTime);

            Console.WriteLine($"SetTest单条测试耗时：{SystemTime.DiffTotalMS(bag.AddTime)}ms");
        }

        [TestMethod]
        public async Task SetAsyncTest()
        {
            var cacheStrategy = DaprTestConfig.GetCacheStrategy();

            var key = Me();
            var bag = MakeBag();

            await cacheStrategy.SetAsync(key, bag);

            var result = await cacheStrategy.GetAsync<ContainerBag>(key);
            Assert.IsNotNull(result);
            Assert.AreEqual(bag.AddTime, result.AddTime);

            Console.WriteLine($"SetTest单条测试耗时：{SystemTime.DiffTotalMS(bag.AddTime)}ms");
        }

        [TestMethod]
        public void ExpiryTest()
        {
            var cacheStrategy = DaprTestConfig.GetCacheStrategy();

            var key = Me();
            var bag = MakeBag();

            cacheStrategy.Set(key, bag, TimeSpan.FromSeconds(100));

            //Cache should not expire after 1s
            Thread.Sleep(1000);
            var result = cacheStrategy.Get(key);
            Assert.IsNotNull(result);

            //Reset cache lifetime to 1s
            cacheStrategy.Update(key, bag, TimeSpan.FromSeconds(1));
            result = cacheStrategy.Get(key);
            Assert.IsNotNull(result);

            var strongEntity = cacheStrategy.Get<ContainerBag>(key);
            Assert.IsNotNull(strongEntity);
            Assert.AreEqual(bag.AddTime, strongEntity.AddTime);
            
            //Wait 1s to let the cache expire
            Thread.Sleep(1000);
            result = cacheStrategy.Get(key);
            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task ExpiryAsyncTest()
        {
            var cacheStrategy = DaprTestConfig.GetCacheStrategy();

            var key = Me();
            var bag = MakeBag();

            await cacheStrategy.SetAsync(key, bag, TimeSpan.FromSeconds(1));

            var entity = await cacheStrategy.GetAsync<ContainerBag>(key);
            Assert.IsNotNull(entity);

            var result = await cacheStrategy.GetAsync<ContainerBag>(key);
            Assert.IsNotNull(result);
            Assert.AreEqual(bag.AddTime, result.AddTime);

            //Cache expires after 1s
            Thread.Sleep(1000);
            entity = await cacheStrategy.GetAsync<ContainerBag>(key);
            Assert.IsNull(entity);
        }

        [MessagePackObject(keyAsPropertyName: true)]
        private class ContainerBag
        {
            [Key(0)]
            public string Key { get; set; }
            [Key(1)]
            public string Name { get; set; }
            [Key(2)]
            public DateTimeOffset AddTime { get; set; }
        }

        [TestMethod]
        public void EfficiencyTest()
        {
            var dt = SystemTime.Now;
            for (int i = 0; i < 100; i++)
            {
                SetTest();
            }

            Console.WriteLine($"EfficiencyTest总测试时间（使用CacheWrapper)：{SystemTime.DiffTotalMS(dt)}ms");
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
        //            var dtx = SystemTime.Now;
        //            var cacheStrategy = DaprTestConfig.GetCacheStrategy();

        //            SetTest();

        //            Console.WriteLine($"Time taken for single test in thread: {SystemTime.DiffTotalMS(dtx)}ms");

        //            finishCount++;
        //        });
        //        thread.Start();
        //    }

        //    while (finishCount < threadCount)
        //    {
        //        //Wait
        //    }

        //    Console.WriteLine($"Total test time for EfficiencyTest: {SystemTime.DiffTotalMS(dt1)}ms");
        //}

        private static ContainerBag MakeBag()
        {
            return new ContainerBag()
            {
                Key = Guid.NewGuid().ToString(),
                Name = "",
                AddTime = SystemTime.Now
            };
        }

        private static string Me([CallerMemberName] string caller = "") => caller;
    }
}