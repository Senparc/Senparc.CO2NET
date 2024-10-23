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

    FileName: RequestUtility.cs
    File Function Description: Get request result


    Creation Identifier: Senparc - 20150211

    Modification Description: Organize interface

    Modification Identifier: Senparc - 20150407
    Modification Description: Use Post method to get string result, modify form handling method

    Modification Identifier: Senparc - 20170122
    Modification Description: v4.9.14 Add null check for AsUrlData method

    Modification Identifier: Senparc - 20170122
    Modification Description: v4.12.2 Fix HttpUtility.UrlEncode method error

    Modification Identifier: Senparc - 20170730
    Modification Description: v4.13.3 Add Accept, UserAgent, KeepAlive settings for RequestUtility.HttpGet() method

    Modification Identifier: Senparc - 20180516
    Modification Description: v4.21.0 Support .NET Core 2.1.0-rc1-final, add compilation conditions

    Modification Identifier: Senparc - 20180518
    Modification Description: v4.21.0 Support .NET Core 2.1.0-rc1-final, add compilation conditions

    -- CO2NET --

    Modification Identifier: Senparc - 20181009
    Modification Description: v0.2.15 Add headerAddition parameter to Post method

    Modification Identifier: Senparc - 20181215
    Modification Description: v0.3.1 Update RequestUtility.GetQueryString() method

    Modification Identifier: Senparc - 20190429
    Modification Description: v0.7.0 Optimize HttpClient, refactor RequestUtility (including Post and Get), introduce HttpClientFactory mechanism

    Modification Identifier: Senparc - 20190521
    Modification Description: v0.7.3 .NET Core provides multi-certificate registration function

    Modification Identifier: Senparc - 20190521
    Modification Description: v0.8.4 Remove UrlEncode encoding for fileName parameter in HttpUtility.HttpPost_Common_NetCore's CreateFileContent

    Modification Identifier: Senparc - 20190928
    Modification Description: v1.0.101 Add AllowSynchronousIO setting for .NET Core 3.0 in RequestUtility.GetRequestMemoryStream()

    -- Migrated from CO2NET to CO2NET.AspNet --
    
    Modification Identifier: Senparc - 20180721
    Modification Description: v0.1.0 Migrated from CO2NET to CO2NET.AspNet
    
    Modification Identifier: Senparc - 20210501
    Modification Description: v0.4.300.4 Provide GetRequestMemoryStreamAsync() asynchronous method


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

#if NET462
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
    /// HTTP request utility class
    /// </summary>
    public static partial class RequestUtility
    {
        #region 代理

#if NET462
        private static System.Net.WebProxy _webproxy = null;
        /// <summary>
        /// Set Web proxy
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
        /// Clear Web proxy status
        /// </summary>
        public static void RemoveHttpProxy()
        {
            _webproxy = null;
        }
#else

        /// <summary>
        /// WebProxy for SenparcHttpClient (needs to be defined before AddSenparcGlobalServices)
        /// </summary>
        public static IWebProxy SenparcHttpClientWebProxy { get; set; } = null;

        /// <summary>
        /// Set Web proxy
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
        /// Clear Web proxy status
        /// </summary>
        public static void RemoveHttpProxy()
        {
            SenparcHttpClientWebProxy = null;
        }

        /// <summary>
        /// Read stream from Request.Body and copy to a separate MemoryStream object
        /// </summary>
        /// <param name="request"></param>
        /// <param name="allowSynchronousIO"></param>
        /// <returns></returns>
        public static Stream GetRequestMemoryStream(this HttpRequest request, bool? allowSynchronousIO = true)
        {
#if !NET462 
            var syncIOFeature = request.HttpContext.Features.Get<IHttpBodyControlFeature>();

            if (syncIOFeature != null && allowSynchronousIO.HasValue)
            {
                syncIOFeature.AllowSynchronousIO = allowSynchronousIO.Value;
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
        /// Validate server certificate
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

#if NET462
        /// <summary>
        /// Set HTTP headers
        /// </summary>
        /// <param name="request"></param>
        /// <param name="refererUrl"></param>
        /// <param name="useAjax">Whether to use Ajax</param>
        /// <param name="headerAddition">Header additional information</param>
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
        /// Validate server certificate
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
        /// Set HTTP headers
        /// </summary>
        /// <param name="client"></param>
        /// <param name="refererUrl"></param>
        /// <param name="useAjax">Whether to use Ajax</param>
        /// <param name="headerAddition">Header additional information</param>
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
        /// Fill form information Stream
        /// </summary>
        /// <param name="formData"></param>
        /// <param name="stream"></param>
        public static void FillFormDataStream(this Dictionary<string, string> formData, Stream stream)
        {
            string dataString = GetQueryString(formData);
            var formDataBytes = formData == null ? new byte[0] : Encoding.UTF8.GetBytes(dataString);
            stream.Write(formDataBytes, 0, formDataBytes.Length);
            stream.Seek(0, SeekOrigin.Begin);//Set pointer read position
        }

        #endregion

        #region 异步方法

        /// <summary>
        /// Fill form information Stream
        /// </summary>
        /// <param name="formData"></param>
        /// <param name="stream"></param>
        public static async Task FillFormDataStreamAsync(this Dictionary<string, string> formData, Stream stream)
        {
            string dataString = GetQueryString(formData);
            var formDataBytes = formData == null ? new byte[0] : Encoding.UTF8.GetBytes(dataString);
            await stream.WriteAsync(formDataBytes, 0, formDataBytes.Length).ConfigureAwait(false);
            stream.Seek(0, SeekOrigin.Begin);//Set pointer read position
        }

#if !NET462
        /// <summary>
        /// [Asynchronous method] Read stream from Request.Body and copy to a separate MemoryStream object
        /// </summary>
        /// <param name="request"></param>
        /// <param name="allowSynchronousIO"></param>
        /// <returns></returns>
        public static async Task<Stream> GetRequestMemoryStreamAsync(this HttpRequest request, bool? allowSynchronousIO = true)
        {
#if NETSTANDARD2_1
            var syncIOFeature = request.HttpContext.Features.Get<IHttpBodyControlFeature>();

            if (syncIOFeature != null && allowSynchronousIO.HasValue)
            {
                syncIOFeature.AllowSynchronousIO = allowSynchronousIO.Value;
            }
#endif
            string body = await new StreamReader(request.Body).ReadToEndAsync();
            byte[] requestData = Encoding.UTF8.GetBytes(body);
            Stream inputStream = new MemoryStream(requestData);
            return inputStream;
        }
#endif
        #endregion

        #region 只需要使用同步的方法

        /// <summary>
        /// Method to assemble QueryString
        /// Parameters are connected with &amp;, no symbol at the beginning, e.g., a=1&amp;b=2&amp;c=3
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
