using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Senparc.CO2NET.MessageQueue;
using Senparc.CO2NET.Threads;

namespace Senparc.CO2NET.Tests.Threads
{
    [TestClass]
    public class SenparcMessageQueueThreadUtilityTests
    {
        [TestMethod]
        public void SenparcMessageQueueThreadUtilityTest()
        {
            var smq = new SenparcMessageQueue();
            var key = "SenparcMessageQueueThreadUtilityTest";
            smq.Add(key, () =>
            {
                Console.WriteLine("执行SenparcMessageQueue");
            });

            Console.WriteLine($"SenparcMessageQueue.Count：{smq.GetCount()}");

            var senparcMessageQueue = new SenparcMessageQueueThreadUtility();

            Task.Factory.StartNew(() =>
            {
                senparcMessageQueue.Run();
            });//异步执行

            //
            while (smq.GetCount() > 0)
            {
                //执行队列
            }

            Console.WriteLine($"SenparcMessageQueue队列处理完毕，当前项目：{smq.GetCount()}");

        }
    }
}
