using Microsoft.VisualStudio.TestTools.UnitTesting;
using Senparc.CO2NET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Senparc.CO2NET.Tests
{
    public class TestClass
    {
        public string Func()
        {
            return "abc";
        }
    }

    [TestClass()]
    public class ApiBindAttributeTests
    {
        [TestMethod()]
        public void GetNameTest()
        {
            var method = typeof(TestClass).GetMethod("Func");
            var attr = new ApiBindAttribute();
            var result = attr.GetApiBindAttrName(method);
            Assert.AreEqual("TestClass.Func", result);
        }
    }
}