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

Detail: https://github.com/Senparc/Senparc.CO2NET/blob/master/LICENSE

----------------------------------------------------------------*/
#endregion Apache License Version 2.0

/*----------------------------------------------------------------
    Copyright (C) 2025 Senparc

    FileName：Get.cs
    File Function Description：Get


    Creation Identifier：Senparc - 20150211

    Modification Identifier：Senparc - 20150303
    Modification Description：Organize interface

    Modification Identifier：zeje - 20160422
    Modification Description：v4.5.19 Add maxJsonLength parameter to GetJson method

    Modification Identifier：zeje - 20170305
    Modification Description：MP v14.3.132 Add Get.DownloadAsync(string url, string dir) method

    Modification Identifier：Senparc - 20170409
    Modification Description：v4.11.9 Modify Download method

    Modification Identifier：Senparc - 20171101
    Modification Description：v4.18.1 Modify Get.Download() method

    Modification Identifier：Senparc - 20180114
    Modification Description：v4.18.13 Modify HttpUtility.Get.Download() method,
                        Store file according to filename in Content-Disposition

    Modification Identifier：Senparc - 20180407
    Modification Description：v14.10.13 Optimize Get.Download() method, improve judgment of FileName

    Modification Identifier：Senparc - 20190429
    Modification Description：v0.7.0 Optimize HttpClient, refactor RequestUtility (including Post and Get), introduce HttpClientFactory mechanism

    Modification Identifier：Senparc - 20200925
    Modification Description：v1.3.201 Update Senparc.CO2NET.HttpUtility.Get.Download() method, fix filename judgment regex

    Modification Identifier：Senparc - 20210606
    Modification Description：v1.4.400 Fix bug in Download method

    Modification Identifier：554393109 - 20220208
    Modification Description：v2.0.3 Modify HttpClient request timeout implementation

----------------------------------------------------------------*/


using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text;
using Senparc.CO2NET.Helpers;
#if NET462
using System.Web.Script.Serialization;
#else
using Microsoft.Extensions.DependencyInjection;
#endif


//using Senparc.CO2NET.Entities;
//using Senparc.CO2NET.Exceptions;
using System.Text.RegularExpressions;

namespace Senparc.CO2NET.HttpUtility
{
    /// <summary>
    /// Get request processing
    /// </summary>
    public static class Get
    {
        /// <summary>
        /// Get random file name
        /// </summary>
        /// <returns></returns>
        private static string GetRandomFileName()
        {
            return SystemTime.Now.ToString("yyyyMMdd-HHmmss") + Guid.NewGuid().ToString("n").Substring(0, 6);
        }

        #region Synchronous Methods

        /// <summary>
        /// GET request URL and return type T
        /// </summary>
        /// <typeparam name="T">Type of data received in JSON</typeparam>
        /// <param name="serviceProvider">Server provider under .NetCore, if .NET Framework then keep null</param>
        /// <param name="url"></param>
        /// <param name="encoding"></param>
        /// <param name="afterReturnText">Return JSON text and trigger before serialization, parameters are: url, returnText</param>
        /// <returns></returns>
        public static T GetJson<T>(
            IServiceProvider serviceProvider,
            string url, Encoding encoding = null, Action<string, string> afterReturnText = null)
        {
            string returnText = RequestUtility.HttpGet(
                 serviceProvider,
                 url, encoding);

            afterReturnText?.Invoke(url, returnText);

            T result = SerializerHelper.GetObject<T>(returnText);

            return result;
        }

        /// <summary>
        /// Download from URL
        /// </summary>
        /// <param name="serviceProvider">Server provider under .NetCore, if .NET Framework then keep null</param>
        /// <param name="url"></param>
        /// <param name="stream"></param>
        public static void Download(
            IServiceProvider serviceProvider,
            string url, Stream stream)
        {
#if NET462
            //ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3
            //ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(CheckValidationResult);

            WebClient wc = new WebClient();
            var data = wc.DownloadData(url);
            stream.Write(data, 0, data.Length);
            //foreach (var b in data)
            //{
            //    stream.WriteByte(b);
            //}
#else
            HttpClient httpClient = serviceProvider.GetRequiredService<SenparcHttpClient>().Client;
            var t = httpClient.GetByteArrayAsync(url);
            t.Wait();
            var data = t.Result;
            stream.Write(data, 0, data.Length);
#endif
        }

        /// <summary>
        /// Download from URL and save to specified directory
        /// </summary>
        /// <param name="serviceProvider">Server provider under .NetCore, if .NET Framework then keep null</param>
        /// <param name="url">URL of the file to be downloaded</param>
        /// <param name="filePathName">Path to save the file, if the downloaded file contains a filename, store it according to the filename, otherwise a random filename with Ticks will be assigned</param>
        /// <param name="timeOut">Timeout</param>
        /// <returns></returns>
        public static string Download(IServiceProvider serviceProvider, string url, string filePathName, int timeOut = Config.TIME_OUT)
        {
            var dir = Path.GetDirectoryName(filePathName) ?? "/";
            Directory.CreateDirectory(dir);

#if NET462

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "GET";
            request.Timeout = timeOut;

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            using (Stream responseStream = response.GetResponseStream())
            {
                string responseFileName = null;
                //e.g.: content-disposition: inline; filename="WeChatSampleBuilder-2.0.0.zip"; filename*=utf-8''WeChatSampleBuilder-2.0.0.zip
                var contentDescriptionHeader = response.GetResponseHeader("Content-Disposition");

                if (!string.IsNullOrEmpty(contentDescriptionHeader))
                {
                    var fileName = Regex.Match(contentDescriptionHeader, @"(?<=filename="")([\s\S]+)(?="")", RegexOptions.IgnoreCase).Value;

                    responseFileName = Path.Combine(dir, fileName);
                }

                var fullName = responseFileName ?? Path.Combine(dir, GetRandomFileName());

                using (var fs = File.Open(fullName, FileMode.OpenOrCreate))
                {
                    byte[] bArr = new byte[1024];
                    int size = responseStream.Read(bArr, 0, (int)bArr.Length);
                    while (size > 0)
                    {
                        fs.Write(bArr, 0, size);
                        fs.Flush();
                        size = responseStream.Read(bArr, 0, (int)bArr.Length);
                    }

                }

                return fullName;
            }

#else
            System.Net.Http.HttpClient httpClient = serviceProvider.GetRequiredService<SenparcHttpClient>().Client;
            using (var cts = new System.Threading.CancellationTokenSource(timeOut))
            {
                try
                {
                    using (var responseMessage = httpClient.GetAsync(url, cancellationToken: cts.Token).Result)
                    {
                        if (responseMessage.StatusCode == HttpStatusCode.OK)
                        {
                            string responseFileName = null;
                            //ContentDisposition may be null
                            if (responseMessage.Content.Headers.ContentDisposition != null &&
                                responseMessage.Content.Headers.ContentDisposition.FileName != null &&
                                responseMessage.Content.Headers.ContentDisposition.FileName != "\"\"")
                            {
                                responseFileName = Path.Combine(dir, responseMessage.Content.Headers.ContentDisposition.FileName.Trim('"'));
                            }

                            var fullName = responseFileName ?? Path.Combine(dir, GetRandomFileName());
                            using (var fs = File.Open(fullName, FileMode.Create))
                            {
                                using (var responseStream = responseMessage.Content.ReadAsStreamAsync().Result)
                                {
                                    responseStream.CopyTo(fs);
                                    fs.Flush();
                                }
                            }
                            return fullName;

                        }
                        else
                        {
                            return null;
                        }
                    }
                }
                catch { throw; }
            }
#endif
        }

        #endregion

        #region Asynchronous Methods

        /// <summary>
        /// [Async Method] Async GetJson
        /// </summary>
        /// <param name="serviceProvider">Server provider under .NetCore, if .NET Framework then keep null</param>
        /// <param name="url"></param>
        /// <param name="encoding"></param>
        /// <param name="afterReturnText">Return JSON text and trigger before serialization, parameters are: url, returnText</param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <exception cref="ErrorJsonResultException"></exception>
        public static async Task<T> GetJsonAsync<T>(
            IServiceProvider serviceProvider,
            string url, Encoding encoding = null, Action<string, string> afterReturnText = null)
        {
            string returnText = await RequestUtility.HttpGetAsync(
                 serviceProvider,
                 url, encoding).ConfigureAwait(false);

            afterReturnText?.Invoke(url, returnText);

            T result = SerializerHelper.GetObject<T>(returnText);

            return result;
        }

        /// <summary>
        /// [Async Method] Async download from URL
        /// </summary>
        /// <param name="serviceProvider">Server provider under .NetCore, if .NET Framework then keep null</param>
        /// <param name="url"></param>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static async Task DownloadAsync(
            IServiceProvider serviceProvider,
            string url, Stream stream)
        {
#if NET462
            //ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3
            //ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(CheckValidationResult);

            WebClient wc = new WebClient();
            var data = await wc.DownloadDataTaskAsync(url).ConfigureAwait(false);
            await stream.WriteAsync(data, 0, data.Length).ConfigureAwait(false);
            //foreach (var b in data)
            //{
            //    stream.WriteAsync(b);
            //}
#else
            using (HttpClient httpClient = serviceProvider.GetRequiredService<SenparcHttpClient>().Client)
            {
                var data = await httpClient.GetByteArrayAsync(url).ConfigureAwait(false);
                await stream.WriteAsync(data, 0, data.Length).ConfigureAwait(false);
            }
#endif
        }

        /// <summary>
        /// [Async Method] Download from URL and save to specified directory
        /// </summary>
        /// <param name="serviceProvider">Server provider under .NetCore, if .NET Framework then keep null</param>
        /// <param name="url">URL of the file to be downloaded</param>
        /// <param name="filePathName">Path to save the file, if the downloaded file contains a filename, store it according to the filename, otherwise a random filename with Ticks will be assigned</param>
        /// <param name="timeOut">Timeout</param>
        /// <returns></returns>
        public static async Task<string> DownloadAsync(
            IServiceProvider serviceProvider,
            string url, string filePathName, int timeOut = Config.TIME_OUT)
        {
            var dir = Path.GetDirectoryName(filePathName) ?? "/";
            Directory.CreateDirectory(dir);

#if NET462
            System.Net.Http.HttpClient httpClient = new HttpClient();
#else
            System.Net.Http.HttpClient httpClient = serviceProvider.GetRequiredService<SenparcHttpClient>().Client;
#endif

            using (httpClient)
            {
                //httpClient.Timeout = TimeSpan.FromMilliseconds(timeOut);  // It is recommended not to directly modify the Timeout property of httpClient here, as this is a globally shared value for the Client and will affect the timeout of other requests under the same Client instance
                // Microsoft technical documentation original link【https://docs.microsoft.com/zh-cn/dotnet/api/system.net.http.httpclient.timeout?f1url=%3FappId%3DDev16IDEF1%26l%3DZH-CN%26k%3Dk(System.Net.Http.HttpClient.Timeout);k(DevLang-csharp)%26rd%3Dtrue&view=net-6.0】
                // The document mentions "All requests using this instance will use the same timeout value HttpClient. You can also set a different timeout for a single request using CancellationTokenSource on the task."
                using (var cts = new System.Threading.CancellationTokenSource(timeOut))
                {
                    try
                    {
                        var responseMessage = await httpClient.GetAsync(url, cancellationToken: cts.Token).ConfigureAwait(false);

                        using (responseMessage)
                        {
                            if (responseMessage.StatusCode == HttpStatusCode.OK)
                            {
                                string responseFileName = null;
                                //ContentDisposition may be null
                                if (responseMessage.Content.Headers.ContentDisposition != null &&
                                    responseMessage.Content.Headers.ContentDisposition.FileName != null &&
                                    responseMessage.Content.Headers.ContentDisposition.FileName != "\"\"")
                                {
                                    responseFileName = Path.Combine(dir, responseMessage.Content.Headers.ContentDisposition.FileName.Trim('"'));
                                }

                                var fullName = responseFileName ?? Path.Combine(dir, GetRandomFileName());
                                using (var fs = File.Open(fullName, FileMode.Create))
                                {
                                    using (var responseStream = await responseMessage.Content.ReadAsStreamAsync().ConfigureAwait(false))
                                    {
                                        await responseStream.CopyToAsync(fs).ConfigureAwait(false);
                                        await fs.FlushAsync().ConfigureAwait(false);
                                    }
                                }
                                return fullName;
                            }
                            else
                            {
                                return null;
                            }
                        }
                    }
                    catch { throw; }
                }
            }
        }
        #endregion

    }
}
