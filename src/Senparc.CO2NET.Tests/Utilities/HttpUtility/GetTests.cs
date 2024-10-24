#region Apache License Version 2.0
/*----------------------------------------------------------------

Copyright 2024 Suzhou Senparc Network Technology Co.,Ltd.

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

using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Senparc.CO2NET.Tests;

namespace Senparc.CO2NET.HttpUtility.Tests
{
    [TestClass]
    public class GetTest : BaseTest
    {
        [TestMethod]
        public void DownloadTest()
        {
            var url = "http://sdk.weixin.senparc.com/images/v2/ewm_01.png";
            using (FileStream fs = new FileStream(string.Format("qr-{0}.jpg", DateTime.Now.Ticks), FileMode.OpenOrCreate))
            {
                Get.Download(BaseTest.serviceProvider, url, fs);//Download
                fs.Flush();//Directly save, no need to handle the pointer
            }

            using (MemoryStream ms = new MemoryStream())
            {
                Get.Download(BaseTest.serviceProvider, url, ms);//Download
                ms.Seek(0, SeekOrigin.Begin);//Move the pointer to the beginning of the stream
                string base64Img = Convert.ToBase64String(ms.ToArray());//Output image base64 encoding
                Console.WriteLine(base64Img);
            }
        }

        //[TestMethod]
        //public void GetJsonTest()
        //{
        //    return; //Already passed, but requires remote connection for testing, too time-consuming, temporarily ignored during routine testing.

        //    {
        //        var url = "http://apistore.baidu.com/microservice/cityinfo?cityname=Suzhou";
        //        var result = Get.GetJson<dynamic>(url);
        //        Assert.IsNotNull(result);
        //        Assert.AreEqual(0, result["errNum"]);
        //        Assert.AreEqual("Suzhou", result["retData"]["cityName"]);

        //        Console.WriteLine(result.GetType());
        //    }

        //    {
        //        var url =
        //     Config.ApiMpHost + "/cgi-bin/token?grant_type=client_credential&appid=APPID&secret=APPSECRET";
        //        try
        //        {
        //            //Here, due to parameter errors, the system will return error information
        //            WxJsonResult resultFail = Get.GetJson<WxJsonResult>(url);
        //            Assert.Fail(); //An exception should have been thrown in the previous step
        //        }
        //        catch (ErrorJsonResultException ex)
        //        {
        //            //Actual returned information (error information)
        //            Assert.AreEqual(ex.JsonResult.errcode, ReturnCode.InvalidAPPID);
        //        }

        //    }

        //}

        //[TestMethod]
        //public void GetJsonAsyncTest()
        //{
        //    //return;//Already passed, but requires remote connection for testing, too time-consuming, temporarily ignored during routine testing.
        //    var url =
        //        Config.ApiMpHost + "/cgi-bin/token?grant_type=client_credential&appid=APPID&secret=APPSECRET";

        //    var t1 = Task.Factory.StartNew(async delegate { await Run(url); });
        //    var t2 = Task.Factory.StartNew(delegate { Run(url); });
        //    var t3 = Task.Factory.StartNew(delegate { Run(url); });
        //    var t4 = Task.Factory.StartNew(delegate { Run(url); });

        //    Console.WriteLine("Waiting...");
        //    Task.WaitAll(t1, t2, t3, t4);
        //}

        //private async Task Run(string url)
        //{
        //    Console.WriteLine("Start Task.CurrentId: {0}, Time: {1}", Task.CurrentId, DateTime.Now.Ticks);

        //    try
        //    {
        //        //Here, due to parameter errors, the system will return error information
        //        WxJsonResult resultFail = await Get.GetJsonAsync<WxJsonResult>(url);
        //        Assert.Fail(); //An exception should have been thrown in the previous step
        //    }
        //    catch (ErrorJsonResultException ex)
        //    {
        //        //Actual returned information (error information)
        //        Assert.AreEqual(ex.JsonResult.errcode, ReturnCode.InvalidAPPID);

        //        Console.WriteLine("End Task.CurrentId: {0}, Time: {1}", Task.CurrentId, DateTime.Now.Ticks);
        //    }
        //}
    }
}
