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

            var threadCount = 60;//定义线程数量
            var runningCount = 0;
            var finishThreadCount = 0;//已完成线程数量
            var retryCount = 10;//重试次数（Redis 默认10）
            var retryDelay = TimeSpan.FromMilliseconds(100);//单次重试等待时间（Redis 默认10毫秒）
            Dictionary<int, Thread> threadCollection = new Dictionary<int, Thread>();//线程集合

            var dtStart = SystemTime.Now;
            var startRun = false;
            //配置所有线程（为了尽量同时开始，忽略设置时间，所以先设置，再依次快速开启）
            for (int i = 0; i < threadCount; i++)
            {
                var lockGroupKey = i % 4;//分为4组
                var threadIndex = i;
                threadCollection[i] = new Thread(async () =>
                {
                    runningCount++;
                    //while (runningCount < threadCount)
                    {
                        //等待统一开始
                    }
                    var dt1 = SystemTime.Now;
                    using (var syncLock =cache.BeginCacheLock(lockResourceName, lockGroupKey.ToString(), retryCount, retryDelay))
                    {
                        var runTime = SystemTime.Now;
                        var waitTime = (runTime - dt1).TotalMilliseconds;
                        Thread.Sleep(100);//模拟线程处理时间
                        finishThreadCount++;
                        Console.WriteLine($"{runTime.ToString("ss.ffffff")} - {SystemTime.Now.ToString("ss.ffffff")}\t\t 等待：{waitTime}ms \t\t获取syncLock完成（Group：{lockGroupKey}）：{threadIndex} - {syncLock.LockSuccessful}");
                    }
                });
                threadCollection[i].Name = $"Lock-{i}";
            }

            //开始所有线程

            //此方法会导致死循环
            //threadCollection.Values.ToList().AsParallel().ForAll(thread => thread.Start());

            foreach (var thread in threadCollection.Values)
            {
                thread.Start();//注意：线程实际启动完成时间不一定是按序的
            }

            while (finishThreadCount < threadCount)
            {
                //等待完成
            }

            Console.WriteLine($"过程结束，总用时：{SystemTime.DiffTotalMS(dtStart)} ms");
        }
    }
}
