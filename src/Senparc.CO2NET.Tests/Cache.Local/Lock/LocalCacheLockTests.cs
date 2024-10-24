using Microsoft.VisualStudio.TestTools.UnitTesting;
using Senparc.CO2NET.Cache;
using System;

namespace Senparc.CO2NET.Tests.Cache.Local.Lock
{
    [TestClass]
    public class LocalCacheLockTests:BaseTest
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
                // Note: Ensure that different resourceName + key combinations are not used simultaneously to avoid conflicts

                var dt0 = SystemTime.Now;
                Console.WriteLine($"Lock start: {dt0}"); while (SystemTime.DiffTotalMS(dt0) < retryCount * retryDelay.TotalMilliseconds + 1000)
                {
                    // Ensure sufficient processing time
                }

                Console.WriteLine($"localCacheLock.LockSuccessful: {localCacheLock.LockSuccessful}");

                using (var localCacheLock2 = LocalCacheLock.CreateAndLock(LocalObjectCacheStrategy.Instance, resourceName, key, retryCount, retryDelay).Lock())
                {
                    Console.WriteLine($"localCacheLock2.LockSuccessful: {localCacheLock2.LockSuccessful}");
                }

            }
        }
    }
}
