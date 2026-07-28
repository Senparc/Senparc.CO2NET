using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text.Json;
using System.Text.Json.Serialization;
using Senparc.CO2NET.Helpers;
using Senparc.CO2NET.Helpers.Serializers;
using static Senparc.CO2NET.Tests.Helpers.SerializerHelperJsonTests;

namespace Senparc.CO2NET.Tests.Helpers
{
    [TestClass]
    public partial class SerializerHelperTests
    {
        [TestMethod]
        public void EncodeUnicodeTest()
        {
            var input = "盛派网络";
            var result = SerializerHelper.EncodeUnicode(input);
            Console.WriteLine(result);
            Assert.IsNotNull(result);
            Assert.IsTrue(result != null && result.Contains("\\u"));
        }

        [TestMethod]
        public void DecodeUnicodeTest()
        {
            var input = "\\u76DB\\u6D3E\\u7F51\\u7EDC";
            var result = SerializerHelper.DecodeUnicode(input);
            Console.WriteLine(result);
            Assert.AreEqual("盛派网络", result);

            //TODO: Override methods need testing
        }

        [TestMethod()]
        public void GetObjectTest()
        {
            var rootClass = new RootClass()
            {
                A = "1",
                B = 2,
                ElementClassA = new ElementClass() { A = "A", B = "B" }
            };

            var jsonStr = JsonSerializer.Serialize(rootClass, new JsonSerializerOptions
            {
                WriteIndented = true,
                ReferenceHandler = ReferenceHandler.IgnoreCycles
            });
            Console.WriteLine(jsonStr);

            using (var document = JsonDocument.Parse(jsonStr))
            {
                Assert.AreEqual("1", document.RootElement.GetProperty("A").GetString());
                Assert.AreEqual(2, document.RootElement.GetProperty("B").GetInt32());
            }

            var data = SerializerHelper.GetObject<RootClass>(jsonStr);

            Assert.IsNotNull(data);
            Assert.AreEqual("1", data.A);
            Assert.AreEqual(2, data.B);
            Assert.IsNotNull(data.ElementClassA);
            Assert.AreEqual("A", data.ElementClassA.A);
            Assert.AreEqual("B", data.ElementClassA.B);
            Assert.IsNull(data.ElementClassB);
            Assert.IsNull(data.ElementClass2);
        }
    }
}
