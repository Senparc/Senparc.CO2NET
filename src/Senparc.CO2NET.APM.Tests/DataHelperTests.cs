using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;

namespace Senparc.CO2NET.APM.Tests
{
    [TestClass]
    public class DataHelperTests
    {

        [TestMethod]
        public void IsLaterMinuteTest()
        {
            {
                var dateTime1 = new DateTime(2018, 11, 16, 15, 08, 20);
                var dateTime2 = new DateTime(2018, 11, 16, 15, 08, 59);
                Assert.IsFalse(DataHelper.IsLaterMinute(dateTime1, dateTime2));
            }

            {
                var dateTime1 = new DateTime(2018, 11, 16, 15, 08, 20);
                var dateTime2 = new DateTime(2018, 11, 16, 15, 20, 01);
                Assert.IsTrue(DataHelper.IsLaterMinute(dateTime1, dateTime2));
            }

            {
                var dateTime1 = new DateTime(2018, 11, 16, 23, 59, 59);
                var dateTime2 = new DateTime(2018, 11, 17, 00, 00, 00);
                Assert.IsTrue(DataHelper.IsLaterMinute(dateTime1, dateTime2));
            }
        }

        [TestMethod]
        public void GetCPUCounterTest()
        {
            var cpuCounter = DataHelper.GetCPUCounter();
            Console.WriteLine(cpuCounter);
        }
    }
}
