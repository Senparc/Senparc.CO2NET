#region Apache License Version 2.0
/*----------------------------------------------------------------

Copyright 2023 Suzhou Senparc Network Technology Co.,Ltd.

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
    Copyright (C) 2024 Senparc

    FileName：RequestUtility.Get.cs
    File Function Description：Get request result


    Creation Identifier：Senparc - 20171006

    Modification Description：Ported Get method

    Modification Identifier：Senparc - 20190429
    Modification Description：v0.7.0 Optimized HttpClient, refactored RequestUtility (including Post and Get), introduced HttpClientFactory mechanism

    Modification Identifier：Senparc - 20200530
    Modification Description：v1.3.108 Added headerAddition parameter to RequestUtility.Get method
              v1.3.109 Added HttpResponseGetAsync

    Modification Identifier：554393109 - 20220208
    Modification Description：v2.0.3 Modified HttpClient request timeout implementation

    Modification Identifier：Senparc - 20230711
    Modification Description：v2.2.1 Optimized Http request, timely release of resources

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
#if NET462
using System.Web;
#else
using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.Extensions.DependencyInjection;
#endif
#if !NET462
using Senparc.CO2NET.WebProxy;
#endif

namespace Senparc.CO2NET.HttpUtility
{
    /// <summary>
    /// HTTP request utility class
    /// </summary>
    public static partial class RequestUtility
    {
        #region 公用静态方法

#if NET462
        /// <summary>
        /// HttpWebRequest parameter settings for .NET 4.5 version
        /// </summary>
        /// <returns></returns>
        private static HttpWebRequest HttpGet_Common_Net45(string url, CookieContainer cookieContainer = null, Encoding encoding = null, X509Certificate2 cer = null,
            string refererUrl = null, bool useAjax = false, int timeOut = Config.TIME_OUT)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "GET";
            request.Timeout = timeOut;
            request.Proxy = _webproxy;
            if (cer != null)
            {
                request.ClientCertificates.Add(cer);
            }

            if (cookieContainer != null)
            {
                request.CookieContainer = cookieContainer;
            }

            HttpClientHeader(request, refererUrl, useAjax, null, timeOut);//Set header information

            return request;
        }
#endif

#if !NET462
        /// <summary>
        /// HttpWebRequest parameter settings for .NET Core version
        /// </summary>
        /// <returns></returns>
        private static HttpClient HttpGet_Common_NetCore(IServiceProvider serviceProvider, string url, CookieContainer cookieContainer = null,
            Encoding encoding = null, X509Certificate2 cer = null,
            string refererUrl = null, bool useAjax = false, Dictionary<string, string> headerAddition = null, int timeOut = Config.TIME_OUT)
        {
            var handler = HttpClientHelper.GetHttpClientHandler(cookieContainer, RequestUtility.SenparcHttpClientWebProxy, DecompressionMethods.GZip);

            if (cer != null)
            {
                handler.ClientCertificates.Add(cer);
            }

            HttpClient httpClient = serviceProvider.GetRequiredService<SenparcHttpClient>().Client;
            HttpClientHeader(httpClient, refererUrl, useAjax, headerAddition, timeOut);

            return httpClient;
        }
#endif

        #endregion

        #region 同步方法

        /// <summary>
        /// Get string result using Get method (without Cookie)
        /// </summary>
        /// <param name="serviceProvider">Server provider under .NetCore, keep null if .NET Framework</param>
        /// <param name="url"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public static string HttpGet(
            IServiceProvider serviceProvider,
            string url, Encoding encoding = null)
        {
#if NET462
            WebClient wc = new WebClient();
            wc.Proxy = _webproxy;
            wc.Encoding = encoding ?? Encoding.UTF8;
            return wc.DownloadString(url);
#else
            var handler = HttpClientHelper.GetHttpClientHandler(null, SenparcHttpClientWebProxy, DecompressionMethods.GZip);

            using (HttpClient httpClient = serviceProvider.GetRequiredService<SenparcHttpClient>().Client)
            {
                return httpClient.GetStringAsync(url).Result;
            }
#endif
        }

        /// <summary>
        /// Get string result using Get method (with Cookie)
        /// </summary>
        /// <param name="serviceProvider">Server provider under .NetCore, keep null if .NET Framework</param>
        /// <param name="url"></param>
        /// <param name="cookieContainer"></param>
        /// <param name="encoding"></param>
        /// <param name="cer">Certificate, keep null if not needed</param>
        /// <param name="refererUrl">Referer parameter</param>
        /// <param name="useAjax">Whether to use Ajax</param>
        /// <param name="headerAddition"></param>
        /// <param name="timeOut"></param>
        /// <returns></returns>
        public static string HttpGet(
            IServiceProvider serviceProvider,
            string url, CookieContainer cookieContainer = null, Encoding encoding = null, X509Certificate2 cer = null,
            string refererUrl = null, bool useAjax = false, Dictionary<string, string> headerAddition = null, int timeOut = Config.TIME_OUT)
        {
#if NET462
            HttpWebRequest request = HttpGet_Common_Net45(url, cookieContainer, encoding, cer, refererUrl, useAjax, timeOut);

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            if (cookieContainer != null)
            {
                response.Cookies = cookieContainer.GetCookies(response.ResponseUri);
            }

            using (Stream responseStream = response.GetResponseStream())
            {
                using (StreamReader myStreamReader = new StreamReader(responseStream, encoding ?? Encoding.GetEncoding("utf-8")))
                {
                    string retString = myStreamReader.ReadToEnd();
                    return retString;
                }
            }
#else

            var httpClient = HttpGet_Common_NetCore(serviceProvider, url, cookieContainer, encoding, cer, refererUrl, useAjax, headerAddition, timeOut);

            using (httpClient)
            {
                using (var cts = new System.Threading.CancellationTokenSource(timeOut))
                {
                    try
                    {
                        var response = httpClient.GetAsync(url, cancellationToken: cts.Token).GetAwaiter().GetResult();//Get response information
                        using (response)
                        {
                            HttpClientHelper.SetResponseCookieContainer(cookieContainer, response);//Set Cookie

                            return response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                        }
                    }
                    catch { throw; }
                }
            }
#endif
        }

#if NET462

        /// <summary>
        /// Get HttpWebResponse or HttpResponseMessage object, this method is usually used for testing
        /// </summary>
        /// <param name="url"></param>
        /// <param name="cookieContainer"></param>
        /// <param name="encoding"></param>
        /// <param name="cer"></param>
        /// <param name="refererUrl"></param>
        /// <param name="useAjax"></param>
        /// <param name="timeOut"></param>
        /// <returns></returns>
        public static HttpWebResponse HttpResponseGet(string url, CookieContainer cookieContainer = null, Encoding encoding = null, X509Certificate2 cer = null,
    string refererUrl = null, bool useAjax = false, int timeOut = Config.TIME_OUT)
        {
            HttpWebRequest request = HttpGet_Common_Net45(url, cookieContainer, encoding, cer, refererUrl, useAjax, timeOut);

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            if (cookieContainer != null)
            {
                response.Cookies = cookieContainer.GetCookies(response.ResponseUri);
            }

            return response;
        }
#else
        /// <summary>
        /// Get HttpWebResponse or HttpResponseMessage object, this method is usually used for testing
        /// </summary>
        /// <param name="serviceProvider">Server provider for NetCore</param>
        /// <param name="url"></param>
        /// <param name="cookieContainer"></param>
        /// <param name="encoding"></param>
        /// <param name="cer"></param>
        /// <param name="refererUrl"></param>
        /// <param name="useAjax">Whether to use Ajax request</param>
        /// <param name="headerAddition"></param>
        /// <param name="timeOut"></param>
        /// <returns></returns>
        public static HttpResponseMessage HttpResponseGet(
            IServiceProvider serviceProvider,
            string url, CookieContainer cookieContainer = null, Encoding encoding = null, X509Certificate2 cer = null,
            string refererUrl = null, bool useAjax = false, Dictionary<string, string> headerAddition = null, int timeOut = Config.TIME_OUT)
        {
            var httpClient = HttpGet_Common_NetCore(serviceProvider, url, cookieContainer, encoding, cer, refererUrl, useAjax, headerAddition, timeOut);
            using (var cts = new System.Threading.CancellationTokenSource(timeOut))
            {
                try
                {
                    var task = httpClient.GetAsync(url, cancellationToken: cts.Token);
                    HttpResponseMessage response = task.Result;

                    HttpClientHelper.SetResponseCookieContainer(cookieContainer, response);//Set Cookie

                    return response;
                }
                catch { throw; }
            }
        }

#endif


        #endregion

        #region 异步方法

        /// <summary>
        /// Get string result using Get method (without Cookie)
        /// </summary>
        /// <param name="serviceProvider">Server provider under .NetCore, keep null if .NET Framework</param>
        /// <param name="url"></param>
        /// <returns></returns>
        public static async Task<string> HttpGetAsync(
            IServiceProvider serviceProvider,
            string url, Encoding encoding = null)
        {
#if NET462
            WebClient wc = new WebClient();
            wc.Proxy = _webproxy;
            wc.Encoding = encoding ?? Encoding.UTF8;
            return await wc.DownloadStringTaskAsync(url).ConfigureAwait(false);
#else
            var handler = new HttpClientHandler
            {
                UseProxy = SenparcHttpClientWebProxy != null,
                Proxy = SenparcHttpClientWebProxy,
            };

            HttpClient httpClient = serviceProvider.GetRequiredService<SenparcHttpClient>().Client;
            using (httpClient)
            {
                return await httpClient.GetStringAsync(url).ConfigureAwait(false);
            }
#endif

        }

        /// <summary>
        /// Get string result using Get method (with Cookie)
        /// </summary>
        /// <param name="serviceProvider">Server provider under .NetCore, keep null if .NET Framework</param>
        /// <param name="url"></param>
        /// <param name="cookieContainer"></param>
        /// <param name="encoding"></param>
        /// <param name="cer">Certificate, keep null if not needed</param>
        /// <param name="timeOut"></param>
        /// <param name="refererUrl">Referer parameter</param>
        /// <param name="useAjax"></param>
        /// <param name="headerAddition"></param>
        /// <returns></returns>
        public static async Task<string> HttpGetAsync(
            IServiceProvider serviceProvider,
            string url, CookieContainer cookieContainer = null, Encoding encoding = null, X509Certificate2 cer = null,
            string refererUrl = null, bool useAjax = false, Dictionary<string, string> headerAddition = null, int timeOut = Config.TIME_OUT)
        {
#if NET462
            HttpWebRequest request = HttpGet_Common_Net45(url, cookieContainer, encoding, cer, refererUrl, useAjax, timeOut);

            HttpWebResponse response = (HttpWebResponse)(await request.GetResponseAsync().ConfigureAwait(false));

            if (cookieContainer != null)
            {
                response.Cookies = cookieContainer.GetCookies(response.ResponseUri);
            }

            using (Stream responseStream = response.GetResponseStream())
            {
                using (StreamReader myStreamReader = new StreamReader(responseStream, encoding ?? Encoding.GetEncoding("utf-8")))
                {
                    string retString = await myStreamReader.ReadToEndAsync().ConfigureAwait(false);
                    return retString;
                }
            }
#else
            var httpClient = HttpGet_Common_NetCore(serviceProvider, url, cookieContainer, encoding, cer, refererUrl, useAjax, headerAddition, timeOut);

            using (httpClient)
            {
                using (var cts = new System.Threading.CancellationTokenSource(timeOut))
                {
                    try
                    {
                        var response = await httpClient.GetAsync(url, cancellationToken: cts.Token).ConfigureAwait(false);//Get response information

                        using (response)
                        {
                            HttpClientHelper.SetResponseCookieContainer(cookieContainer, response);//Set Cookie

                            var retString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                            return retString;
                        }

                    }
                    catch { throw; }
                }
            }
#endif
        }

#if NET462

        /// <summary>
        /// Get HttpWebResponse or HttpResponseMessage object, this method is usually used for testing
        /// </summary>
        /// <param name="url"></param>
        /// <param name="cookieContainer"></param>
        /// <param name="encoding"></param>
        /// <param name="cer"></param>
        /// <param name="refererUrl"></param>
        /// <param name="useAjax"></param>
        /// <param name="timeOut"></param>
        /// <returns></returns>
        public static async Task<HttpWebResponse> HttpResponseGetAsync(string url, CookieContainer cookieContainer = null, Encoding encoding = null, X509Certificate2 cer = null,
    string refererUrl = null, bool useAjax = false, int timeOut = Config.TIME_OUT)
        {
            HttpWebRequest request = HttpGet_Common_Net45(url, cookieContainer, encoding, cer, refererUrl, useAjax, timeOut);

            HttpWebResponse response =  (HttpWebResponse)(await request.GetResponseAsync().ConfigureAwait(false));

            if (cookieContainer != null)
            {
                response.Cookies = cookieContainer.GetCookies(response.ResponseUri);
            }

            return response;
        }
#else
        /// <summary>
        /// Get HttpWebResponse or HttpResponseMessage object, this method is usually used for testing
        /// </summary>
        /// <param name="serviceProvider">Server provider for NetCore</param>
        /// <param name="url"></param>
        /// <param name="cookieContainer"></param>
        /// <param name="encoding"></param>
        /// <param name="cer"></param>
        /// <param name="refererUrl"></param>
        /// <param name="useAjax">Whether to use Ajax request</param>
        /// <param name="headerAddition"></param>
        /// <param name="timeOut"></param>
        /// <returns></returns>
        public static async Task<HttpResponseMessage> HttpResponseGetAsync(
            IServiceProvider serviceProvider,
            string url, CookieContainer cookieContainer = null, Encoding encoding = null, X509Certificate2 cer = null,
            string refererUrl = null, bool useAjax = false, Dictionary<string, string> headerAddition = null, int timeOut = Config.TIME_OUT)
        {
            var httpClient = HttpGet_Common_NetCore(serviceProvider, url, cookieContainer, encoding, cer, refererUrl, useAjax, headerAddition, timeOut);
            using (var cts = new System.Threading.CancellationTokenSource(timeOut))
            {
                try
                {
                    var task = httpClient.GetAsync(url, cancellationToken: cts.Token);
                    HttpResponseMessage response = await task;

                    HttpClientHelper.SetResponseCookieContainer(cookieContainer, response);//Set Cookie

                    return response;
                }
                catch { throw; }
            }
        }

#endif

        #endregion
    }
}
