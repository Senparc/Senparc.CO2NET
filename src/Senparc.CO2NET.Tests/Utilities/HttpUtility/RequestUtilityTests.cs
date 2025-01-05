#region Apache License Version 2.0
/*----------------------------------------------------------------

Copyright 2025 Suzhou Senparc Network Technology Co.,Ltd.

Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file
except in compliance with the License. You may obtain a copy of the License at

http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software distributed under the
License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND,
either express or implied. See the License for the specific language governing permissions
and limitations under the License.

Detail: https://github.com/JeffreySu/WeiXinMPSDK/blob/master/license.md

----------------------------------------------------------------*/
#endregion Apache License Version 2.0

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Senparc.CO2NET.Tests;
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Senparc.CO2NET.Helpers;

namespace Senparc.CO2NET.HttpUtility.Tests
{
    [TestClass()]
    public class RequestUtilityTests : BaseTest
    {
        private string _domain = "https://sdk.weixin.senparc.com";//Local run Senaprc.Weixin SDK Sample(All) https://localhost:5021

        [TestMethod()]
        public void SetHttpProxyTest()
        {
            //Set
            RequestUtility.SetHttpProxy("http://192.168.1.130", "8088", "username", "pwd");

            //Clear
            RequestUtility.RemoveHttpProxy();
        }


        [TestMethod]
        public void PostTest()
        {
            var data = "Jeffrey";
            Stream stream = new MemoryStream();
            var bytes = Encoding.UTF8.GetBytes(data);
            stream.Write(bytes, 0, bytes.Length);
            stream.Seek(0, SeekOrigin.Begin);

            var cookieContainer = new CookieContainer();
            var url = $"{_domain}/ForTest/PostTest";//Sample using .NET 4.5
            var result = RequestUtility.HttpPost(BaseTest.serviceProvider, url,
                cookieContainer, stream, useAjax: true);

            Console.WriteLine(result);

            Assert.IsNotNull(result);
        }

        /// <summary>
        /// Test WeChat special interface, returns null after normal request
        /// Test result: Actually received a 503 response, but PostMan is usable.
        /// </summary>
        [TestMethod]
        public void PostJsonDataTest()
        {
            var data = @"{""name"":""hardenzhang"",""longitude"":""113.323753357"",""latitude"":""23.0974903107"",""province"":""广东省"",""city"":""广州市"",""district"":""海珠区"",""address"":""TTT"",""category"":""美食: 中餐厅"",""telephone"":""12345678901"",""photo"":""http://mmbiz.qpic.cn/mmbiz_png/tW66AWE2K6ECFPcyAcIZTG8RlcR0sAqBibOm8gao5xOoLfIic9ZJ6MADAktGPxZI7MZLcadZUT36b14NJ2cHRHA/0?wx_fmt=png"",""license"":""http://mmbiz.qpic.cn/mmbiz_png/tW66AWE2K6ECFPcyAcIZTG8RlcR0sAqBibOm8gao5xOoLfIic9ZJ6MADAktGPxZI7MZLcadZUT36b14NJ2cHRHA/0?wx_fmt=png"",""introduct"":""test"",""districtid"":""440105""}";
            Stream stream = new MemoryStream();
            var bytes = Encoding.UTF8.GetBytes(data);
            stream.Write(bytes, 0, bytes.Length);
            stream.Seek(0, SeekOrigin.Begin);

            var cookieContainer = new CookieContainer();
            var accesstoken = "34_WeSuCDgRVtJ0KfPlS0fNdMtBZ4XQDes54MIHt4HlaFkpkItYpLfr0OlfLsntE73eWK_jVifGWxoV2zygK4J2tE6U4eDnNUeLupAkSqf83WMh-6QgNPK9_f6r8xiMlNzVald2l1sKyaQcDPHgSXPlCGAZEW";
            var url = "https://api.weixin.qq.com/wxa/create_map_poi?access_token=" + accesstoken;
            var result = RequestUtility.HttpPost(BaseTest.serviceProvider, url,
                cookieContainer, stream, useAjax: false);

            Console.WriteLine(result);

            Assert.IsNotNull(result);
        }


        [TestMethod]
        public void SenparcHttpResponseTest()
        {
            var data = "Jeffrey";
            Stream stream = new MemoryStream();
            var bytes = Encoding.UTF8.GetBytes(data);
            stream.Write(bytes, 0, bytes.Length);
            stream.Seek(0, SeekOrigin.Begin);

            var cookieContainer = new CookieContainer();
            var url = $"{_domain}/ForTest/PostTest";//Sample using .NET 4.5
            var result = RequestUtility.HttpResponsePost(BaseTest.serviceProvider, url,
                cookieContainer, stream, useAjax: true);

            Assert.IsNotNull(result);
#if !NET462
            var resultString = result.Result.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            Console.WriteLine("resultString : \t{0}", resultString);
#endif
            var cookie = cookieContainer.GetCookies(new Uri($"{_domain}"));
            Console.WriteLine("TestCookie：{0}", cookie["TestCookie"]);
        }

        [TestMethod]
        public void PostCookieTest()
        {
            var cookieContainer = new CookieContainer();
            //cookieContainer.Add(new Uri("https://localhost"), new Cookie("TestCount", "20"));
            cookieContainer.SetCookies(new Uri($"{_domain}/ForTest/PostTest"), "TestCount=100; path=/; domain=sdk.weixin.senparc.com; Expires=Tue, 19 Jan 2038 03:14:07 GMT;");

            for (int i = 0; i < 3; i++)
            {
                var data = "CookieTest";
                Stream stream = new MemoryStream();
                var bytes = Encoding.UTF8.GetBytes(data);
                stream.Write(bytes, 0, bytes.Length);
                stream.Seek(0, SeekOrigin.Begin);

                var url = $"{_domain}/ForTest/PostTest";//Sample using Senparc.Weixin SDK
                var result = RequestUtility.HttpResponsePost(BaseTest.serviceProvider, url, cookieContainer, stream, useAjax: true);

                Assert.IsNotNull(result);
                var resultString = result.Result.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                Console.WriteLine("resultString : \t{0}", resultString);

                var cookie = cookieContainer.GetCookies(new Uri($"{_domain}"));
                Console.WriteLine($"TestCookie：{cookie["TestCookie"]}，TestCount：{cookie["TestCount"]}\r\n");
            }
        }

        [TestMethod]
        public void GetCookieTest()
        {
            var cookieContainer = new CookieContainer();
            cookieContainer.SetCookies(new Uri($"{_domain}"), "TestCount=100; path=/; domain=sdk.weixin.senparc.com; Expires=Tue, 19 Jan 2038 03:14:07 GMT;");


            for (int i = 0; i < 3; i++)
            {
                var data = "CookieTest";

                var url = $"{_domain}/ForTest/GetTest?data={data}";//Sample using Senparc.Weixin SDK
                var result = RequestUtility.HttpGet(BaseTest.serviceProvider, url, cookieContainer);

                Console.WriteLine("result length: \t{0}", result.Length);
                Assert.IsTrue(result.Length > 0 && result.StartsWith($"{data} Ajax:"));

                var cookie = cookieContainer.GetCookies(new Uri($"{_domain}/ForTest/GetTest"));
                Console.WriteLine($"TestCookie：{cookie["TestCookie"]}，TestCount：{cookie["TestCount"]}\r\n");
            }
        }

        [TestMethod()]
        public async Task HttpDeleteAsyncTest()
        {
            var url = "http://region-9.seetacloud.com:21930/langchain/local_doc_qa/delete_knowledge_base?knowledge_base_id=123";

            var result = await Senparc.CO2NET.HttpUtility.RequestUtility.HttpDeleteAsync(BaseTest.serviceProvider, url, null);
            await Console.Out.WriteLineAsync(result);
            Assert.IsTrue(result.Length > 0);

            var typedResult = result.GetObject<dynamic>();
            Assert.IsTrue(typedResult.msg.ToString().Length > 0);
        }
    }
}