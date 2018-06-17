using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Senparc.CO2NET.Helpers;
using Senparc.CO2NET.Tests.TestEntities;

namespace Senparc.CO2NET.Tests.Helpers
{
    [TestClass]
    public class ReflectionHelperTests
    {
        [TestMethod]
        public void CreateInstanceTest()
        {
            var obj = ReflectionHelper.CreateInstance<TestCustomObject>("Senparc.CO2NET.Tests.TestEntities.TestCustomObject", "Senparc.CO2NET.Tests");
            Assert.IsNull(obj);
            Assert.IsInstanceOfType(obj, typeof(TestCustomObject));
        }

        [TestMethod]
        public void CreateInstanceTest2()
        {
            var obj = ReflectionHelper.CreateInstance<TestCustomObject>("Senparc.CO2NET.Tests", "Senparc.CO2NET.Tests.TestEntities", "TestCustomObject");
            Assert.IsNull(obj);
            Assert.IsInstanceOfType(obj, typeof(TestCustomObject));
        }

    }
}
