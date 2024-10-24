using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Senparc.CO2NET.Extensions;
using Senparc.CO2NET.RegisterServices;

namespace Senparc.CO2NET.Tests.RegisterServices
{
    [TestClass]
    public class RegisterServiceExtensionTests
    {
        [TestMethod]
        public void RegisterServiceExtensionTest()
        {
            var serviceCollection = new ServiceCollection();
            var configBuilder = new ConfigurationBuilder();
            var config = configBuilder.Build();
            serviceCollection.AddSenparcGlobalServices(config);

            Assert.IsNotNull(SenparcDI.GlobalServiceCollection);
            Assert.AreEqual(serviceCollection.GetHashCode(), SenparcDI.GlobalServiceCollection.GetHashCode());


            var b1 = serviceCollection.BuildServiceProvider();
            var b2 = serviceCollection.BuildServiceProvider();

            // Synchronize
            Console.WriteLine(b1.GetHashCode());
            Console.WriteLine(b2.GetHashCode());


            //TODO: Implement the logic to obtain configuration elements
            //var senparcSetting = serviceCollection
            //                        .BuildServiceProvider().GetService<IOptions<SenparcSetting>>();
            //Console.WriteLine(senparcSetting.ToJson());
            //Assert.IsNotNull(senparcSetting);
            //Assert.IsTrue(senparcSetting.Value.IsDebug);
            //Assert.AreEqual("DefaultCacheTest", senparcSetting.Value.DefaultCacheNamespace);
        }
    }
}
