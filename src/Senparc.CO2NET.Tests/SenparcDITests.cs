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
            //BaseTest.RegisterServiceCollection();
        }

        [TestMethod]
        public void GetServiceTest()
        {
            BaseTest.RegisterServiceCollection();
            BaseTest.RegisterServiceStart(true);

            var memcache = SenparcDI.GetService<IMemoryCache>();
            Assert.IsNotNull(memcache);
            Console.WriteLine($"memcache HashCode：{memcache.GetHashCode()}");

            var key = Guid.NewGuid().ToString();
            var dt = SystemTime.Now;
            memcache.Set(key, dt);//直接使用缓存

            var memcache2 = SenparcDI.GetService<IMemoryCache>();
            Console.WriteLine($"memcache 2 HashCode：{memcache2.GetHashCode()}");

            Assert.AreEqual(memcache.GetHashCode(), memcache2.GetHashCode());//同一个全局对象

            var obj0 = memcache2.Get(key);
            Assert.IsNotNull(obj0);
            Assert.AreEqual(dt, obj0);

            //使用本地缓存测试
            CacheStrategyFactory.RegisterObjectCacheStrategy(() => LocalObjectCacheStrategy.Instance);
            var cache = CacheStrategyFactory.GetObjectCacheStrategyInstance();

            var obj1 = cache.Get(key, true);//使用缓存策略获取
            Assert.IsNotNull(obj1);
            Assert.AreEqual(dt, obj1);

            var obj2 = cache.Get<DateTime>(key, true);//获取明确类型对象
            Assert.IsNotNull(obj2);
            Assert.AreEqual(dt, obj2);

            Assert.AreEqual(obj0.GetHashCode(), obj2.GetHashCode());
        }
    }
}
