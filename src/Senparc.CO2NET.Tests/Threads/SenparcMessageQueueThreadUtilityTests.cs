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
                Console.WriteLine("Execute SenparcMessageQueue");
            });

            Console.WriteLine($"SenparcMessageQueue.Count: {smq.GetCount()}");

            var senparcMessageQueue = new SenparcMessageQueueThreadUtility();

            Task.Factory.StartNew(() =>
            {
                senparcMessageQueue.Run();
            });// Asynchronous execution

            //
            while (smq.GetCount() > 0)
            {
                // Execution details
            }

            Console.WriteLine($"SenparcMessageQueue processing completed, current count: {smq.GetCount()}");
        }
    }
}
