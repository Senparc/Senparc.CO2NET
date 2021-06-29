using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Senparc.CO2NET.WebApi.Tests
{
    [TestClass]
   public class WebApiEngineTests:BaseTest
    {
        [ApiBind("CO2NET", "WebApiEngineTests.TestApi")]
        public string TestApi(string name, int value)
        {
            return $"{name}:{value}";
        }


        [TestMethod]
        public void InitDynamicApiTest()
        {
            Init();
            var findWeixinApiService = ServiceProvider.GetService<FindWeixinApiService>();
            WebApiEngine wae = new WebApiEngine(findWeixinApiService, 400);
            wae.InitDynamicApi(MvcCoreBuilder, Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "App_Data"));
        }
    }
}
