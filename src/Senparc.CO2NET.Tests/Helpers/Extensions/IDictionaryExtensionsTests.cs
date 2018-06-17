using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Senparc.CO2NET.Helpers;
using Senparc.CO2NET.Tests.TestEntities;

namespace Senparc.CO2NET.Tests.Helpers
{
    [TestClass]
    public class IDictionaryExtensionsTests
    {
        [TestMethod]
        public void IDictionaryExtensionsTest()
        {
            var dic = new Dictionary<string, object>();
            dic["A"] = 1;
            dic["B"] = "2";
            dic["C"] = new TestCustomObject() { Id = 666 };

            dynamic result = dic.ToExpando();

            Assert.AreEqual(1, result.A);
            Assert.AreEqual("2", result.B);
            Assert.IsInstanceOfType( result.C,typeof(TestCustomObject));
            Assert.AreEqual(666, (result.C as TestCustomObject).Id);
        }
    }
}
