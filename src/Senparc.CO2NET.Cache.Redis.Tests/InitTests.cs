using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Senparc.CO2NET.RegisterServices;
using Senparc.CO2NET.Tests;
using System;
using System.Collections.Generic;
using System.Text;

namespace Senparc.CO2NET.Cache.Redis.Tests
{
    [TestClass]
    public class InitTests
    {
        /// <summary>
        /// 测试Redis连接字符串是否可以在 UseSenparcGlobal() 阶段就立即生效
        /// </summary>
        [TestMethod]
        public void AutoRegisterConfigurationTest()
        {
            //进行常规的注册
            var serviceCollection = new ServiceCollection();
            var configBuilder = new ConfigurationBuilder();
            var config = configBuilder.Build();
            serviceCollection.AddSenparcGlobalServices(config);
            serviceCollection.AddMemoryCache();//使用内存缓存

            var mockEnv = new Mock<IHostingEnvironment>();
            mockEnv.Setup(z => z.ContentRootPath).Returns(() => UnitTestHelper.RootPath);
            var redisServer = "localhost:6379";
           var registerService = RegisterService.Start(mockEnv.Object, new SenparcSetting() { IsDebug = true, Cache_Redis_Configuration= redisServer })
                .UseSenparcGlobal();

            Assert.AreEqual(null, RedisManager.ConfigurationOption);//当前还没有进行注册

            registerService.RegisterCacheRedis(
                     redisServer,
                     redisConfiguration => RedisObjectCacheStrategy.Instance//第一次调用时会自动注册
                        );
            Assert.AreEqual(redisServer, RedisManager.ConfigurationOption);

            var currentCache = CacheStrategyFactory.GetObjectCacheStrategyInstance();
            Assert.IsInstanceOfType(currentCache, typeof(RedisObjectCacheStrategy));
        }
    }
}
