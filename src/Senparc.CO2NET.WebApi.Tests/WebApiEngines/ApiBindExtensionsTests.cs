using Microsoft.VisualStudio.TestTools.UnitTesting;
using Senparc.CO2NET.WebApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Senparc.CO2NET.WebApi.Tests
{
    public class TestClass
    {
        [ApiBindAttribute("ThisIsTestClass", "ThisIsTestClass.Func")]
        public string Func()
        {
            return "abc";
        }

        [ApiBindAttribute]
        public string FuncWithoutAttr()
        {
            return "abc2";
        }
    }

    [TestClass()]
    public class ApiBindExtensionsTests : BaseTest
    {
        [TestMethod()]
        public void GetDynamicCategoryTest()
        {
            Init();

            {
                var methodInfo = typeof(TestClass).GetMethod("Func");
                var attr = methodInfo.GetCustomAttributes(typeof(ApiBindAttribute), true).First() as ApiBindAttribute;
                var result = attr.GetDynamicCategory(methodInfo);
                Assert.AreEqual("Senparc.DynamicWebApi" + ".ThisIsTestClass", result);
            }

            {
                var methodInfo = typeof(TestClass).GetMethod("FuncWithoutAttr");
                var attr = methodInfo.GetCustomAttributes(typeof(ApiBindAttribute), true).First() as ApiBindAttribute;
                var result = attr.GetDynamicCategory(methodInfo);
                Assert.AreEqual("Senparc.DynamicWebApi" + ".Senparc.CO2NET.WebApi.Tests", result);
            }
        }
    }
}