using Microsoft.VisualStudio.TestTools.UnitTesting;
using Senparc.CO2NET.Helpers;
using System;
using System.Collections.Generic;
using System.Text;

namespace Senparc.CO2NET.Tests.Helpers
{
    [TestClass]
    public class DateTimeHelperTests
    {
        [TestMethod]
        public void GetDateTimeFromXmlTest()
        {
            {
                var timeTicks = "1545888011";
                var result = DateTimeHelper.GetDateTimeFromXml(timeTicks);
                var timeStr = "12/27/2018 13:20:11";// Result when printed directly
                Console.WriteLine(result);

                var timeStrToString = "2018/12/27 13:20:11";// Result differs after ToString()
                Console.WriteLine(result.ToString("yyyy/MM/dd HH:mm:ss"));
                Assert.AreEqual(timeStrToString, result.ToString("yyyy/MM/dd HH:mm:ss"));
            }
        }

        [TestMethod]
        public void GetDateTimeOffsetFromXmlTest()
        {
            {
                var timeTicks = "1545888011";
                var result = DateTimeHelper.GetDateTimeOffsetFromXml(timeTicks);
                var timeStr = "12/27/2018 13:20:11 +08:00";// Result when printed directly
                Console.WriteLine(result);
                Console.WriteLine(result + "");
                Console.WriteLine(result.ToString());
                Console.WriteLine(result.Date.ToString());
                Console.WriteLine(result.DateTime.ToString());
                Console.WriteLine(result.LocalDateTime.ToString());
                var timeStrTostring = "2018/12/27 13:20:11 +08:00";// Without ToString(), the returned result is inconsistent
                Assert.AreEqual(timeStrTostring, result.ToString("yyyy/MM/dd HH:mm:ss zzz"));

                Console.WriteLine("==============");
                var localTimeStr = "2018/12/27 13:20:11";
                Console.WriteLine(result.LocalDateTime);
                Assert.AreEqual(localTimeStr, result.LocalDateTime.ToString("yyyy/MM/dd HH:mm:ss"));
                Console.WriteLine(result.DateTime);
                Assert.AreEqual(localTimeStr, result.DateTime.ToString("yyyy/MM/dd HH:mm:ss"));
            }
        }

        [TestMethod]
        public void GetDateTimeOffsetFromXml_ShouldReturnCorrectDateTime()
        {
            // Arrange  
            long unixTimeStamp = 1545888011; // Unix timestamp
            DateTimeOffset expectedDateTime = new DateTimeOffset(2018, 12, 27, 13, 20, 11, TimeSpan.FromHours(8)); // Expected China Standard Time

            // Act  
            DateTimeOffset actualDateTime = DateTimeHelper.GetDateTimeOffsetFromXml(unixTimeStamp);

            // Assert  
            Assert.AreEqual(expectedDateTime, actualDateTime);
        }

        [TestMethod]
        public void GetDateTimeOffsetFromXml_ShouldHandleEpochStart()
        {
            // Arrange  
            long unixTimeStamp = 0;
            DateTimeOffset expectedDateTime = new DateTimeOffset(1970, 1, 1, 8, 0, 0, TimeSpan.FromHours(8)); // +8 time zone

            // Act  
            DateTimeOffset actualDateTime = DateTimeHelper.GetDateTimeOffsetFromXml(unixTimeStamp);

            // Assert  
            Assert.AreEqual(expectedDateTime, actualDateTime);
        }
    }

}
