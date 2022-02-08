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

        [ApiBind]
        public string FuncWithoutAttrParam()
        {
            return "abc2";
        }
    }

    [ApiBind]
    public class TestCoverClass
    {
        public string FuncWithoutAttr1()
        {
            return "abc3";
        }

        public string FuncWithoutAttr2()
        {
            return "abc4";
        }
    }

    [TestClass()]
    public class ApiBindExtensionsTests : BaseTest
    {
        [TestMethod()]
        public void GetDynamicCategoryTest()
        {
            Init();
            var assemblyName = this.GetType().Assembly.GetName().Name;

            {
                var methodInfo = typeof(TestClass).GetMethod("Func");
                var attr = methodInfo.GetCustomAttributes(typeof(ApiBindAttribute), true).First() as ApiBindAttribute;
                var result = attr.GetDynamicCategory(methodInfo, assemblyName);
                Assert.AreEqual("Senparc.DynamicWebApi" + ".ThisIsTestClass", result);
            }

            {
                var methodInfo = typeof(TestClass).GetMethod("FuncWithoutAttrParam");
                var attr = methodInfo.GetCustomAttributes(typeof(ApiBindAttribute), true).First() as ApiBindAttribute;
                var result = attr.GetDynamicCategory(methodInfo, assemblyName);
                Assert.AreEqual("Senparc.DynamicWebApi" + ".Senparc.CO2NET.WebApi.Tests", result);
            }
        }
    }
}