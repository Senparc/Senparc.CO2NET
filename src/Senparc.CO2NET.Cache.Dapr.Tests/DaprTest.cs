using MessagePack;
using Senparc.CO2NET.Cache.Dapr.ObjectCacheStrategy;
using System.Runtime.CompilerServices;

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
            public DateTimeOffset AddTime { get; set; }
        }

        private static string Me([CallerMemberName] string caller = "") => caller;

        private static IBaseObjectCacheStrategy GetCacheStrategy()
        {
            DaprStateManager.StoreName = "statestore";
            DaprStateManager.HttpEndPoint = "http://localhost:3500";
            CacheStrategyFactory.RegisterObjectCacheStrategy(() => DaprStateObjectCacheStrategy.Instance);
            return CacheStrategyFactory.GetObjectCacheStrategyInstance();
        }

        private static ContainerBag MakeBag()
        {
            return new ContainerBag()
            {
                Key = Guid.NewGuid().ToString(),
                Name = "",
                AddTime = SystemTime.Now
            };
        }

        [TestMethod]
        public void SetTest()
        {
            var cacheStrategy = GetCacheStrategy();

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
            var cacheStrategy = GetCacheStrategy();

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
            var cacheStrategy = GetCacheStrategy();

            var key = Me();
            var bag = MakeBag();

            cacheStrategy.Set(key, bag, TimeSpan.FromSeconds(100));

            Thread.Sleep(1000);//等待
            var result = cacheStrategy.Get(key);
            Assert.IsNotNull(result);//未过期

            cacheStrategy.Update(key, bag, TimeSpan.FromSeconds(1));//重新设置时间
            result = cacheStrategy.Get(key);
            Assert.IsNotNull(result);

            var strongEntity = cacheStrategy.Get<ContainerBag>(key);
            Assert.IsNotNull(strongEntity);
            Assert.AreEqual(bag.AddTime, strongEntity.AddTime);

            Thread.Sleep(1000);//让缓存过期
            result = cacheStrategy.Get(key);
            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task ExpiryAsyncTest()
        {
            var cacheStrategy = GetCacheStrategy();

            var key = Me();
            var bag = MakeBag();

            await cacheStrategy.SetAsync(key, bag, TimeSpan.FromSeconds(1));

            var entity = await cacheStrategy.GetAsync<ContainerBag>(key);
            Assert.IsNotNull(entity);

            var result = await cacheStrategy.GetAsync<ContainerBag>(key);
            Assert.IsNotNull(result);
            Assert.AreEqual(bag.AddTime, result.AddTime);

            Thread.Sleep(1000);//让缓存过期
            entity = await cacheStrategy.GetAsync<ContainerBag>(key);
            Assert.IsNull(entity);
        }
    }
}