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
                var timeStr = "12/27/2018 13:20:11";//直接打印出来的结果
                Console.WriteLine(result);

                var timeStrToString = "2018/12/27 13:20:11";//ToString 之后结果会不一样
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
                var timeStr = "12/27/2018 13:20:11 +08:00";//直接打印出来的结果
                Console.WriteLine(result);
                Console.WriteLine(result + "");
                Console.WriteLine(result.ToString());
                Console.WriteLine(result.Date.ToString());
                Console.WriteLine(result.DateTime.ToString());
                Console.WriteLine(result.LocalDateTime.ToString());
                var timeStrTostring = "2018/12/27 13:20:11 +08:00";//不是用ToString()返回结果不一致
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
            long unixTimeStamp = 1545888011; // Unix 时间戳  
            DateTimeOffset expectedDateTime = new DateTimeOffset(2018, 12, 27, 13, 20, 11, TimeSpan.FromHours(8)); // 预期的中国标准时间  

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
            DateTimeOffset expectedDateTime = new DateTimeOffset(1970, 1, 1, 8, 0, 0, TimeSpan.FromHours(8)); // +8时区  

            // Act  
            DateTimeOffset actualDateTime = DateTimeHelper.GetDateTimeOffsetFromXml(unixTimeStamp);

            // Assert  
            Assert.AreEqual(expectedDateTime, actualDateTime);
        }

        [TestMethod]
        public void GetDateTimeOffsetFromXml_ShouldReturnCorrectDateTime_UTC()
        {
            // Arrange  
            long unixTimeStamp = 1545888011; // Unix 时间戳  
            DateTimeOffset expectedDateTime = new DateTimeOffset(2018, 12, 27, 5, 20, 11, TimeSpan.Zero); // 预期的 UTC 时间  

            // Act  
            DateTimeOffset actualDateTime = DateTimeHelper.GetDateTimeOffsetFromXml(unixTimeStamp, "UTC");

            // Assert  
            Assert.AreEqual(expectedDateTime, actualDateTime);
        }

        [TestMethod]
        public void GetDateTimeOffsetFromXml_ShouldReturnCorrectDateTime_EasternStandardTime()
        {
            // Arrange  
            long unixTimeStamp = 1545888011; // Unix 时间戳  
            DateTimeOffset expectedDateTime = new DateTimeOffset(2018, 12, 27, 0, 20, 11, TimeSpan.FromHours(-5)); // 预期的美国东部标准时间  

            // Act  
            DateTimeOffset actualDateTime = DateTimeHelper.GetDateTimeOffsetFromXml(unixTimeStamp, "Eastern Standard Time");

            // Assert  
            Assert.AreEqual(expectedDateTime, actualDateTime);
        }

        [TestMethod]
        public void GetDateTimeOffsetFromXml_ShouldReturnCorrectDateTime_PacificStandardTime()
        {
            // Arrange  
            long unixTimeStamp = 1545888011; // Unix 时间戳  
            DateTimeOffset expectedDateTime = new DateTimeOffset(2018, 12, 26, 21, 20, 11, TimeSpan.FromHours(-8)); // 预期的美国太平洋标准时间  

            // Act  
            DateTimeOffset actualDateTime = DateTimeHelper.GetDateTimeOffsetFromXml(unixTimeStamp, "Pacific Standard Time");

            // Assert  
            Assert.AreEqual(expectedDateTime, actualDateTime);
        }
    }

}
