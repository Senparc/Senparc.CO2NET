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
    public class SenparcDITests : BaseTest
    {
        public SenparcDITests()
        {
            //BaseTest.RegisterServiceCollection();
        }

        [TestMethod]
        public void GetServiceTest()
        {
            var serviceProvider = BaseTest.serviceProvider;
            BaseTest.RegisterServiceStart(true);

            //Assert.AreSame(serviceProvider, SenparcDI.GlobalServiceProvider);

            var memcache = serviceProvider.GetService<IMemoryCache>();
            Assert.IsNotNull(memcache);
            Console.WriteLine($"memcache HashCode：{memcache.GetHashCode()}");

            var key = Guid.NewGuid().ToString();
            var dt = SystemTime.Now;
            memcache.Set(key, dt);//Directly use cache

            var memcache2 = serviceProvider.GetService<IMemoryCache>();
            Console.WriteLine($"memcache 2 HashCode：{memcache2.GetHashCode()}");

            Assert.AreEqual(memcache.GetHashCode(), memcache2.GetHashCode());//Same global object

            var obj0 = memcache2.Get(key);
            Assert.IsNotNull(obj0);
            Assert.AreEqual(dt, obj0);

            //Test using local cache
            CacheStrategyFactory.RegisterObjectCacheStrategy(() => LocalObjectCacheStrategy.Instance);
            var cache = CacheStrategyFactory.GetObjectCacheStrategyInstance();

            var obj1 = cache.Get(key, true);//Using cache strategy to get, ServiceProvider is inconsistent, so the result cannot be obtained
            Assert.IsNull(obj1);

            //Store again
            cache.Set(key, dt, null, true);
            obj1 = cache.Get(key, true);
            Assert.AreEqual(dt, obj1);

            var obj2 = cache.Get<DateTimeOffset>(key, true);//Get specific type object
            Assert.IsNotNull(obj2);
            Assert.AreEqual(dt, obj2);

            Assert.AreEqual(obj0.GetHashCode(), obj2.GetHashCode());
        }

        /// <summary>
        /// Test singleton instance and BuildServiceProvider() method efficiency
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
                //Make some changes each time
                if (i % 2 == 0)
                {
                    services.AddScoped<Senparc.CO2NET.Utilities.BrowserUtility>();
                    services.AddScoped<Senparc.CO2NET.Helpers.DateTimeHelper>();
                    services.AddScoped<Senparc.CO2NET.Helpers.FileHelper>();
                }

                var dt1 = SystemTime.Now;
                var provider = services.BuildServiceProvider();
                Console.WriteLine($"HashCode:{provider.GetHashCode()},Time:{SystemTime.DiffTotalMS(dt1)}ms");


                Console.WriteLine("进行Scope测试");
                for (int j = 0; j < 2; j++)
                {
                    using (var scope = b1.CreateScope())
                    {
                        Console.WriteLine($"{j}.\tBuildServiceProvider:{b1.GetHashCode()}");
                        Console.WriteLine($"{j}.\tscope.ServiceProvider:{scope.ServiceProvider.GetHashCode()}");
                        //Test result: the two ServiceProviders are not the same object


                        var scopeS1 = scope.ServiceProvider.GetRequiredService<SenparcSetting>();
                        Console.WriteLine($"{j}.\tscopeS1:{scope.ServiceProvider.GetHashCode()}");

                    }
                }


            }
        }

        /// <summary>
        /// Test global and thread-scoped ServiceProvider
        /// </summary>
        [TestMethod]
        public void ThreadAndGlobalServiceTest()
        {
            BaseTest.RegisterServiceCollection();
            BaseTest.RegisterServiceStart(true);

            SenparcDI.GlobalServiceCollection.AddScoped<SenparcSetting>();

            BaseTest.serviceProvider = SenparcDI.GlobalServiceCollection.BuildServiceProvider();
            //Test unique across threads
            var s = BaseTest.serviceProvider.GetService<SenparcSetting>();
            Console.WriteLine($"s:{s.GetHashCode()}");

            var threadsCount = 10;

            Console.WriteLine("======= 开始全局唯一测试 =======");
            var finishedThread = 0;
            for (int i = 0; i < threadsCount; i++)
            {
                var index = i;
                var thread = new Thread(() =>
                {
                    var s1 = serviceProvider.GetService<SenparcSetting>();
                    var s2 = serviceProvider.GetService<SenparcSetting>();
                    Console.WriteLine("ServiceProvider:" + serviceProvider.GetHashCode());
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
            //All HashCodes are the same

            //TODO: Now all unified to global

            //Test unique within thread
            Console.WriteLine("======= 开始线程内唯一测试 =======");
            finishedThread = 0;
            for (int i = 0; i < threadsCount; i++)
            {
                var thread = new Thread(() =>
                {
                    var index = i;
                    Console.WriteLine($"-------{index}----------");

                    //var threadScope = Thread.GetData(Thread.GetNamedDataSlot(CO2NET.SenparcDI.SENPARC_DI_THREAD_SERVICE_Scope)) as IServiceScope;
                    //Console.WriteLine("ServiceScope:" + threadScope?.GetHashCode());
                    //Console.WriteLine("ServiceProvider:" + threadScope?.ServiceProvider.GetHashCode());

                    using (var threadScopeService = serviceProvider.CreateScope())
                    {
                        var s1 = threadScopeService.ServiceProvider.GetService<SenparcSetting>();
                        var s2 = threadScopeService.ServiceProvider.GetService<SenparcSetting>();
                        Console.WriteLine($"{index}:{s1.GetHashCode()}");
                        Console.WriteLine($"{index}:{s2.GetHashCode()}");
                        Assert.AreEqual(s1.GetHashCode(), s2.GetHashCode());

                        //threadScope = Thread.GetData(Thread.GetNamedDataSlot(CO2NET.SenparcDI.SENPARC_DI_THREAD_SERVICE_Scope)) as IServiceScope;
                        //Console.WriteLine("ServiceScope:" + threadScope.GetHashCode());
                        //Console.WriteLine("ServiceProvider:" + threadScope.ServiceProvider.GetHashCode());
                        Console.WriteLine("-----------------");
                        finishedThread++;
                    }
                });
                thread.Start();
            }

            while (finishedThread != threadsCount)
            {
            }
            //HashCode is the same within a single thread

        }
    }
}
