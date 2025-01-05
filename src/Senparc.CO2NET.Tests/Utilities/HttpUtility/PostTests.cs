using Senparc.CO2NET.HttpUtility;
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
using Senparc.CO2NET.Helpers;
using Senparc.CO2NET.Tests;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Senparc.CO2NET.Helpers.Serializers;

namespace Senparc.CO2NET.HttpUtility.Tests
{
    [TestClass]
    public class PostTests : BaseTest
    {
        string ApiMpHost = "https://api.weixin.qq.com";

        [TestMethod]
        public void PostGetJsonTest()
        {
            //return;//Already passed, but requires remote connection testing, too time-consuming, temporarily ignored during routine testing.
            var url = ApiMpHost + "/cgi-bin/media/upload?access_token=TOKEN&type=image";

            //Here, due to parameter errors, the system will return error information
            WxJsonResult resultFail = Post.PostGetJson<WxJsonResult>(BaseTest.serviceProvider, url, cookieContainer: null, formData: null, encoding: null);
            //Assert.Fail();//An exception should have been thrown in the previous step
            Console.WriteLine(resultFail);
            Assert.AreEqual(40001, resultFail.errcode);
        }

        [TestMethod()]
        public async Task PostGetJsonAsyncTest()
        {
            //return;//Already passed, but requires remote connection testing, too time-consuming, temporarily ignored during routine testing.
            var url = ApiMpHost + "/cgi-bin/media/upload?access_token=TOKEN&type=image";

            WxJsonResult resultFail =
                await Post.PostGetJsonAsync<WxJsonResult>(BaseTest.serviceProvider, url, cookieContainer: null, formData: null,
                        encoding: null);
            Console.WriteLine(resultFail);
            Assert.AreEqual(40001, resultFail.errcode);
        }

        [TestMethod]
        public void PostGetJsonByFormDataTest()
        {
            //return;//Already passed, but requires remote connection testing, too time-consuming, temporarily ignored during routine testing.
            var url = "http://localhost:12222/P2P/GetPassport";
            try
            {
                //Here, due to parameter errors, the system will return error information
                var formData = new Dictionary<string, string>();
                formData["appKey"] = "test";
                formData["secret"] = "test2";
                var resultFail = Post.PostGetJson<object>(BaseTest.serviceProvider, url, formData: formData);
            }
            catch (Exception ex)
            {
                Assert.Fail();
            }
        }


        [TestMethod]
        public async Task PostGetJsonStreamTest()
        {
            //return;//Already passed, but requires remote connection testing, too time-consuming, temporarily ignored during routine testing.
            var url = "http://localhost:58936/VirtualPath/weixin?timestamp=1559561525&nonce=05dde8fcd38e46fea0b7af7517831c5f&echostr=251ee1f4c800429282c824f46dd0f47b&signature=402a11f9b8537b08239607af0a789400796c5861";
            try
            {
                var doc = new XDocument();
                doc.Add(new XElement("xml"));
                doc.Root.Add(new XElement("ToUserName", "Senparc"));
                doc.Root.Add(new XElement("FromUserName", "NeuChar"));
                doc.Root.Add(new XElement("CreateTime", DateTimeHelper.GetUnixDateTime(DateTime.Now).ToString()));
                doc.Root.Add(new XElement("MsgType", "NeuChar"));
                doc.Root.Add(new XElement("MsgId", DateTime.Now.Ticks.ToString()));
                doc.Root.Add(new XElement("NeuCharMessageType", "GetConfig"));//Set type
                                                                              //Send request
                var ms = new MemoryStream();
                var sr = new StreamWriter(ms);
                sr.Write(doc.ToString());
                sr.Flush();
                ms.Seek(0, SeekOrigin.Begin);

                var result = await Post.PostGetJsonAsync<dynamic>(BaseTest.serviceProvider, url, fileStream: ms, encoding: Encoding.UTF8);

                Console.WriteLine(result);
            }
            catch (Exception ex)
            {
                //Actual returned information (error information)
                Console.WriteLine(ex.Message);
                Assert.Fail();//An exception should have been thrown in the previous step
            }
        }

        [TestMethod]
        public void DownLoadTest()
        {

        }

        [TestMethod()]
        public void PostFileGetJsonTest()
        {
            var agentId = "1000009";
            var accessToken = "D0pI7JIOdFMfBPZ3QNIdazGupfEFlXNfC8aScj6BS3Vcdk3EjRwWdIJ_cxIQNbMoqdhWjHb6PplzK4tQ88MXz2qCugIhJ82IqBWTF-Q8ggK24QE8-iYB8c2yiSRZkTGirdbDLfZk4ERMs7GhhIkR4UiHplhNjtenXaztHAietRNUhQMhrVbw_vVMFgvYeDiAwzjP1Ntv0KddjWvDaXtscg";

            //Upload test
            var type = "file";
            var url = $"https://qyapi.weixin.qq.com/cgi-bin/media/upload?access_token={accessToken}&type={type}";
            var fileDictionary = new Dictionary<string, string>();
            fileDictionary["media"] = "E:\\Senparc项目\\WeiXinMPSDK\\src\\Senparc.Weixin.Work\\Senparc.Weixin.Work.Test\\AdvancedAPIs\\Media\\中文名.txt";

            var uploadResult = CO2NET.HttpUtility.Post.PostFileGetJsonAsync<dynamic>(BaseTest.serviceProvider, url, null, fileDictionary, null, null, null, null, false).GetAwaiter().GetResult();

            Console.WriteLine(uploadResult);

            var mediaId = uploadResult.media_id;

            Console.WriteLine("mediaId:" + mediaId);

            //Send test
            var data = new
            {
                touser = "001",
                toparty = (string)null,
                totag = (string)null,
                msgtype = "file",
                agentid = agentId,
                file = new
                {
                    media_id = mediaId
                },
                safe = 0,
                enable_duplicate_check = 0,
                duplicate_check_interval = 1800
            };
            JsonSetting jsonSetting = new JsonSetting(true);
            var jsonString = SerializerHelper.GetJsonString(data, jsonSetting);
            using (MemoryStream ms = new MemoryStream())
            {
                var bytes = Encoding.UTF8.GetBytes(jsonString);
                ms.Write(bytes, 0, bytes.Length);
                ms.Seek(0, SeekOrigin.Begin);
                var sendUrl = $"https://qyapi.weixin.qq.com/cgi-bin/message/send?access_token={accessToken}";
                var sendResult = Post.PostGetJsonAsync<dynamic>(BaseTest.serviceProvider, sendUrl, null, ms).GetAwaiter().GetResult();
                Console.WriteLine("sendResult:");
                Console.WriteLine(SerializerHelper.GetJsonString(sendResult, jsonSetting));
            }

        }

        /// <summary>
        /// v2.1.7.1 Exception fix test (This test should pass after v2.1.7.2)
        /// </summary>
        [TestMethod()]
        public void PostGetJsonTestForV2_1_7_1()
        {
            /* For exception:
            Middleware type: WxOpenMessageHandlerMiddleware`1
MessageHandler type: CustomWxOpenMessageHandler
Exception information: Senparc.NeuChar.Exceptions.MessageHandlerException: Error occurred in Execute() process of MessageHandler: The value cannot be null or empty. (Parameter 'mediaType')
 ---> System.ArgumentException: The value cannot be null or empty. (Parameter 'mediaType')
   at System.Net.Http.Headers.MediaTypeHeaderValue.CheckMediaTypeFormat(String mediaType, String parameterName)
   at System.Net.Http.Headers.MediaTypeHeaderValue..ctor(String mediaType)
   at Senparc.CO2NET.HttpUtility.RequestUtility.HttpPost_Common_NetCoreAsync(IServiceProvider serviceProvider, String url, CookieContainer cookieContainer, Stream postStream, Dictionary`2 fileDictionary, String refererUrl, Encoding encoding, String certName, Boolean useAjax, Dictionary`2 headerAddition, Int32 timeOut, Boolean checkValidationResult, Boolean hasFormData, String contentType)
   at Senparc.CO2NET.HttpUtility.RequestUtility.HttpResponsePostAsync(IServiceProvider serviceProvider, String url, CookieContainer cookieContainer, Stream postStream, Dictionary`2 fileDictionary, String refererUrl, Encoding encoding, String certName, Boolean useAjax, Dictionary`2 headerAddition, Boolean hasFormData, Int32 timeOut, Boolean checkValidationResult, String contentType)
   at Senparc.CO2NET.HttpUtility.RequestUtility.HttpPostAsync(IServiceProvider serviceProvider, String url, CookieContainer cookieContainer, Stream postStream, Dictionary`2 fileDictionary, String refererUrl, Encoding encoding, String certName, Boolean useAjax, Dictionary`2 headerAddition, Boolean hasFormData, Int32 timeOut, Boolean checkValidationResult, String contentType)
   at Senparc.CO2NET.HttpUtility.Post.PostGetJsonAsync[T](IServiceProvider serviceProvider, String url, CookieContainer cookieContainer, Stream fileStream, Encoding encoding, String certName, Boolean useAjax, Boolean checkValidationResult, String contentType, Action`2 afterReturnText, Int32 timeOut)
   at Senparc.Weixin.CommonAPIs.CommonJsonSend.SendAsync[T](String accessToken, String urlFormat, Object data, CommonJsonSendType sendType, Int32 timeOut, Boolean checkValidationResult, JsonSetting jsonSetting, String contentType) in E:\Senparc项目\WeiXinMPSDK\src\Senparc.Weixin\Senparc.Weixin\CommonAPIs\CommonJsonSend.cs:line 254
   at Senparc.Weixin.CommonAPIs.CommonJsonSend.SendAsync(String accessToken, String urlFormat, Object data, CommonJsonSendType sendType, Int32 timeOut, Boolean checkValidationResult, JsonSetting jsonSetting, String contentType) in E:\Senparc项目\WeiXinMPSDK\src\Senparc.Weixin\Senparc.Weixin\CommonAPIs\CommonJsonSend.cs:line 216
   at Senparc.Weixin.WxOpen.AdvancedAPIs.CustomApi.<>c__DisplayClass11_0.<<SendTextAsync>b__0>d.MoveNext() in E:\Senparc项目\WeiXinMPSDK\src\Senparc.Weixin.WxOpen\src\Senparc.Weixin.WxOpen\Senparc.Weixin.WxOpen\AdvancedAPIs\Custom\CustomApi.cs:line 277
--- End of stack trace from previous location ---
   at Senparc.Weixin.CommonAPIs.ApiHandlerWapper.ApiHandlerWapperBase.TryCommonApiBaseAsync[T](PlatformType platformType, Func`1 accessTokenContainer_GetFirstOrDefaultAppIdAsyncFunc, Func`2 accessTokenContainer_CheckRegisteredAsyncFunc, Func`3 accessTokenContainer_GetAccessTokenResultAsyncFunc, IEnumerable`1 invalidCredentialValues, Func`2 fun, String accessTokenOrAppId, Boolean retryIfFaild) in E:\Senparc项目\WeiXinMPSDK\src\Senparc.Weixin\Senparc.Weixin\CommonAPIs\ApiHandlerWapper\ApiHandlerWapperBase.cs:line 352
   at Senparc.Weixin.WxOpen.WxOpenApiHandlerWapper.TryCommonApiAsync[T](Func`2 fun, String accessTokenOrAppId, Boolean retryIfFaild) in E:\Senparc项目\WeiXinMPSDK\src\Senparc.Weixin.WxOpen\src\Senparc.Weixin.WxOpen\Senparc.Weixin.WxOpen\CommonAPIs\WxOpenApiHandlerWapper.cs:line 239
   at Senparc.Weixin.WxOpen.AdvancedAPIs.CustomApi.SendTextAsync(String accessTokenOrAppId, String openId, String content, String businessId, Int32 timeOut) in E:\Senparc项目\WeiXinMPSDK\src\Senparc.Weixin.WxOpen\src\Senparc.Weixin.WxOpen\Senparc.Weixin.WxOpen\AdvancedAPIs\Custom\CustomApi.cs:line 273
   at Senparc.Weixin.Sample.CommonService.WxOpenMessageHandler.CustomWxOpenMessageHandler.OnEvent_UserEnterTempSessionRequestAsync(RequestMessageEvent_UserEnterTempSession requestMessage) in E:\Senparc项目\WeiXinMPSDK\Samples\All\Senparc.Weixin.Sample.CommonService\MessageHandlers\WxOpenMessageHandler\CustomWxOpenMessageHandler.cs:line 222
   at Senparc.Weixin.WxOpen.MessageHandlers.WxOpenMessageHandler`1.OnEventRequestAsync(IRequestMessageEventBase requestMessage) in E:\Senparc项目\WeiXinMPSDK\src\Senparc.Weixin.WxOpen\src\Senparc.Weixin.WxOpen\Senparc.Weixin.WxOpen\MessageHandlers\WxOpenMessageHandler.Event.cs:line 248
   at Senparc.Weixin.WxOpen.MessageHandlers.WxOpenMessageHandler`1.BuildResponseMessageAsync(CancellationToken cancellationToken) in E:\Senparc项目\WeiXinMPSDK\src\Senparc.Weixin.WxOpen\src\Senparc.Weixin.WxOpen\Senparc.Weixin.WxOpen\MessageHandlers\WxOpenMessageHandler.cs:line 335
   at Senparc.NeuChar.MessageHandlers.MessageHandler`3.ExecuteAsync(CancellationToken cancellationToken)
   --- End of inner exception stack trace ---
   at Senparc.NeuChar.MessageHandlers.MessageHandler`3.ExecuteAsync(CancellationToken cancellationToken)
   at Senparc.NeuChar.MessageHandlers.MessageHandler`3.ExecuteAsync(CancellationToken cancellationToken)
   at Senparc.NeuChar.Middlewares.MessageHandlerMiddleware`5.Invoke(HttpContext context)
*/

            var url = "https://api.weixin.qq.com/cgi-bin/message/subscribe/send?access_token=<AccessToken>";
            var data = @"{""touser"":""oeaTy0DgoGq-lyqvTauWVjbIVuP0"",""template_id"":""xWclWkOqDrxEgWF4DExmb9yUe10pfmSSt2KM6pY7ZlU"",""page"":""pages/index/index"",""data"":{""thing1"":{""value"":""微信公众号+小程序快速开发""},""time5"":{""value"":""2023年01月12日 17:32""},""thing6"":{""value"":""盛派网络研究院""},""thing7"":{""value"":""第二部分课程正在准备中，尽情期待""}}}";

            var ms = new MemoryStream();
            var sw = new StreamWriter(ms);
            sw.Write(data);
            sw.Flush();

            var result = HttpUtility.Post.PostGetJsonAsync<dynamic>(BaseTest.serviceProvider, url, null, ms).GetAwaiter().GetResult();
            Assert.IsNotNull(result);
            Console.WriteLine(result);

            sw.Close();
        }
    }
}
