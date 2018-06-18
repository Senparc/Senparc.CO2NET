using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Senparc.CO2NET.MessageQueue;

namespace Senparc.CO2NET.Tests.MessageQueue
{
    [TestClass]
    public class SenparcMessageQueueItemTests
    {
        [TestMethod]
        public void SenparcMessageQueueItemTest()
        {
            var actionRun = 0;
            var item = new SenparcMessageQueueItem("Key", () => {
                actionRun = 1;
             }, "desc");

            Assert.AreEqual("Key", item.Key);
            item.Action();//actionRun = 1
            Assert.AreEqual(1, actionRun);
            Assert.AreEqual("desc", item.Description);
        }
    }
}
