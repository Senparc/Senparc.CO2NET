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
                    smq.Add("测试" + Guid.NewGuid().ToString(), () =>
                    {
                        Console.WriteLine("执行队列：" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffffff"));
                    });
                }
            }//立即执行所有队列

            //已经执行完
            Assert.AreEqual(0, smq.GetCount());
        }
    }
}
