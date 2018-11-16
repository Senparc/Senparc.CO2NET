using Microsoft.VisualStudio.TestTools.UnitTesting;
using Senparc.CO2NET.Tests;
using System;

namespace Senparc.CO2NET.APM.Tests
{
    [TestClass]
    public class DataOperationTests : BaseTest
    {
        const string domain = "CO2NET_Test";
        DataOperation dataOperation = new DataOperation(domain);


        [TestMethod]
        public void SetAndGetTest()
        {
            dataOperation.Set("CPU", .65, dateTime: SystemTime.Now);
            dataOperation.Set("CPU", .78, dateTime: SystemTime.Now);
            dataOperation.Set("CPU", .75, dateTime: SystemTime.Now);
            dataOperation.Set("CPU", .92, dateTime: SystemTime.Now.AddMinutes(1));
            dataOperation.Set("CPU", .48, dateTime: SystemTime.Now.AddMinutes(2));

            dataOperation.Set("访问量", 1, dateTime: SystemTime.Now);
            dataOperation.Set("访问量", 1, dateTime: SystemTime.Now);
            dataOperation.Set("访问量", 1, dateTime: SystemTime.Now);
            dataOperation.Set("访问量", 1, dateTime: SystemTime.Now.AddMinutes(1));
            dataOperation.Set("访问量", 1, dateTime: SystemTime.Now.AddMinutes(2));
            dataOperation.Set("访问量", 1, dateTime: SystemTime.Now.AddMinutes(2));

            var cpuData = dataOperation.GetDataItemList("CPU");
            Assert.AreEqual(5, cpuData?.Count);

            var viewData = dataOperation.GetDataItemList("访问量");
            Assert.AreEqual(6, viewData?.Count);
        }

        [TestMethod]
        public void IsLaterMinuteTest()
        {
            {
                var dateTime1 = new DateTime(2018, 11, 16, 15, 08, 20);
                var dateTime2 = new DateTime(2018, 11, 16, 15, 08, 59);
                Assert.IsFalse(dataOperation.IsLaterMinute(dateTime1, dateTime2));
            }

            {
                var dateTime1 = new DateTime(2018, 11, 16, 15, 08, 20);
                var dateTime2 = new DateTime(2018, 11, 16, 15, 20, 01);
                Assert.IsTrue(dataOperation.IsLaterMinute(dateTime1, dateTime2));
            }

            {
                var dateTime1 = new DateTime(2018, 11, 16, 23, 59, 59);
                var dateTime2 = new DateTime(2018, 11, 17, 00, 00, 00);
                Assert.IsTrue(dataOperation.IsLaterMinute(dateTime1, dateTime2));
            }
        }
    }
}
