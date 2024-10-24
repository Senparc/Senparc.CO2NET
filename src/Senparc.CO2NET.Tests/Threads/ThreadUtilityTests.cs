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
            ThreadUtility.Register();// This comment is only recorded once, not multiple times
            ThreadUtility.Register();

            Assert.AreEqual(1, ThreadUtility.AsynThreadCollection.Count);

            var smq = new SenparcMessageQueue();
            var key = "ThreadUtilityTests";
            smq.Add(key, () =>
            {
                Console.WriteLine("Executing SenparcMessageQueue");
            });
            // No need to operate SenparcMessageQueueThreadUtility.Run(), the queue will handle automatically  
            while (smq.GetCount() > 0)
            {
                // Execute the queue  
            }
            Console.WriteLine($"SenparcMessageQueue processing completed, current count: {smq.GetCount()}");
        }
    }
}
