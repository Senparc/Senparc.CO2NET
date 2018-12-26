using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
using Senparc.CO2NET.Extensions;

namespace Senparc.CO2NET.Tests.ExtensionEntities
{
    public class DateTimeObj
    {
        public DateTimeOffset Date { get; set; }
    }

    [TestClass]
    public class SystemTimeTests
    {
        [TestMethod]
        public void NowTest()
        {
            //Console.WriteLine(SystemTime.Now.ToString("K"));
            Console.WriteLine(SystemTime.Now.ToString("r"));
            Console.WriteLine(SystemTime.Now.ToString("u"));
            Console.WriteLine(SystemTime.Now.ToString("o"));
            Console.WriteLine(SystemTime.Now.ToString("yyyyMMdd:HHmmss"));

            Console.WriteLine(new { date = SystemTime.Now }.ToJson());
            Console.WriteLine(new { date = DateTime.Now }.ToJson());

            var dateTimeStr = "{\"Date\":\"2018-12-26T18:49:10.4357113+08:00\"}";
            var obj = Senparc.CO2NET.Helpers.SerializerHelper.GetObject<DateTimeObj>(dateTimeStr);
            Console.WriteLine(obj.Date);
        }

    }
}
