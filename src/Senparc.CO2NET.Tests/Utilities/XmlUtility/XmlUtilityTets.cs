using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Senparc.CO2NET.Tests.TestEntities;
using Senparc.CO2NET.Utilities;

namespace Senparc.CO2NET.Tests.Utilities
{
    [TestClass]
    public class XmlUtilityTests
    {
        [TestMethod]
        public void DeserializeTest()
        {
            var xml = @"<?xml>
 <node>
<Id>666</Id>
<Name>Jeffrey</Name>
</node>";

            var result = XmlUtility.Deserialize<TestCustomObject>(xml) as TestCustomObject;
            Assert.IsNotNull(result);
            Assert.AreEqual(666, result.Id);
            Assert.AreEqual("Jeffrey", result.Name);


        }
    }
}
