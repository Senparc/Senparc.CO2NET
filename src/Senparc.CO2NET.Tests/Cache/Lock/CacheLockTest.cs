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
        /// 测试 Parallel 和 Thread 的并行数量
        /// </summary>
        [TestMethod]
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
            //测试结果：同时运行的线程数约为4（平均3.6）,实际目测为5

            Console.WriteLine("测试Threads");

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
            //测试结果：无限制
        }

        [TestMethod]
        public void LockTest()
        {
            /* 测试逻辑：
             * 20个异步线程同时进行，
             * 线程内有两组不同的ResourceName： "Test-0"，"Test-1"，模拟AccessTokenContainer和JsTicketContainer），
             * 每组ResourceName下面有两组分别为0和1的appId。
             * 同时运行这些线程，观察锁是否生效。
             * 失败的现象：1、同一个ResourceName、同一个appId下有两个线程同时获得锁
             *             2、不同的ResourceName、appId之间任意组合相互干扰；
             *             3、出现死锁；
             *             4、某线程始终无法获得锁（超时或一直运行）
             *             
             * 注意：超时导致的失败可能是由于设置的最大等待时间相对于测试的Thread.Sleep(sleepMillionSeconds)太短。
             */

            Console.WriteLine("同步方法测试");

            bool useRedis = true;

            if (useRedis)
            {
                var redisConfiguration = "localhost:6379";
                RedisManager.ConfigurationOption = redisConfiguration;
                CacheStrategyFactory.RegisterObjectCacheStrategy(() => RedisObjectCacheStrategy.Instance);//Redis
            }

            Console.WriteLine($"使用缓存策略：{CacheStrategyFactory.GetObjectCacheStrategyInstance()}");

            Random rnd = new Random();
            var threadsCount = 20M;
            int sleepMillionSeconds = 200;//数字越大，每个线程占用时间越长，直至超时

            //初步估计需要重试时间
            var differentLocksCount = (2 /*appId*/* 2 /*resource*/) /* = diffrent locks count */;
            var runCycle = threadsCount / differentLocksCount /* = Run Cycle*/;
            var hopedTotalTime = runCycle * (sleepMillionSeconds + 100) /* = Hoped Total Time */;
            var retryDelay = 20; /* = retryDelayCycle MillionSeconds */
            var randomRetryDelay = (retryDelay / 2) /*random retry delay*/;
            var retryTimes = 50;// hopedTotalTime / randomRetryDelay; /* = maxium retry times */;//此数字可以调整（根据测试结果逐步缩小，可以看到会有失败的锁产生）

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
                var thread = new Thread(() =>
                {
                    var appId = (i1 % 2).ToString();
                    var resourceName = "Test-" + rnd.Next(0, 2);//调整这里的随机数，可以改变锁的个数
                    var cache = CacheStrategyFactory.GetObjectCacheStrategyInstance();//每次重新获取实例（因为单例模式，所以其实是同一个）

                    Console.WriteLine($"线程 {i1} / {resourceName} : {appId} 进入，准备尝试锁。Cache实例：{cache.GetHashCode()}");

                    var dt1 = SystemTime.Now;
                    using (var cacheLock = cache.BeginCacheLock(resourceName, appId, (int)retryTimes, TimeSpan.FromMilliseconds(retryDelay)))
                    {
                        var result = cacheLock.LockSuccessful
                            ? "成功"
                            : "【失败！】";
                        result += " | TTL：" + cacheLock.GetTotalTtl(retryTimes, TimeSpan.FromMilliseconds(retryDelay)) + "ms";

                        Console.WriteLine($"线程 {i1} / {resourceName} : {appId} 进入锁，等待时间：{SystemTime.DiffTotalMS(dt1)} / {SystemTime.DiffTotalMS(dt0)}ms，获得锁结果：{result}");

                        Thread.Sleep(sleepMillionSeconds);
                    }
                    Console.WriteLine($"线程 {i1} / {resourceName} : {appId} 释放锁（{SystemTime.DiffTotalMS(dt1)}ms）");
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

            //等待
            //Parallel.For(0, (int)threadsCount, i =>
            //{
            //    var appId = (i % 2).ToString();
            //    var resourceName = "Test-" + rnd.Next(0, 2);
            //    Console.WriteLine("线程 {0} / {1} : {2} 进入，准备尝试锁", Thread.CurrentThread.GetHashCode(), resourceName, appId);

            //    DateTime dt1 = SystemTime.Now;
            //    using (var cacheLock = Cache.BeginCacheLock(resourceName, appId, (int)retryTimes, new TimeSpan(0, 0, 0, 0, 20)))
            //    {
            //        var result = cacheLock.LockSuccessful
            //            ? "成功"
            //            : "【失败！】";

            //        Console.WriteLine("线程 {0} / {1} : {2} 进入锁，等待时间：{3}ms，获得锁结果：{4}", Thread.CurrentThread.GetHashCode(), resourceName, appId, SystemTime.DiffTotalMS(dt1), result);

            //        Thread.Sleep(sleepMillionSeconds);
            //    }
            //    Console.WriteLine("线程 {0} / {1} : {2} 释放锁（{3}ms）", Thread.CurrentThread.GetHashCode(), resourceName, appId, SystemTime.DiffTotalMS(dt1));
            //});
        }

        [TestMethod]
        public void LockAsyncTest()
        {
            /* 测试逻辑：
             * 20个异步线程同时进行，
             * 线程内有两组不同的ResourceName： "Test-0"，"Test-1"，模拟AccessTokenContainer和JsTicketContainer），
             * 每组ResourceName下面有两组分别为0和1的appId。
             * 同时运行这些线程，观察锁是否生效。
             * 失败的现象：1、同一个ResourceName、同一个appId下有两个线程同时获得锁
             *             2、不同的ResourceName、appId之间任意组合相互干扰；
             *             3、出现死锁；
             *             4、某线程始终无法获得锁（超时或一直运行）
             *             
             * 注意：超时导致的失败可能是由于设置的最大等待时间相对于测试的Thread.Sleep(sleepMillionSeconds)太短。
             */

            Console.WriteLine("异步方法测试");

            bool useRedis = true;

            if (useRedis)
            {
                var redisConfiguration = "localhost:6379";
                RedisManager.ConfigurationOption = redisConfiguration;
                CacheStrategyFactory.RegisterObjectCacheStrategy(() => RedisObjectCacheStrategy.Instance);//Redis
            }

            Console.WriteLine($"使用缓存策略：{CacheStrategyFactory.GetObjectCacheStrategyInstance()}");

            Random rnd = new Random();
            var threadsCount = 20M;
            int sleepMillionSeconds = 200;//数字越大，每个线程占用时间越长，直至超时

            //初步估计需要重试时间
            var differentLocksCount = (2 /*appId*/* 2 /*resource*/) /* = diffrent locks count */;
            var runCycle = threadsCount / differentLocksCount /* = Run Cycle*/;
            var hopedTotalTime = runCycle * (sleepMillionSeconds + 100) /* = Hoped Total Time */;
            var retryDelay = 20; /* = retryDelayCycle MillionSeconds */
            var randomRetryDelay = (retryDelay / 2) /*random retry delay*/;
            var retryTimes = 50;// hopedTotalTime / randomRetryDelay; /* = maxium retry times */;//此数字可以调整（根据测试结果逐步缩小，可以看到会有失败的锁产生）

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
                    var resourceName = "Test-" + rnd.Next(0, 2);//调整这里的随机数，可以改变锁的个数
                    var cache = CacheStrategyFactory.GetObjectCacheStrategyInstance();//每次重新获取实例（因为单例模式，所以其实是同一个）

                    Console.WriteLine($"线程 {i1} / {resourceName} : {appId} 进入，准备尝试锁。Cache实例：{cache.GetHashCode()}");

                    var dt1 = SystemTime.Now;
                    using (var cacheLock = await cache.BeginCacheLockAsync(resourceName, appId, (int)retryTimes, TimeSpan.FromMilliseconds(retryDelay)))
                    {
                        var result = cacheLock.LockSuccessful
                            ? "成功"
                            : "【失败！】";
                        result += " | TTL：" + cacheLock.GetTotalTtl(retryTimes, TimeSpan.FromMilliseconds(retryDelay)) + "ms";

                        Console.WriteLine($"线程 {i1} / {resourceName} : {appId} 进入锁，等待时间：{SystemTime.DiffTotalMS(dt1)} / {SystemTime.DiffTotalMS(dt1)}ms，获得锁结果：{result}");

                        Thread.Sleep(sleepMillionSeconds);
                    }
                    Console.WriteLine($"线程 {i1} / {resourceName} : {appId} 释放锁（{SystemTime.DiffTotalMS(dt1)}ms）");
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
