using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Senparc.CO2NET.WebProxy;

namespace Senparc.CO2NET.Tests
{
    [TestClass]
    public class CoreWebProxyTests
    {
        [TestMethod]
        public void CoreWebProxyTest()
        {
            var mockCred = new Mock<ICredentials>();

            CoreWebProxy proxy = new CoreWebProxy(new Uri("http://www.senparc.com"), mockCred.Object
                , new[] { "https://weixin.senparc.com", "https://sdk.weixin.senparc.com" });

            Assert.AreEqual(mockCred.Object, proxy.Credentials);
        }
    }
}
