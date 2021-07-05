using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Senparc.CO2NET.WebApi.WebApiEngines;

namespace Senparc.CO2NET.WebApi.Tests
{
    [TestClass]
    public class WebApiEngineTests : BaseTest
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
            var findWeixinApiService = ServiceProvider.GetService<FindApiService>();
            WebApiEngine wae = new WebApiEngine(ApiRequestMethod.Get, copyCustomAttributes: true, taskCount: 400, showDetailApiLog: true);
            base.ServiceCollection.UseAndInitDynamicApi(MvcCoreBuilder, Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "App_Data"));
        }
    }
}
