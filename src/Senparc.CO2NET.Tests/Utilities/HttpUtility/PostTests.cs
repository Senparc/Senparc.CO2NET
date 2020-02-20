#region Apache License Version 2.0
/*----------------------------------------------------------------

Copyright 2019 Suzhou Senparc Network Technology Co.,Ltd.

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

namespace Senparc.CO2NET.HttpUtility.Tests
{
    [TestClass]
    public class PostTests : BaseTest
    {
        string ApiMpHost = "https://api.weixin.qq.com";

        [TestMethod]
        public void PostGetJsonTest()
        {
            //return;//已经通过，但需要连接远程测试，太耗时，常规测试时暂时忽略。
            var url = ApiMpHost + "/cgi-bin/media/upload?access_token=TOKEN&type=image";
            try
            {
                //这里因为参数错误，系统会返回错误信息
                WxJsonResult resultFail = Post.PostGetJson<WxJsonResult>(BaseTest.serviceProvider, url, cookieContainer: null, formData: null, encoding: null);
                Assert.Fail();//上一步就应该已经抛出异常
            }
            catch (Exception ex)
            {
                //实际返回的信息（错误信息）
                Console.WriteLine(ex.Message);
            }
        }

        [TestMethod()]
        public async Task PostGetJsonAsyncTest()
        {
            //return;//已经通过，但需要连接远程测试，太耗时，常规测试时暂时忽略。
            var url = ApiMpHost + "/cgi-bin/media/upload?access_token=TOKEN&type=image";

            try
            {
                WxJsonResult resultFail =
                    await Post.PostGetJsonAsync<WxJsonResult>(BaseTest.serviceProvider, url, cookieContainer: null, formData: null,
                            encoding: null);
                //这里因为参数错误，系统会返回错误信息
                Assert.Fail(); //上一步就应该已经抛出异常

            }
            catch (Exception ex)
            {
                //实际返回的信息（错误信息）
                Console.WriteLine(ex.Message);
                Console.WriteLine("Success");
            }
        }

        [TestMethod]
        public void PostGetJsonByFormDataTest()
        {
            //return;//已经通过，但需要连接远程测试，太耗时，常规测试时暂时忽略。
            var url = "http://localhost:12222/P2P/GetPassport";
            try
            {
                //这里因为参数错误，系统会返回错误信息
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
            //return;//已经通过，但需要连接远程测试，太耗时，常规测试时暂时忽略。
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
                doc.Root.Add(new XElement("NeuCharMessageType", "GetConfig"));//设置类型
                                                                              //发送请求
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
                //实际返回的信息（错误信息）
                Console.WriteLine(ex.Message);
                Assert.Fail();//上一步就应该已经抛出异常
            }
        }

        [TestMethod]
        public void DownLoadTest()
        {

        }


    }
}
