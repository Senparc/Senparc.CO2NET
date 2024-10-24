using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Senparc.CO2NET.CacheUtility;
using Senparc.CO2NET.MessageQueue;

namespace Senparc.CO2NET.Tests.Utilities
{
    [TestClass]
    public class FlushCacheTests
    {
        [TestMethod]
        public void FlushCacheTest()
        {
            var smq = new SenparcMessageQueue();
            using (var flushCache = new FlushCache())
            {
                for (int i = 0; i < 10; i++)
                {
                    smq.Add("Test" + Guid.NewGuid().ToString(), () =>
                    {
                        Console.WriteLine("Execute Queue:" + SystemTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffffff"));
                    });
                }
            }// Check if the command has been executed

            // Already executed
            Assert.AreEqual(0, smq.GetCount());
        }
    }
}
