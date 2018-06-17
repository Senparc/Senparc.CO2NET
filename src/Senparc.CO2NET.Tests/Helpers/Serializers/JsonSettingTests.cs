using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Senparc.CO2NET.Helpers.Serializers;

namespace Senparc.CO2NET.Tests.Helpers
{
    [TestClass]
    public class JsonSettingTests
    {
        [TestMethod]
        public void JsonSettingTest()
        {
            var jsonSetting = new JsonSetting();
            Assert.IsNotNull(jsonSetting);
            
            //TODO:需要更多条件的测试内容
        }
    }
}
