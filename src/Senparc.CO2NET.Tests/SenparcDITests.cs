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
using System.Threading;

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

            var obj2 = cache.Get<DateTimeOffset>(key, true);//获取明确类型对象
            Assert.IsNotNull(obj2);
            Assert.AreEqual(dt, obj2);

            Assert.AreEqual(obj0.GetHashCode(), obj2.GetHashCode());
        }

        /// <summary>
        /// 测试对象单例情况，以及 BuildServiceProvider() 方法效率
        /// </summary>
        [TestMethod]
        public void BuildProviderTest()
        {
            for (int i = 0; i < 10; i++)
            {
                var services = new ServiceCollection();
                services.AddSingleton<Senparc.CO2NET.SenparcSetting>();

                var b1 = services.BuildServiceProvider();
                var b2 = services.BuildServiceProvider();
                var s1 = b1.GetRequiredService<SenparcSetting>();
                var s2 = b1.GetRequiredService<SenparcSetting>();
                Console.WriteLine($"s1-b1:{s1.GetHashCode()}");
                Console.WriteLine($"s2-b1:{s2.GetHashCode()}");

                s2 = b2.GetRequiredService<SenparcSetting>();
                Console.WriteLine($"s2-b2:{s2.GetHashCode()}");

                services.AddScoped<Senparc.CO2NET.Helpers.EncryptHelper>();
                services.AddScoped<Senparc.CO2NET.MessageQueue.MessageQueueDictionary>();
                services.AddScoped<Senparc.CO2NET.Utilities.ServerUtility>();
                if (i % 2 == 0)
                {
                    services.AddScoped<Senparc.CO2NET.Utilities.BrowserUtility>();
                    services.AddScoped<Senparc.CO2NET.Helpers.DateTimeHelper>();
                    services.AddScoped<Senparc.CO2NET.Helpers.FileHelper>();
                }

                var dt1 = SystemTime.Now;
                var provider = services.BuildServiceProvider();
                Console.WriteLine($"HashCode:{provider.GetHashCode()},Time:{(SystemTime.Now - dt1).TotalMilliseconds}ms");
            }
        }

        /// <summary>
        /// 测试
        /// </summary>
        [TestMethod]
        public void ThreadAndGlobalServiceTest()
        {
            BaseTest.RegisterServiceCollection();
            BaseTest.RegisterServiceStart(true);

            SenparcDI.GlobalServiceCollection.AddSingleton<SenparcSetting>();

            //测试跨线程唯一
            var s = SenparcDI.GetService<SenparcSetting>(true);
            Console.WriteLine($"s:{s.GetHashCode()}");

            var threadsCount = 3;

            Console.WriteLine("======= 开始全局唯一测试 =======");
            var finishedThread = 0;
            for (int i = 0; i < threadsCount; i++)
            {
                var index = i;
                var thread = new Thread(() =>
                {
                    var s1 = SenparcDI.GetService<SenparcSetting>(true);
                    var s2 = SenparcDI.GetService<SenparcSetting>(true);
                    Console.WriteLine("ServiceProcider:" + SenparcDI.GlobalIServiceProvider?.GetHashCode());
                    Console.WriteLine($"{index}:{s1.GetHashCode()}");
                    Console.WriteLine($"{index}:{s2.GetHashCode()}");
                    Assert.AreEqual(s1.GetHashCode(), s2.GetHashCode());
                    finishedThread++;
                });
                thread.Start();
            }

            while (finishedThread != threadsCount)
            {
            }
            //所有HashCode相同

            //测试通线程唯一
            Console.WriteLine("======= 开始线程内唯一测试 =======");
            finishedThread = 0;
            for (int i = 0; i < threadsCount; i++)
            {
                var thread = new Thread(() =>
                {
                    var index = i;
                    Console.WriteLine("ServiceProcider:" + Thread.GetData(Thread.GetNamedDataSlot(CO2NET.SenparcDI.SENPARC_DI_THREAD_SERVICE_PROVIDER))?.GetHashCode());
                    var s1 = SenparcDI.GetService<SenparcSetting>(false);
                    var s2 = SenparcDI.GetService<SenparcSetting>(false);
                    Console.WriteLine($"{index}:{s1.GetHashCode()}");
                    Console.WriteLine($"{index}:{s2.GetHashCode()}");
                    Assert.AreEqual(s1.GetHashCode(), s2.GetHashCode());
                    Console.WriteLine("ServiceProcider:" + Thread.GetData(Thread.GetNamedDataSlot(CO2NET.SenparcDI.SENPARC_DI_THREAD_SERVICE_PROVIDER))?.GetHashCode());

                    finishedThread++;
                });
                thread.Start();
            }

            while (finishedThread != threadsCount)
            {
            }
            //单线程内HashCode相同

        }
    }
}
