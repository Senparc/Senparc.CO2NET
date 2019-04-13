using Microsoft.VisualStudio.TestTools.UnitTesting;
using Senparc.CO2NET.Tests;
using System;
using System.Collections.Generic;
using System.Text;

namespace Senparc.CO2NET.Cache.Redis.Tests
{
    [TestClass]
    public class RedisLockTest : BaseTest
    {
        [TestMethod]
        public void LockTest()
        {
            var cache = CacheStrategyFactory.GetObjectCacheStrategyInstance();
            Console.WriteLine("Redis 配置："+BaseTest._senparcSetting.Cache_Redis_Configuration);
            Console.WriteLine("当前缓存策略："+cache.GetType());


        }
    }
}
