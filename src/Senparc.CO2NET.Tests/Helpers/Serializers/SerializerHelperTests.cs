using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
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
            var input = " ¢≈…Õ¯¬Á";
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
            Assert.AreEqual(" ¢≈…Õ¯¬Á", result);

            //TODO:”–÷ÿ–¥∑Ω∑®–Ë“™≤‚ ‘
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

            var jsonStr = JsonConvert.SerializeObject(rootClass, Formatting.Indented, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            });
            Console.WriteLine(jsonStr);

            Assert.AreEqual(@"{
  ""$type"": ""Senparc.CO2NET.Tests.Helpers.SerializerHelperJsonTests+RootClass, Senparc.CO2NET.Tests"",
  ""A"": ""1"",
  ""B"": 2,
  ""C"": null,
  ""ElementClassA"": {
    ""$type"": ""Senparc.CO2NET.Tests.Helpers.SerializerHelperJsonTests+ElementClass, Senparc.CO2NET.Tests"",
    ""A"": ""A"",
    ""B"": ""B"",
    ""RootClass"": null
  },
  ""ElementClassB"": null,
  ""ElementClass2"": null
}", jsonStr.Trim());

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
