using System;
using Microsoft.AspNetCore.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Senparc.CO2NET.Utilities;

namespace Senparc.CO2NET.Tests.Utilities
{
    [TestClass]
    public class BrowserUtilityTests
    {
        [TestMethod]
        public void BrowserUtilityTest()
        {
            var userAgent = "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/67.0.3396.87 Safari/537.36";
            var mockHttpRequest = new Mock<HttpRequest>();
            mockHttpRequest.Setup(z => z.Headers["User-Agent"])
                .Returns(userAgent);

            var result = BrowserUtility.GetUserAgent(mockHttpRequest.Object);
            Console.WriteLine(result);

            Assert.AreEqual(userAgent, result, true);
        }
    }
}
