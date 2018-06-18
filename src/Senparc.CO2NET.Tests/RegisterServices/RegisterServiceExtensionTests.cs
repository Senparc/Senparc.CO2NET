using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Senparc.CO2NET.RegisterServices;

namespace Senparc.CO2NET.Tests.RegisterServices
{
    [TestClass]
    public class RegisterServiceExtensionTests
    {
        [TestMethod]
        public void RegisterServiceExtensionTest()
        {
            var mockService = new Mock<IServiceCollection>();
            mockService.Setup(z => z.Count).Returns(666);

            mockService.Object.AddSenparcGlobalServices();

            Assert.IsNotNull(RegisterService.GlobalServiceCollection);
            Assert.AreEqual(mockService.GetHashCode(), RegisterService.GlobalServiceCollection.GetHashCode());
            Assert.AreEqual(666,RegisterService.GlobalServiceCollection.Count);
        }
    }
}
