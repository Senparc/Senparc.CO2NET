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

Detail: https://github.com/Senparc/Senparc.CO2NET/blob/master/LICENSE

----------------------------------------------------------------*/
#endregion Apache License Version 2.0

/*----------------------------------------------------------------
    Copyright (C) 2020 Senparc

    文件名：RequestUtility.cs
    文件功能描述：获取请求结果


    创建标识：Senparc - 20150211

    修改描述：整理接口

    修改标识：Senparc - 20150407
    修改描述：使用Post方法获取字符串结果 修改表单处理方法

    修改标识：Senparc - 20170122
    修改描述：v4.9.14 为AsUrlData方法添加null判断

    修改标识：Senparc - 20170122
    修改描述：v4.12.2 修复HttpUtility.UrlEncode方法错误

    修改标识：Senparc - 20170730
    修改描述：v4.13.3 为RequestUtility.HttpGet()方法添加Accept、UserAgent、KeepAlive设置

    修改标识：Senparc - 20180516
    修改描述：v4.21.0 支持 .NET Core 2.1.0-rc1-final 添加编译条件

    修改标识：Senparc - 20180518
    修改描述：v4.21.0 支持 .NET Core 2.1.0-rc1-final 添加编译条件

    -- CO2NET --

    修改标识：Senparc - 20181009
    修改描述：v0.2.15 Post 方法添加 headerAddition参数

    修改标识：Senparc - 20181215
    修改描述：v0.3.1 更新 RequestUtility.GetQueryString() 方法

    修改标识：Senparc - 20190429
    修改描述：v0.7.0 优化 HttpClient，重构 RequestUtility（包括 Post 和 Get），引入 HttpClientFactory 机制

    修改标识：Senparc - 20190521
    修改描述：v0.7.3 .NET Core 提供多证书注册功能

    修改标识：Senparc - 20190521
    修改描述：v0.8.4 HttpUtility.HttpPost_Common_NetCore 所调用的额 CreateFileContent 取消对 fileName 参数的 UrlEncode 编码

    修改标识：Senparc - 20190928
    修改描述：v1.0.101 RequestUtility.GetRequestMemoryStream() 增加对 .NET Core 3.0 AllowSynchronousIO 的设置

    -- 从 CO2NET 移植到 CO2NET.AspNet --
    
    修改标识：Senparc - 20180721
    修改描述：v0.1.0  从 CO2NET 移植到 CO2NET.AspNet
----------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Senparc.CO2NET.Helpers;
using Senparc.CO2NET.WebProxy;

#if NET45
using System.Web;
#else
using System.Net.Http;
using System.Net.Http.Headers;
using Senparc.CO2NET.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
#endif

namespace Senparc.CO2NET.AspNet.HttpUtility
{
    /// <summary>
    /// HTTP 请求工具类
    /// </summary>
    public static partial class RequestUtility
    {
        #region 代理

#if NET45
        private static System.Net.WebProxy _webproxy = null;
        /// <summary>
        /// 设置Web代理
        /// </summary>
        /// <param name="host"></param>
        /// <param name="port"></param>
        /// <param name="username"></param>
        /// <param name="password"></param>
        public static void SetHttpProxy(string host, string port, string username, string password)
        {
            ICredentials cred;
            cred = new NetworkCredential(username, password);
            if (!string.IsNullOrEmpty(host))
            {
                _webproxy = new System.Net.WebProxy(host + ":" + port ?? "80", true, null, cred);
            }
        }

        /// <summary>
        /// 清除Web代理状态
        /// </summary>
        public static void RemoveHttpProxy()
        {
            _webproxy = null;
        }
#else

        /// <summary>
        /// 作用于 SenparcHttpClient 的 WebProxy（需要在 AddSenparcGlobalServices 之前定义）
        /// </summary>
        public static IWebProxy SenparcHttpClientWebProxy { get; set; } = null;

        /// <summary>
        /// 设置Web代理
        /// </summary>
        /// <param name="host"></param>
        /// <param name="port"></param>
        /// <param name="username"></param>
        /// <param name="password"></param>
        public static void SetHttpProxy(string host, string port, string username, string password)
        {
            ICredentials cred;
            cred = new NetworkCredential(username, password);
            if (!string.IsNullOrEmpty(host))
            {
                SenparcHttpClientWebProxy = new CoreWebProxy(new Uri(host + ":" + port ?? "80"), cred);
            }
        }

        /// <summary>
        /// 清除Web代理状态
        /// </summary>
        public static void RemoveHttpProxy()
        {
            SenparcHttpClientWebProxy = null;
        }

        /// <summary>
        /// 从 Request.Body 中读取流，并复制到一个独立的 MemoryStream 对象中
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static Stream GetRequestMemoryStream(this HttpRequest request)
        {
#if NETSTANDARD2_1
            var syncIOFeature = request.HttpContext.Features.Get<IHttpBodyControlFeature>();

            if (syncIOFeature != null)
            {
                syncIOFeature.AllowSynchronousIO = true;
            }
#endif
            string body = new StreamReader(request.Body).ReadToEnd();
            byte[] requestData = Encoding.UTF8.GetBytes(body);
            Stream inputStream = new MemoryStream(requestData);
            return inputStream;
        }
#endif

        #endregion

        #region 私有方法


        /// <summary>
        /// 验证服务器证书
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="certificate"></param>
        /// <param name="chain"></param>
        /// <param name="errors"></param>
        /// <returns></returns>
        private static bool CheckValidationResult(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
        {
            return true;
        }

#if NET45
        /// <summary>
        /// 设置HTTP头
        /// </summary>
        /// <param name="request"></param>
        /// <param name="refererUrl"></param>
        /// <param name="useAjax">是否使用Ajax</param>
        /// <param name="headerAddition">header附加信息</param>
        /// <param name="timeOut"></param>
        private static void HttpClientHeader(HttpWebRequest request, string refererUrl, bool useAjax, Dictionary<string, string> headerAddition, int timeOut)
        {
            request.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8";
            request.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/31.0.1650.57 Safari/537.36";
            request.Timeout = timeOut;
            request.KeepAlive = true;

            if (string.IsNullOrEmpty(refererUrl))
            {
                request.Referer = refererUrl;
            }

            if (useAjax)
            {
                request.Headers.Add("X-Requested-With", "XMLHttpRequest");
            }

            if (headerAddition != null)
            {
                foreach (var item in headerAddition)
                {
                    request.Headers.Add(item.Key, item.Value);
                }
            }

        }
#else // NETSTANDARD2_0

        /// <summary>
        /// 验证服务器证书
        /// </summary>
        /// <param name="request"></param>
        /// <param name="certificate"></param>
        /// <param name="chain"></param>
        /// <param name="sslPolicyErrors"></param>
        /// <returns></returns>
        public static bool CheckValidationResult(HttpRequestMessage request, X509Certificate2 certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            if (sslPolicyErrors == SslPolicyErrors.None)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private static StreamContent CreateFileContent(Stream stream, string formName, string fileName, string contentType = "application/octet-stream")
        {
            //fileName = fileName.UrlEncode();
            var fileContent = new StreamContent(stream);
            //上传格式参考：
            //https://mp.weixin.qq.com/wiki?t=resource/res_main&id=mp1444738729
            //https://work.weixin.qq.com/api/doc#10112
            fileContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
            {
                Name = "\"{0}\"".FormatWith(formName),
                FileName = "\"" + fileName + "\"",
                Size = stream.Length
            }; // the extra quotes are key here
            fileContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);
            return fileContent;
        }

        //private static void HttpContentHeader(HttpContent hc, int timeOut)
        //{
        //    hc.Headers.Add("UserAgent", "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/61.0.3163.100 Safari/537.36");
        //    hc.Headers.Add("Timeout", timeOut.ToString());
        //    hc.Headers.Add("KeepAlive", "true");
        //}

        /// <summary>
        /// 设置HTTP头
        /// </summary>
        /// <param name="client"></param>
        /// <param name="refererUrl"></param>
        /// <param name="useAjax">是否使用Ajax</param>
        /// <param name="headerAddition">header附加信息</param>
        /// <param name="timeOut"></param>
        private static void HttpClientHeader(HttpClient client, string refererUrl, bool useAjax, Dictionary<string, string> headerAddition = null, int timeOut = Config.TIME_OUT)
        {
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/html"));
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xhtml+xml"));
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xml", 0.9));
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("image/webp"));
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*", 0.8));

            //HttpContent hc = new StringContent(null);
            //HttpContentHeader(hc, timeOut);

            //httpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("Mozilla","5.0 (Windows NT 10.0; WOW64)"));
            //httpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("AppleWebKit", "537.36 (KHTML, like Gecko)"));
            //httpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("Chrome", "61.0.3163.100 Safari/537.36"));

            //httpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/61.0.3163.100 Safari/537.36"));

            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/61.0.3163.100 Safari/537.36");
            client.DefaultRequestHeaders.Add("Timeout", timeOut.ToString());
            client.DefaultRequestHeaders.Add("KeepAlive", "true");

            if (!string.IsNullOrEmpty(refererUrl))
            {
                client.DefaultRequestHeaders.Referrer = new Uri(refererUrl);
            }

            if (useAjax)
            {
                client.DefaultRequestHeaders.Add("X-Requested-With", "XMLHttpRequest");
            }

            if (headerAddition != null)
            {
                foreach (var item in headerAddition)
                {
                    client.DefaultRequestHeaders.Add(item.Key, item.Value);
                }
            }
        }

#endif

        #endregion

        #region 同步方法

        /// <summary>
        /// 填充表单信息的Stream
        /// </summary>
        /// <param name="formData"></param>
        /// <param name="stream"></param>
        public static void FillFormDataStream(this Dictionary<string, string> formData, Stream stream)
        {
            string dataString = GetQueryString(formData);
            var formDataBytes = formData == null ? new byte[0] : Encoding.UTF8.GetBytes(dataString);
            stream.Write(formDataBytes, 0, formDataBytes.Length);
            stream.Seek(0, SeekOrigin.Begin);//设置指针读取位置
        }

        #endregion

#if !NET35 && !NET40
        #region 异步方法

        /// <summary>
        /// 填充表单信息的Stream
        /// </summary>
        /// <param name="formData"></param>
        /// <param name="stream"></param>
        public static async Task FillFormDataStreamAsync(this Dictionary<string, string> formData, Stream stream)
        {
            string dataString = GetQueryString(formData);
            var formDataBytes = formData == null ? new byte[0] : Encoding.UTF8.GetBytes(dataString);
            await stream.WriteAsync(formDataBytes, 0, formDataBytes.Length).ConfigureAwait(false);
            stream.Seek(0, SeekOrigin.Begin);//设置指针读取位置
        }

        #endregion
#endif

        #region 只需要使用同步的方法

        /// <summary>
        /// 组装QueryString的方法
        /// 参数之间用&amp;连接，首位没有符号，如：a=1&amp;b=2&amp;c=3
        /// </summary>
        /// <param name="formData"></param>
        /// <returns></returns>
        public static string GetQueryString(this Dictionary<string, string> formData)
        {
            if (formData == null || formData.Count == 0)
            {
                return "";
            }

            StringBuilder sb = new StringBuilder();

            var i = 0;
            foreach (var kv in formData)
            {
                i++;
                sb.AppendFormat("{0}={1}", kv.Key, Senparc.CO2NET.Extensions.WebCodingExtensions.UrlEncode(kv.Value));
                if (i < formData.Count)
                {
                    sb.Append("&");
                }
            }

            return sb.ToString();
        }
        #endregion
    }
}
