using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Senparc.CO2NET.Tests.TestEntities;

namespace Senparc.CO2NET.Tests.Helpers
{
#if NET462

    [TestClass]
    public class ExpandoJsonConverterTests
    {
        [TestMethod]
        public void DeserializeTest()
        {
            var dic = new Dictionary<string, object>();
            dic["A"] = 1;
            dic["B"] = "2";
            dic["C"] = new TestCustomObject() { Id = 666 };

            var convert = new Senparc.CO2NET.Helpers.ExpandoJsonConverter();
            var result = (dynamic)convert.Deserialize(dic, null, null);

            Assert.AreEqual(1, result.A);
            Assert.AreEqual("2", result.B);
            Assert.IsInstanceOfType(result.C, typeof(TestCustomObject));
            Assert.AreEqual(666, (result.C as TestCustomObject).Id);
        }
    }
#endif
}
