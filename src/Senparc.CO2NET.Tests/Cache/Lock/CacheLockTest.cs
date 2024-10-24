using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Senparc.CO2NET.Cache;
using Senparc.CO2NET.Cache.Redis;

namespace Senparc.CO2NET.Cache.Lock
{
    [TestClass]
    public class CacheLockTest
    {
        /// <summary>
        /// Test the parallel quantity of Parallel and Thread
        /// </summary>
        //[TestMethod]
        public void TestParallelThreadsRunAtSameTime()
        {
            var dt1 = SystemTime.Now;
            Parallel.For(0, 100, i =>
            {
                Console.WriteLine("T:{0}，Use Time：{1}ms", Thread.CurrentThread.GetHashCode(),
                    SystemTime.DiffTotalMS(dt1));
                Thread.Sleep(20);
            });

            var dt2 = SystemTime.Now;
            Console.WriteLine("Working Threads Count:{0}", 100 * 20 / (dt2 - dt1).TotalMilliseconds);
            //Test result: The number of threads running simultaneously is about 4 (average 3.6), visually observed as 5

            Console.WriteLine("Test Threads");

            List<Thread> list = new List<Thread>();
            int[] runThreads = { 0 };

            for (int i = 0; i < 100; i++)
            {
                runThreads[0]++;
                var thread = new Thread(() =>
                {
                    Console.WriteLine("T:{0}，Use Time：{1}ms", Thread.CurrentThread.GetHashCode(),
                        SystemTime.DiffTotalMS(dt2));
                    Thread.Sleep(20);
                    runThreads[0]--;
                });
                list.Add(thread);
            }

            var dt3 = SystemTime.Now;
            list.ForEach(z => z.Start());
            while (runThreads[0] > 0)
            {
                //Thread.Sleep(100);
            }
            var dt4 = SystemTime.Now;
            Console.WriteLine("Working Threads Count:{0}", 1000 * 20 / (dt4 - dt3).TotalMilliseconds);
            //Test result: No limit
        }

        [TestMethod]
        public void LockTest()
        {
            /* Test logic:
             * 20 asynchronous threads run simultaneously,
             * Each thread has two different ResourceNames: "Test-0", "Test-1", simulating AccessTokenContainer and JsTicketContainer),
             * Each ResourceName has two appIds, 0 and 1.
             * Run these threads simultaneously to observe if the lock works.
             * Failure phenomena: 1. Two threads obtain the lock simultaneously under the same ResourceName and appId
             *                    2. Interference between different combinations of ResourceName and appId;
             *                    3. Deadlock;
             *                    4. A thread can never obtain the lock (timeout or always running)
             *                    
             * Note: Failures due to timeout may be caused by the maximum wait time being too short relative to the Thread.Sleep(sleepMillionSeconds) set for the test.
             */

            Console.WriteLine("Synchronous Method Test");
            bool useRedis = true;

            if (useRedis)
            {
                var redisConfiguration = "localhost:6379";
                RedisManager.ConfigurationOption = redisConfiguration;
                CacheStrategyFactory.RegisterObjectCacheStrategy(() => RedisObjectCacheStrategy.Instance);//Redis
            }

            Console.WriteLine($"Using Cache Strategy: {CacheStrategyFactory.GetObjectCacheStrategyInstance()}");
            Random rnd = new Random();
            var threadsCount = 20M;
            int sleepMillionSeconds = 200;//The larger the number, the longer each thread occupies time, until timeout

            //Preliminary estimate of retry time required
            var differentLocksCount = (2 /*appId*/* 2 /*resource*/) /* = different locks count */;
            var runCycle = threadsCount / differentLocksCount /* = Run Cycle*/;
            var hopedTotalTime = runCycle * (sleepMillionSeconds + 100) /* = Hoped Total Time */;
            var retryDelay = 20; /* = retryDelayCycle MillionSeconds */
            var randomRetryDelay = (retryDelay / 2) /*random retry delay*/;
            var retryTimes = 50;// hopedTotalTime / randomRetryDelay; /* = maximum retry times */;//This number can be adjusted (gradually reduced based on test results, failures can be observed)

            Console.WriteLine("sleepMillionSeconds:{0}ms", sleepMillionSeconds);
            Console.WriteLine("differentLocksCount:{0}", differentLocksCount);
            Console.WriteLine("runCycle:{0}", runCycle);
            Console.WriteLine("hopedTotalTime:{0}ms", hopedTotalTime);
            Console.WriteLine("randomRetryDelay:{0}ms", randomRetryDelay);
            Console.WriteLine("retryTimes:{0}", retryTimes);
            Console.WriteLine("randomLockTime (possible Lock timeout): {0}ms", randomRetryDelay * retryTimes); 
            Console.WriteLine("=====================");

            List<Thread> list = new List<Thread>();
            int[] runThreads = { 0 };
            var dt0 = SystemTime.Today;
            for (int i = 0; i < (int)threadsCount; i++)
            {
                runThreads[0]++;
                var i1 = i;
                var thread = new Thread(() =>
                {
                    var appId = (i1 % 2).ToString();
                    var resourceName = "Test-" + rnd.Next(0, 2);//Adjust the random number here to change the number of locks
                    var cache = CacheStrategyFactory.GetObjectCacheStrategyInstance();//Reacquire the instance each time (since it's a singleton, it's actually the same one)

                    Console.WriteLine($"线程 {i1} / {resourceName} : {appId} 进入，准备尝试锁。Cache实例：{cache.GetHashCode()}");

                    var dt1 = SystemTime.Now;
                    using (var cacheLock = cache.BeginCacheLock(resourceName, appId, (int)retryTimes, TimeSpan.FromMilliseconds(retryDelay)))
                    {
                        var result = cacheLock.LockSuccessful
                            ? "Success"
                            : "Failed";
                        result += " | TTL：" + cacheLock.GetTotalTtl(retryTimes, TimeSpan.FromMilliseconds(retryDelay)) + "ms";

                        Console.WriteLine($"Thread {i1} / {resourceName} : {appId} 进入锁，等待时间：{SystemTime.DiffTotalMS(dt1)} / {SystemTime.DiffTotalMS(dt0)}ms，获得锁结果：{result}");

                        Thread.Sleep(sleepMillionSeconds);
                    }
                    Console.WriteLine($"Thread {i1} / {resourceName} : {appId} 释放锁（{SystemTime.DiffTotalMS(dt1)}ms）");
                    runThreads[0]--;
                });
                list.Add(thread);
            }


            var dtAll1 = SystemTime.Now;

            list.ForEach(z => z.Start());

            while (runThreads[0] > 0)
            {
                Thread.Sleep(10);
            }

            Console.WriteLine("Real Time:{0}ms", SystemTime.DiffTotalMS(dtAll1));

            //Thread.Sleep((int)hopedTotalTime + 1000);
            //while (true)
            //{
            //    Thread.Sleep(10);
            //    if (list.Count(z => z.ThreadState == ThreadState.Aborted) == list.Count())
            //    {
            //        break;
            //    }
            //}

            //Wait
            //Parallel.For(0, (int)threadsCount, i =>
            //{
            //    var appId = (i % 2).ToString();
            //    var resourceName = "Test-" + rnd.Next(0, 2);
            //    Console.WriteLine("Thread {0} / {1} : {2} entering, preparing to try lock", Thread.CurrentThread.GetHashCode(), resourceName, appId);

            //    DateTime dt1 = SystemTime.Now;
            //    using (var cacheLock = Cache.BeginCacheLock(resourceName, appId, (int)retryTimes, new TimeSpan(0, 0, 0, 0, 20)))
            //    {
            //        var result = cacheLock.LockSuccessful
            //            ? "Success"
            //            : "【Failure!】";

            //        Console.WriteLine("Thread {0} / {1} : {2} entered lock, wait time: {3}ms, lock result: {4}", Thread.CurrentThread.GetHashCode(), resourceName, appId, SystemTime.DiffTotalMS(dt1), result);

            //        Thread.Sleep(sleepMillionSeconds);
            //    }
            //    Console.WriteLine("Thread {0} / {1} : {2} released lock ({3}ms)", Thread.CurrentThread.GetHashCode(), resourceName, appId, SystemTime.DiffTotalMS(dt1));
            //});
        }

        [TestMethod]
        public void LockAsyncTest()
        {
            /* Test logic:
             * 20 asynchronous threads run simultaneously,
             * Each thread has two different ResourceNames: "Test-0", "Test-1", simulating AccessTokenContainer and JsTicketContainer),
             * Each ResourceName has two appIds, 0 and 1.
             * Run these threads simultaneously to observe if the lock works.
             * Failure phenomena: 1. Two threads obtain the lock simultaneously under the same ResourceName and appId
             *                    2. Interference between different combinations of ResourceName and appId;
             *                    3. Deadlock;
             *                    4. A thread can never obtain the lock (timeout or always running)
             *                    
             * Note: Failures due to timeout may be caused by the maximum wait time being too short relative to the Thread.Sleep(sleepMillionSeconds) set for the test.
             */

            Console.WriteLine("Asynchronous Method Test");
            bool useRedis = true;

            if (useRedis)
            {
                var redisConfiguration = "localhost:6379";
                RedisManager.ConfigurationOption = redisConfiguration;
                CacheStrategyFactory.RegisterObjectCacheStrategy(() => RedisObjectCacheStrategy.Instance);//Redis
            }

            Console.WriteLine("Asynchronous Method Test");
            Random rnd = new Random();
            var threadsCount = 20M;
            int sleepMillionSeconds = 200;//The larger the number, the longer each thread occupies time, until timeout

            //Preliminary estimate of retry time required
            var differentLocksCount = (2 /*appId*/* 2 /*resource*/) /* = different locks count */;
            var runCycle = threadsCount / differentLocksCount /* = Run Cycle*/;
            var hopedTotalTime = runCycle * (sleepMillionSeconds + 100) /* = Hoped Total Time */;
            var retryDelay = 20; /* = retryDelayCycle MillionSeconds */
            var randomRetryDelay = (retryDelay / 2) /*random retry delay*/;
            var retryTimes = 50;// hopedTotalTime / randomRetryDelay; /* = maximum retry times */;//This number can be adjusted (gradually reduced based on test results, failures can be observed)

            Console.WriteLine("sleepMillionSeconds:{0}ms", sleepMillionSeconds);
            Console.WriteLine("differentLocksCount:{0}", differentLocksCount);
            Console.WriteLine("runCycle:{0}", runCycle);
            Console.WriteLine("hopedTotalTime:{0}ms", hopedTotalTime);
            Console.WriteLine("randomRetryDelay:{0}ms", randomRetryDelay);
            Console.WriteLine("retryTimes:{0}", retryTimes);
            Console.WriteLine("randomLockTime（可能的Lock超时时间）:{0}ms", randomRetryDelay * retryTimes);
            Console.WriteLine("=====================");

            List<Thread> list = new List<Thread>();
            int[] runThreads = { 0 };
            var dt0 = SystemTime.Today;
            for (int i = 0; i < (int)threadsCount; i++)
            {
                runThreads[0]++;
                var i1 = i;
                var thread = new Thread(async () =>
                {
                    var appId = (i1 % 2).ToString();
                    var resourceName = "Test-" + rnd.Next(0, 2);//Adjust the random number here to change the number of locks
                    var cache = CacheStrategyFactory.GetObjectCacheStrategyInstance();//Reacquire the instance each time (since it's a singleton, it's actually the same one)

                    Console.WriteLine($"Thread {i1} / {resourceName} : {appId} 进入，准备尝试锁。Cache实例：{cache.GetHashCode()}");

                    var dt1 = SystemTime.Now;
                    using (var cacheLock = await cache.BeginCacheLockAsync(resourceName, appId, (int)retryTimes, TimeSpan.FromMilliseconds(retryDelay)))
                    {
                        var result = cacheLock.LockSuccessful
                            ? "Success"
                            : "Failed";
                        result += " | TTL：" + cacheLock.GetTotalTtl(retryTimes, TimeSpan.FromMilliseconds(retryDelay)) + "ms";

                        Console.WriteLine($"Thread {i1} / {resourceName} : {appId} 进入锁，等待时间：{SystemTime.DiffTotalMS(dt1)} / {SystemTime.DiffTotalMS(dt1)}ms，获得锁结果：{result}");

                        Thread.Sleep(sleepMillionSeconds);
                    }
                    Console.WriteLine($"Thread {i1} / {resourceName} : {appId} 释放锁（{SystemTime.DiffTotalMS(dt1)}ms）");
                    runThreads[0]--;
                });
                list.Add(thread);
            }


            var dtAll1 = SystemTime.Now;

            list.ForEach(z => z.Start());

            while (runThreads[0] > 0)
            {
                Thread.Sleep(10);
            }

            Console.WriteLine("Real Time:{0}ms", SystemTime.DiffTotalMS(dtAll1));
        }
    }
}
