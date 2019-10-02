using Microsoft.VisualStudio.TestTools.UnitTesting;
using Senparc.CO2NET.Cache;
using System;

namespace Senparc.CO2NET.Tests.Cache.Local.Lock
{
    [TestClass]
    public class LocalCacheLockTests
    {
        [TestMethod]
        public void LocalCacheLockTest()
        {
            var resourceName = "TestLocalCacheLock";
            var key = "test";
            int retryCount = 10;
            var retryDelay = TimeSpan.FromMilliseconds(20);

            using (var localCacheLock = LocalCacheLock.CreateAndLock(LocalObjectCacheStrategy.Instance, resourceName, key, retryCount, retryDelay).Lock())
            {
                //注意：常规情况下这里不能使用相同的 resourceName + key 组合，否则会造成死锁！！

                var dt0 = SystemTime.Now;
                Console.WriteLine($"锁定开始：{dt0}");
                while (SystemTime.DiffTotalMS(dt0) < retryCount * retryDelay.TotalMilliseconds + 1000)
                {
                    //确保足够的过期时间
                }

                Console.WriteLine($"localCacheLock.LockSuccessful：{localCacheLock.LockSuccessful}");

                using (var localCacheLock2 = LocalCacheLock.CreateAndLock(LocalObjectCacheStrategy.Instance, resourceName, key, retryCount, retryDelay).Lock())
                {
                    Console.WriteLine($"localCacheLock2.LockSuccessful：{localCacheLock2.LockSuccessful}");
                }

            }
        }
    }
}
