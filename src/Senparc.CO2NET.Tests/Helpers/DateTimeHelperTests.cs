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
                Console.WriteLine(result.ToString());
                Assert.AreEqual(timeStrToString, result.ToString());
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
                Console.WriteLine(result+"");
                Console.WriteLine(result.ToString());
                Console.WriteLine(result.Date.ToString());
                Console.WriteLine(result.DateTime.ToString());
                Console.WriteLine(result.LocalDateTime.ToString());
                var timeStrTostring = "2018/12/27 13:20:11 +08:00";//不是用ToString()返回结果不一致
                Assert.AreEqual(timeStrTostring, result.ToString());

                Console.WriteLine("==============");
                var localTimeStr = "2018/12/27 13:20:11";
                Console.WriteLine(result.LocalDateTime) ;
                Assert.AreEqual(localTimeStr, result.LocalDateTime.ToString());
                Console.WriteLine(result.DateTime);
                Assert.AreEqual(localTimeStr, result.DateTime.ToString());



                //Assert.AreEqual(timeStr, result);

            }
        }
    }

}
