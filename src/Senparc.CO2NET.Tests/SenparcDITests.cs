using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Senparc.CO2NET.Cache;
using Senparc.CO2NET.RegisterServices;
using System;
using System.Collections.Generic;
using System.Text;

namespace Senparc.CO2NET.Tests
{
    [TestClass]
    public class SenparcDITests
    {
        public SenparcDITests()
        {
            //注册
            var mockEnv = new Mock<IHostingEnvironment>();
            mockEnv.Setup(z => z.ContentRootPath).Returns(() => UnitTestHelper.RootPath);
            RegisterService.Start(mockEnv.Object, new SenparcSetting() { IsDebug = true });


            var serviceCollection = new ServiceCollection();
            var configBuilder = new ConfigurationBuilder();
            var config = configBuilder.Build();
            serviceCollection.AddSenparcGlobalServices(config);
            serviceCollection.AddMemoryCache();//使用内存缓存
        }

        [TestMethod]
        public void GetServiceTest()
        {
            var memcache = SenparcDI.GetService<IMemoryCache>();
            Assert.IsNotNull(memcache);
            Console.WriteLine($"memcache HashCode：{memcache.GetHashCode()}");
            memcache = SenparcDI.GetService<IMemoryCache>();
            Console.WriteLine($"memcache 2 HashCode：{memcache.GetHashCode()}");

            var key = Guid.NewGuid().ToString();
            var dt = DateTime.Now;
            memcache.Set(key, dt);//直接使用缓存

            //使用本地缓存测试
            CacheStrategyFactory.RegisterObjectCacheStrategy(() => LocalObjectCacheStrategy.Instance);
            var cache = CacheStrategyFactory.GetObjectCacheStrategyInstance();

            var obj = cache.Get(key, true);//使用缓存策略获取
            Assert.IsNotNull(obj);
            Assert.AreEqual(dt, obj);
        }
    }
}
