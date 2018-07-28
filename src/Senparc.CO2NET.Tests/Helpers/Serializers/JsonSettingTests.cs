using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Senparc.CO2NET.Helpers;
using Senparc.CO2NET.Helpers.Serializers;

namespace Senparc.CO2NET.Tests.Helpers
{
    [TestClass]
    public class JsonSettingTests
    {
        #region JsonSetting ²âÊÔ

        [Serializable]
        public class WeixinData
        {
            public int Id { get; set; }
            public string UserName { get; set; }
            public string Note { get; set; }
            public string Sign { get; set; }
            public Sex Sex { get; set; }
        }


        [TestMethod]
        public void JsonSettingTest()
        {
            var weixinData = new WeixinData()
            {
                Id = 1,
                UserName = "JeffreySu",
                Note = null,
                Sign = null,
                Sex = Sex.ÄÐ
            };

            //string json = js.GetJsonString(weixinData);
            //Console.WriteLine(json);

            //JsonSetting jsonSetting = new JsonSetting(true);
            //string json2 = js.GetJsonString(weixinData, jsonSetting);
            //Console.WriteLine(json2);

            JsonSetting jsonSetting3 = new JsonSetting(true, new List<string>() { "Note" });
            string json3 = SerializerHelper.GetJsonString(weixinData, jsonSetting3);
            Console.WriteLine(json3);
        }

        [TestMethod]
        public void NullAndEmptyTest()
        {
            JsonSetting jsonSetting = new JsonSetting(true);
            var data = new { scene = (string)null, page = "pages/websocket/websocket", width = 100, line_color = "red", is_hyaline = true };
            var json = SerializerHelper.GetJsonString(data, jsonSetting);
            Console.WriteLine(json);
            Assert.AreEqual("{\"page\":\"pages/websocket/websocket\",\"width\":100,\"line_color\":\"red\",\"is_hyaline\":true}", json);

            //²âÊÔ¿Õ×Ö·û´®
             data = new { scene = "", page = "pages/websocket/websocket", width = 100, line_color = "red", is_hyaline = true };
            json = SerializerHelper.GetJsonString(data, jsonSetting);
            Console.WriteLine(json);
            Assert.AreEqual("{\"scene\":\"\",\"page\":\"pages/websocket/websocket\",\"width\":100,\"line_color\":\"red\",\"is_hyaline\":true}", json);
        }

        #endregion
    }
}
