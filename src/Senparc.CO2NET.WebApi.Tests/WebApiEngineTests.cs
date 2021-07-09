using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Senparc.CO2NET.Utilities;
using Senparc.CO2NET.WebApi.WebApiEngines;
using System;
using System.IO;

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
            //string load = "" + typeof(Senparc.Weixin.MP.Register) + typeof(Senparc.Weixin.WxOpen.Register) + typeof(Senparc.Weixin.Open.Register);

            var findWeixinApiService = ServiceProvider.GetService<FindApiService>();
            var appDataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "App_Data");// ServerUtility.ContentRootMapPath("~/App_Data");
            base.ServiceCollection.AddAndInitDynamicApi(MvcCoreBuilder, appDataPath, taskCount: 400);
        }
    }
}
