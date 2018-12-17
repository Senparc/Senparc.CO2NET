using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
using Senparc.CO2NET.Extensions;

namespace Senparc.CO2NET.Tests.Extensions
{
    [TestClass]
    public class WebCodingExtensionsTests
    {
        [TestMethod]
        public void UrlEecodeTest()
        {
            var query = "https://weixin.senparc.com/?a=1&b=2&c=<a>";
            var excepted = "https%3A%2F%2Fweixin.senparc.com%2F%3Fa%3D1%26b%3D2%26c%3D%3Ca%3E";
            var result = query.UrlEncode();
            Console.WriteLine(result);
            Assert.AreEqual(excepted, result);
        }

        [TestMethod]
        public void UrlDecodeTest()
        {
            var query  = "https%3A%2F%2Fweixin.senparc.com%2F%3Fa%3D1%26b%3D2%26c%3D%3Ca%3E";
            var excepted = "https://weixin.senparc.com/?a=1&b=2&c=<a>";
            var result = query.UrlDecode();
            Console.WriteLine(result);
            Assert.AreEqual(excepted, result);
        }
    }
}
