using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Senparc.CO2NET.Helpers;
using Senparc.CO2NET.Helpers.Serializers;

namespace Senparc.CO2NET.Tests.Helpers
{
    [TestClass]
    public class SerializerHelperTests
    {
        [TestMethod]
        public void DecodeUnicodeTest()
        {
            var input = "\u7B2C01\u96C6";
            var result = SerializerHelper.DecodeUnicode(input);
            
            Console.WriteLine(result);

            Assert.IsTrue(result.Length > 0);

            //TODO:需要更多条件的测试内容
        }
    }
}
