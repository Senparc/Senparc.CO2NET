using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Senparc.CO2NET.RegisterServices;

namespace Senparc.CO2NET.Tests
{
    //[TestClass]
    public class BaseTest
    {
        public BaseTest()
        {
            //×¢²á
            var mockEnv = new Mock<IHostingEnvironment>();
            mockEnv.Setup(z => z.ContentRootPath).Returns(() => UnitTestHelper.RootPath);
            RegisterService.Start(mockEnv.Object, new SenparcSetting() { IsDebug = true });


            var serviceCollection = new ServiceCollection();
            var configBuilder = new ConfigurationBuilder();
            var config = configBuilder.Build();
            serviceCollection.AddSenparcGlobalServices(config);
            serviceCollection.AddMemoryCache();//Ê¹ÓÃÄÚ´æ»º´æ
        }
    }
}
