﻿using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Senparc.CO2NET.RegisterServices;
using Senparc.CO2NET.AspNet.RegisterServices;
using Senparc.CO2NET.Tests;
using System;
using System.Collections.Generic;
using System.Text;

namespace Senparc.CO2NET.Cache.CsRedis.Tests
{
    [TestClass]
    public class InitTests
    {
        /// <summary>
        /// Test whether the Redis connection string can take effect immediately during the UseSenparcGlobal() phase
        /// </summary>
        [TestMethod]
        public void AutoRegisterConfigurationTest()
        {
            // Perform regular registration
            var serviceCollection = new ServiceCollection();
            var configBuilder = new ConfigurationBuilder();
            var config = configBuilder.Build();
            serviceCollection.AddSenparcGlobalServices(config);
            serviceCollection.AddMemoryCache();// Use memory cache

            var mockEnv = new Mock<Microsoft.Extensions.Hosting.IHostEnvironment/*IHostingEnvironment*/>();
            mockEnv.Setup(z => z.ContentRootPath).Returns(() => UnitTestHelper.RootPath);

            RedisManager.ConfigurationOption = null;// Clear before testing

            var redisServer = "localhost:6379";

            Senparc.CO2NET.Config.SenparcSetting.IsDebug = true;
            Senparc.CO2NET.Config.SenparcSetting.Cache_Redis_Configuration = redisServer;


            //var senparcSetting = new SenparcSetting()
            //{
            //    IsDebug = true,
            //    Cache_Redis_Configuration = redisServer
            //};


            var registerService = Senparc.CO2NET.AspNet.RegisterServices.
                                    RegisterService.Start(mockEnv.Object/*, senparcSetting*/)
                 .UseSenparcGlobal();
            Assert.AreEqual(null, RedisManager.ConfigurationOption);// Not registered yet

            Register.SetConfigurationOption(redisServer);
            Assert.AreEqual(redisServer, RedisManager.ConfigurationOption);

            var currentCache = CacheStrategyFactory.GetObjectCacheStrategyInstance();
            Assert.IsInstanceOfType(currentCache, typeof(RedisObjectCacheStrategy));
        }
    }
}
