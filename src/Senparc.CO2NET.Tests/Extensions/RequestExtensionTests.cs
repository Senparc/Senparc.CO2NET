using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;

namespace Senparc.CO2NET.Tests.Extensions
{ 
    [TestClass]
    public class RequestExtensionTests
    {

        [TestMethod]
        public void AbsoluteUriTest()
        {
            var mockRequest = new Mock<HttpRequest>();

            mockRequest.Setup(r => r.PathBase).Returns("");
            mockRequest.Setup(r => r.Path).Returns("/Home/Index/");

            var q = new QueryString();
            q.Add("a", "1");
            q.Add("b", "2");
            q.Add("c", "3");

            var qc = new QueryCollection(new Dictionary<string, StringValues>(
                new KeyValuePair<string, StringValues>[] {
                     new KeyValuePair<string,StringValues>("d","4"),
                     new KeyValuePair<string,StringValues>("e","5"),
                     new KeyValuePair<string,StringValues>("f","6")
            }));

            mockRequest.Setup(r => r.Query).Returns(qc);
            //mockRequest.Setup(r => r.QueryString).Returns(q);

            Console.WriteLine(mockRequest.Object.QueryString);
            mockRequest.Object.QueryString.Add("g", "7");
            Console.WriteLine(mockRequest.Object.QueryString);//测试中无法输出QueryString，真实环境可以

            mockRequest.Setup(r => r.Scheme).Returns("http");
            mockRequest.Setup(r => r.Host).Returns(new HostString("www.senparc.com", 80));
            var result = mockRequest.Object.AbsoluteUri();
            Console.WriteLine(result);
            Assert.AreEqual("http://www.senparc.com/Home/Index/", result);

            mockRequest.Setup(r => r.Scheme).Returns("https");
            mockRequest.Setup(r => r.Host).Returns(new HostString("www.senparc.com", 443));
            result = mockRequest.Object.AbsoluteUri();
            Assert.AreEqual("https://www.senparc.com/Home/Index/", result);

            mockRequest.Setup(r => r.Host).Returns(new HostString("www.senparc.com", 1443));
            result = mockRequest.Object.AbsoluteUri();
            Assert.AreEqual("https://www.senparc.com:1443/Home/Index/", result);

        }
    }
}
