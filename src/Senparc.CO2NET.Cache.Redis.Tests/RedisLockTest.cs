using Microsoft.VisualStudio.TestTools.UnitTesting;
using Senparc.CO2NET.Tests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Senparc.CO2NET.Cache.Redis.Tests
{
    [TestClass]
    public class RedisLockTest : BaseTest
    {
        [TestMethod]
        public void LockTest()
        {
            var cache = CacheStrategyFactory.GetObjectCacheStrategyInstance();
            Console.WriteLine("Redis 配置：" + BaseTest._senparcSetting.Cache_Redis_Configuration);
            Console.WriteLine("当前缓存策略：" + cache.GetType());

            var lockResourceName = "LockTest";

            var threadCount = 60;// Define the number of threads
            var runningCount = 0;
            var finishThreadCount = 0;// Number of completed threads
            var retryCount = 10;// Retry count (Redis default is 10)
            var retryDelay = TimeSpan.FromMilliseconds(100);// Single retry wait time (Redis default is 10 milliseconds)
            Dictionary<int, Thread> threadCollection = new Dictionary<int, Thread>();// Thread collection

            var dtStart = SystemTime.Now;
            var startRun = false;
            // Configure all threads (to start as simultaneously as possible, ignore setup time, so set first, then start quickly in sequence)
            for (int i = 0; i < threadCount; i++)
            {
                var lockGroupKey = i % 4;// Divided into 4 groups
                var threadIndex = i;
                threadCollection[i] = new Thread(async () =>
                {
                    runningCount++;
                    // while (runningCount < threadCount)
                    {
                        // Wait for unified start
                    }
                    var dt1 = SystemTime.Now;
                    using (var syncLock =cache.BeginCacheLock(lockResourceName, lockGroupKey.ToString(), retryCount, retryDelay))
                    {
                        var runTime = SystemTime.Now;
                        var waitTime = (runTime - dt1).TotalMilliseconds;
                        Thread.Sleep(100);// Simulate thread processing time
                        finishThreadCount++;
                        Console.WriteLine($"{runTime.ToString("ss.ffffff")} - {SystemTime.Now.ToString("ss.ffffff")}\t\t 等待：{waitTime}ms \t\t获取syncLock完成（Group：{lockGroupKey}）：{threadIndex} - {syncLock.LockSuccessful}");
                    }
                });
                threadCollection[i].Name = $"Lock-{i}";
            }

            // Start all threads

            // This method will cause an infinite loop
            // threadCollection.Values.ToList().AsParallel().ForAll(thread => thread.Start());

            foreach (var thread in threadCollection.Values)
            {
                thread.Start();// Note: The actual start completion time of threads may not be in order
            }

            while (finishThreadCount < threadCount)
            {
                // Wait for completion
            }

            Console.WriteLine($"过程结束，总用时：{SystemTime.DiffTotalMS(dtStart)} ms");
        }
    }
}
