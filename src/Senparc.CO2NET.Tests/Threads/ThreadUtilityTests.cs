using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Senparc.CO2NET.MessageQueue;
using Senparc.CO2NET.Threads;

namespace Senparc.CO2NET.Tests.Threads
{
    [TestClass]
    public class ThreadUtilityTests
    {
        [TestMethod]
        public void ThreadUtilityTest()
        {
            ThreadUtility.Register();
            ThreadUtility.Register();//多次注仍然只记录一次（最早的一次）
            ThreadUtility.Register();

            Assert.AreEqual(1, ThreadUtility.AsynThreadCollection.Count);

            var smq = new SenparcMessageQueue();
            var key = "ThreadUtilityTests";
            smq.Add(key, () =>
            {
                Console.WriteLine("队列执行SenparcMessageQueue");
            });

            //不再需要操作 SenparcMessageQueueThreadUtility.Run()，队列已经会自动处理

            while (smq.GetCount() > 0)
            {
                //执行队列
            }

            Console.WriteLine($"SenparcMessageQueue队列处理完毕，当前项目：{smq.GetCount()}");
        }
    }
}
