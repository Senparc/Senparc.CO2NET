﻿using MessagePack;
using MessagePack.Formatters;
using MessagePack.Resolvers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StackExchange.Redis;
using System;
using System.Threading;

namespace Senparc.CO2NET.Cache.CsRedis.Tests
{
    [TestClass]
    public class RedisHashSetTest
    {
        [TestMethod]
        public void HashSetSetTest()
        {
            RedisManager.ConfigurationOption = "10.37.129.2:6379";
            Senparc.CO2NET.Config.SenparcSetting.Cache_Redis_Configuration = RedisManager.ConfigurationOption;
            CsRedis.Register.SetConfigurationOption(RedisManager.ConfigurationOption);
            CsRedis.Register.UseKeyValueRedisNow();
            CacheStrategyFactory.RegisterObjectCacheStrategy(() => RedisHashSetObjectCacheStrategy.Instance);
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

            Console.WriteLine($"HashSet-SetTest single test takes time: {SystemTime.DiffTotalMS(dt)}ms");
        }
    }
}
