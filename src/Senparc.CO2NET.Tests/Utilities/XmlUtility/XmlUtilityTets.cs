using System;
using System.IO;
using System.Threading.Tasks;
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
</TestCustomObject>";// Note: When serializing, ensure that the same namespace is used, and that xmlns is specified

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
            using (var ms = new MemoryStream())// Model for stream
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
            using (var ms = new MemoryStream())// Model for stream
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

        [TestMethod]
        public async Task ConvertAsyncTest()
        {
            using (var ms = new MemoryStream())// Model for stream
            {
                using (var sw = new StreamWriter(ms))
                {
                    await sw.WriteAsync(xml);
                    await sw.FlushAsync();
                    ms.Seek(0, SeekOrigin.Begin);

                    var xdoc = await XmlUtility.ConvertAsync(ms, new System.Threading.CancellationToken());
                    Console.WriteLine(xdoc.ToString());

                    Assert.AreEqual("666", xdoc.Root.Element("Id").Value);
                    Assert.AreEqual("Jeffrey", xdoc.Root.Element("Name").Value);

                    //Test wether sw didn't closed
                    ms.Seek(0, SeekOrigin.End);
                    await sw.WriteAsync("<END></END>");
                    await sw.FlushAsync();
                    ms.Seek(0,SeekOrigin.Begin);

                    using (var sr =new StreamReader(ms))
                    {
                        var str = await sr.ReadToEndAsync();
                        Console.WriteLine("new Str:");
                        Console.WriteLine(str);
                        Assert.IsTrue(str.EndsWith("<END></END>"));
                    }

                }
            }
        }
    }
}
