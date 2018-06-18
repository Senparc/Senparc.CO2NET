using System;
using System.IO;
using System.Xml.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Senparc.CO2NET.Tests.TestEntities;
using Senparc.CO2NET.Utilities;

namespace Senparc.CO2NET.Tests.Utilities
{
    [TestClass]
    public class XmlUtilityTests
    {
        string xml = @"<TestCustomObject>
<Id>666</Id>
<Name>Jeffrey</Name>
</TestCustomObject>";//注意：根节点名称需要和实体类名相同，否则可能需要设置xmlns等

        [TestMethod]
        public void DeserializeTest()
        {
            var result = XmlUtility.Deserialize<TestCustomObject>(xml) as TestCustomObject;
            Assert.IsNotNull(result);
            Assert.AreEqual(666, result.Id);
            Assert.AreEqual("Jeffrey", result.Name);
        }

        [TestMethod]
        public void DeserializeStreamTest()
        {
            using (var ms = new MemoryStream())//模拟已有stream
            {
                using (var sw = new StreamWriter(ms))
                {
                    sw.Write(xml);
                    sw.Flush();
                    ms.Position = 0;


                    var result = XmlUtility.Deserialize<TestCustomObject>(ms) as TestCustomObject;

                    Assert.IsNotNull(result);
                    Assert.AreEqual(666, result.Id);
                    Assert.AreEqual("Jeffrey", result.Name);
                }
            }

        }

        [TestMethod]
        public void SerializerTest()
        {
            var obj = new TestCustomObject()
            {
                Id = 666,
                Name = "Jeffrey Su",
                Markers = 7
            };

            var xmlStr = XmlUtility.Serializer(obj);

            Console.WriteLine(xmlStr);

            Assert.IsNotNull(xmlStr);
            Assert.IsTrue(xmlStr.Length > 0);
        }


        [TestMethod]
        public void ConvertTest()
        {
            using (var ms = new MemoryStream())//模拟已有stream
            {
                using (var sw = new StreamWriter(ms))
                {
                    sw.Write(xml);
                    sw.Flush();
                    ms.Position = 0;

                    var xdoc = XmlUtility.Convert(ms);
                    Console.WriteLine(xdoc.ToString());

                    Assert.AreEqual("666", xdoc.Root.Element("Id").Value);
                    Assert.AreEqual("Jeffrey", xdoc.Root.Element("Name").Value);
                }
            }
        }
    }
}
