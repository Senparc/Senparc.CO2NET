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
    Copyright (C) 2026 Senparc

    Filename: RequestUtility.Post.cs

    Description: Retrieve request result (Post)

    Creation Identifier: Senparc - 20180602

    Modification Description: Ported the Post method
    Modification Identifier: Senparc - 20180516
    Modification Description: v4.21.1-rc1 Resolved the issue of RequestUtility.HttpResponsePost() and HttpPostAsync() methods not closing postStream in .NET Core promptly

    Modification Identifier: Senparc - 20180602
    Modification Description: v4.22.2 Improved the string information submission process in RequestUtility.HttpPost_Common_NetCore()
    -- CO2NET --

    Modification Identifier: Senparc - 20181009
    Modification Description: v0.2.15 Added headerAddition parameter to the Post method

    Modification Identifier: Senparc - 20190429
    Modification Description: v0.7.0 Optimized HttpClient, refactored RequestUtility (including Post and Get), introduced HttpClientFactory mechanism

    Modification Identifier: Senparc - 20190521
    Modification Description: v0.7.3 .NET Core provides multi-certificate registration feature

    Modification Identifier: Senparc - 20190811
    Modification Description: v0.8.7 Added new feature to RequestUtility.Post() method: simulating Form submission using file stream

    Modification Identifier: 554393109 - 20220208
    Modification Description: v2.0.3 Modified the implementation method of HttpClient request timeout

    Modification Identifier: Senparc - 20220721
    Modification Description: v2.1.2 Refactored RequestUtility, changed HttpPost_Common_NetCore() to an asynchronous method: HttpPost_Common_NetCoreAsync()

    Modification Identifier: Senparc - 20221115
    Modification Description: v2.1.3 Optimized simulated Form submission

    Modification Identifier: Senparc - 20230128
    Modification Description: v2.1.7.3 Continued to resolve the exception “The value cannot be null or empty. (Parameter 'mediaType')” caused by the previous version upgrade

    Modification Identifier: Senparc - 20230711
    Modification Description: v2.2.1 Optimized Http request, promptly closed resources

    Modification Identifier: Senparc - 20241119
    Modification Description: v3.0.0-beta3 Added ApiClient parameter

    Modification Identifier: Senparc - 20260722
    Modification Description: v4.1.0 Requests without cookies on .NET 8+ no longer create a non-reusable CookieContainer

----------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Senparc.CO2NET.Helpers;
using Senparc.CO2NET.Utilities.HttpUtility.HttpPost;
using Senparc.CO2NET.Extensions;

#if NET462
using System.Web;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
#else
using System.Net.Http;
using System.Net.Http.Headers;
using Senparc.CO2NET.WebProxy;
using Senparc.CO2NET.Exceptions;
using System.Linq;
#endif

namespace Senparc.CO2NET.HttpUtility
{ 
    /// <summary>
    /// HTTP request utility class
    /// </summary>
    public static partial class RequestUtility
    {
        #region Static Public Methods

#if NET462

        /// <summary>
        /// Common HttpPost request setup method for .NET Framework
        /// </summary>
        /// <param name="url"></param>
        /// <param name="method">Request method, such as POST/GET, etc.</param>
        /// <param name="cookieContainer"></param>
        /// <param name="postStream"></param>
        /// <param name="fileDictionary">Files to upload; Key: upload Name, Value: local file path or Base64-encoded file content</param>
        /// <param name="refererUrl"></param>
        /// <param name="encoding"></param>
        /// <param name="cer"></param>
        /// <param name="useAjax"></param>
        /// <param name="headerAddition">Additional header information</param>
        /// <param name="timeOut"></param>
        /// <param name="checkValidationResult"></param>
        /// <param name="contentType"></param>
        /// <returns></returns>
        public static HttpWebRequest HttpPost_Common_Net45(string url, string method, CookieContainer cookieContainer = null,
            Stream postStream = null, Dictionary<string, string> fileDictionary = null, string refererUrl = null,
            Encoding encoding = null, X509Certificate2 cer = null, bool useAjax = false,
            Dictionary<string, string> headerAddition = null,
            bool hasFormData = false,
            int timeOut = Config.TIME_OUT, bool checkValidationResult = false, string contentType = HttpClientHelper.DEFAULT_CONTENT_TYPE)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = method.ToUpper();// "POST";
            request.Timeout = timeOut;
            request.Proxy = _webproxy;
            if (cer != null)
            {
                request.ClientCertificates.Add(cer);
            }

            if (checkValidationResult)
            {
                ServicePointManager.ServerCertificateValidationCallback =
                  new RemoteCertificateValidationCallback(CheckValidationResult);
            }

            contentType ??= HttpClientHelper.DEFAULT_CONTENT_TYPE;

            #region Handle Form File Upload
            var formUploadFile = fileDictionary != null && fileDictionary.Count > 0;//Whether to upload files via Form
            if (formUploadFile)
            {
                contentType = "multipart/form-data";

                //Upload files via form
                string boundary = "----" + SystemTime.Now.Ticks.ToString("x");

                postStream = postStream ?? new MemoryStream();
                //byte[] boundarybytes = Encoding.ASCII.GetBytes("\r\n--" + boundary + "\r\n");
                string fileFormdataTemplate = "\r\n--" + boundary + "\r\nContent-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"\r\nContent-Type: application/octet-stream\r\n\r\n";
                string dataFormdataTemplate = "\r\n--" + boundary +
                                                "\r\nContent-Disposition: form-data; name=\"{0}\"\r\n\r\n{1}";


                foreach (var file in fileDictionary)
                {
                    try
                    {
                        var fileNameOrFileData = file.Value;

                        var formFileData = new FormFileData(fileNameOrFileData);
                        string formdata = null;
                        using (var memoryStream = new MemoryStream())
                        {
                            if (formFileData.TryLoadStream(memoryStream).ConfigureAwait(false).GetAwaiter().GetResult())
                            {
                                //fileNameOrFileData stores a Stream
                                var fileName = Path.GetFileName(formFileData.GetAvaliableFileName(SystemTime.NowTicks.ToString()));
                                formdata = string.Format(fileFormdataTemplate, file.Key, fileName);
                            }
                            else
                            {
                                //fileNameOrFileData may store a file path or a comment

                                //Prepare file stream
                                using (var fileStream = FileHelper.GetFileStream(fileNameOrFileData))
                                {
                                    if (fileStream != null)
                                    {
                                        //File exists
                                        memoryStream.Seek(0, SeekOrigin.Begin);
                                        fileStream.CopyTo(memoryStream);
                                        formdata = string.Format(fileFormdataTemplate, file.Key, Path.GetFileName(fileNameOrFileData));
                                        fileStream.Dispose();
                                    }
                                    else
                                    {
                                        //File does not exist or is only a comment
                                        formdata = string.Format(dataFormdataTemplate, file.Key, file.Value);
                                    }
                                }
                            }

                            //Unified processing
                            var formdataBytes = Encoding.UTF8.GetBytes(postStream.Length == 0 ? formdata.Substring(2, formdata.Length - 2) : formdata);//No newline needed on the first line
                            postStream.Write(formdataBytes, 0, formdataBytes.Length);

                            //Write file
                            if (memoryStream.Length > 0)
                            {
                                memoryStream.Seek(0, SeekOrigin.Begin);

                                byte[] buffer = new byte[1024];
                                int bytesRead = 0;
                                while ((bytesRead = memoryStream.Read(buffer, 0, buffer.Length)) != 0)
                                {
                                    postStream.Write(buffer, 0, bytesRead);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                }
                //Footer
                var footer = Encoding.UTF8.GetBytes("\r\n--" + boundary + "--\r\n");
                postStream.Write(footer, 0, footer.Length);

                //request.ContentType = string.Format("multipart/form-data; boundary={0}", boundary);//request.ContentType is set uniformly below
                contentType = string.Format("multipart/form-data; boundary={0}", boundary);
            }
            else
            {
                if (postStream.Length > 0)
                {
                    if (hasFormData)
                    {
                        // Form submission
                        contentType = "application/x-www-form-urlencoded";
                    }
                    else if (contentType == HttpClientHelper.DEFAULT_CONTENT_TYPE)
                    {
                        //If ContentType is the default value, set it to binary stream
                        contentType = "application/octet-stream";
                    }

                    //contentType = "application/x-www-form-urlencoded";
                }
            }
            #endregion

            request.ContentType = contentType;
            request.ContentLength = postStream != null ? postStream.Length : 0;

            HttpClientHeader(request, refererUrl, useAjax, headerAddition, timeOut);

            if (cookieContainer != null)
            {
                request.CookieContainer = cookieContainer;
            }

            return request;
        }

#endif

#if !NET462
        /// <summary>
        /// Common HttpPost request setup method for .NET Core
        /// </summary>
        /// <param name="serviceProvider">Server provider under .NET Core; keep null for .NET Framework</param>
        /// <param name="url"></param>
        /// <param name="hc"></param>
        /// <param name="cookieContainer"></param>
        /// <param name="postStream"></param>
        /// <param name="fileDictionary">Files to upload; Key: upload Name, Value: local file path or Base64-encoded file content</param>
        /// <param name="refererUrl"></param>
        /// <param name="encoding"></param>
        /// <param name="certName">Unique certificate name; keep null if not needed</param>
        /// <param name="useAjax"></param>
        /// <param name="headerAddition">Additional header information</param>
        /// <param name="timeOut"></param>
        /// <param name="checkValidationResult"></param>
        /// <param name="contentType"></param>
        /// <returns></returns>
        public static async Task<(HttpClient HttpClient, HttpContent HttpContent)> HttpPost_Common_NetCoreAsync(
            IServiceProvider serviceProvider,
            string url, CookieContainer cookieContainer = null,
            Stream postStream = null, Dictionary<string, string> fileDictionary = null, string refererUrl = null,
            Encoding encoding = null,
            ApiClient apiClient = null,
            string certName = null, bool useAjax = false, Dictionary<string, string> headerAddition = null,
            int timeOut = Config.TIME_OUT, bool checkValidationResult = false,
            bool hasFormData = false,
            string contentType = HttpClientHelper.DEFAULT_CONTENT_TYPE)
        {
            //HttpClientHandler handler = HttpClientHelper.GetHttpClientHandler(cookieContainer, SenparcHttpClientWebProxy, DecompressionMethods.GZip);

            //if (checkValidationResult)
            //{
            //    handler.ServerCertificateCustomValidationCallback = new Func<HttpRequestMessage, X509Certificate2, X509Chain, SslPolicyErrors, bool>(CheckValidationResult);
            //}

            //if (cer != null)
            //{
            //    handler.ClientCertificates.Add(cer);
            //}

            //TODO: handler is not used here, so cer cannot actually be passed (this is also a current .NET Core issue for multi-certificate scenarios)

            var senparcHttpClient = apiClient == null
                ? SenparcHttpClient.GetInstanceByName(serviceProvider, certName)
                : apiClient.SenparcHttpClient;

            contentType ??= HttpClientHelper.DEFAULT_CONTENT_TYPE;

            senparcHttpClient.SetCookie(new Uri(url), cookieContainer);//Set Cookie

            HttpClient client = senparcHttpClient.Client;
            client.Timeout = TimeSpan.FromMilliseconds(timeOut);
            HttpContent hc = null;
            HttpClientHeader(client, refererUrl, useAjax, headerAddition, timeOut);

            #region Handle Form File Upload

            var formUploadFile = fileDictionary != null && fileDictionary.Count > 0;//Whether to upload files via Form
            if (formUploadFile)
            {
                if (contentType == HttpClientHelper.DEFAULT_CONTENT_TYPE)
                {
                    contentType = "multipart/form-data";
                }

                //Upload files via form
                string boundary = "----" + SystemTime.Now.Ticks.ToString("x");

                var multipartFormDataContent = new MultipartFormDataContent(boundary);
                hc = multipartFormDataContent;

                foreach (var file in fileDictionary)
                {
                    try
                    {
                        var fileNameOrFileData = file.Value;
                        var formFileData = new FormFileData(fileNameOrFileData);
                        string fileName = null;

                        //Prepare file stream
                        var memoryStream = new MemoryStream();//Cannot dispose here; otherwise an error occurs if memoryStream is already closed during the request
                        if (await formFileData.TryLoadStream(memoryStream))
                        {
                            //fileNameOrFileData stores a Stream
                            fileName = Path.GetFileName(formFileData.GetAvaliableFileName(SystemTime.NowTicks.ToString()));
                        }
                        else
                        {
                            //fileNameOrFileData may store a file path or a comment
                            using (var fileStream = FileHelper.GetFileStream(fileNameOrFileData))
                            {
                                if (fileStream != null)
                                {
                                    //File exists
                                    fileStream.CopyTo(memoryStream);//TODO: async method can be used
                                    fileName = Path.GetFileName(fileNameOrFileData);
                                    fileStream.Dispose();
                                }
                                else
                                {
                                    //Comment only
                                    multipartFormDataContent.Add(new StringContent(file.Value), "\"" + file.Key + "\"");
                                }
                            }
                        }

                        if (memoryStream.Length > 0)
                        {
                            //Has file content
                            //multipartFormDataContent.Add(new StreamContent(memoryStream), file.Key, Path.GetFileName(fileName)); //Throws stream-already-closed exception

                            memoryStream.Seek(0, SeekOrigin.Begin);
                            var streamContent = CreateFileContent(memoryStream, file.Key, fileName, contentType);
                            multipartFormDataContent.Add(streamContent, file.Key, fileName);
                        }
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                }

                hc.Headers.ContentType = MediaTypeHeaderValue.Parse(string.Format("multipart/form-data; boundary={0}", boundary));
            }
            else
            {
                postStream.Seek(0, SeekOrigin.Begin);
                
                if (postStream.Length > 0)
                {
                    if (hasFormData)
                    {
                        // Form submission
                        contentType = "application/x-www-form-urlencoded";
                    }
                    else if (contentType == HttpClientHelper.DEFAULT_CONTENT_TYPE)
                    {
                        //If ContentType is the default value, set it to binary stream
                        contentType = "application/octet-stream";
                    }

                    //contentType = "application/x-www-form-urlencoded";
                }

                hc = new StreamContent(postStream);

                contentType ??= HttpClientHelper.DEFAULT_CONTENT_TYPE;

                hc.Headers.ContentType = new MediaTypeHeaderValue(contentType);

                //Use application/x-www-form-urlencoded only when posting Form data in URL format
                //Uncomment to test the case where Request.Body is empty
                //hc.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");
            }

            //HttpContentHeader(hc, timeOut);
            #endregion

            if (!string.IsNullOrEmpty(refererUrl))
            {
                client.DefaultRequestHeaders.Referrer = new Uri(refererUrl);
            }

            return (client, hc);
        }

#endif

        #endregion

        #region Synchronous Methods

        /// <summary>
        /// Get string result using Post method (standard form submission)
        /// </summary>
        /// <param name="serviceProvider">Server provider under .NET Core; keep null for .NET Framework</param>
        /// <param name="url"></param>
        /// <param name="cookieContainer"></param>
        /// <param name="formData"></param>
        /// <param name="encoding"></param>
        /// <param name="certName">Unique certificate name; keep null if not needed</param>
        /// <param name="cer">Certificate; keep null if not needed</param>
        /// <param name="useAjax"></param>
        /// <param name="headerAddition">Additional header information</param>
        /// <param name="timeOut"></param>
        /// <param name="checkValidationResult">Automatically validate server certificate callback</param>
        /// <param name="contentType">Content-Type in the request header; defaults to <see cref="HttpClientHelper.DEFAULT_CONTENT_TYPE"/></param>
        /// <returns></returns>
        public static string HttpPost(
            IServiceProvider serviceProvider,
            string url, CookieContainer cookieContainer = null, Dictionary<string, string> formData = null,
            Encoding encoding = null,
#if !NET462
            ApiClient apiClient = null,
            string certName = null,
#else
            X509Certificate2 cer = null,
#endif            
            bool useAjax = false, Dictionary<string, string> headerAddition = null,
            int timeOut = Config.TIME_OUT,
            bool checkValidationResult = false,
            string contentType = null
            )
        {
            var hasFormData = formData != null;

            MemoryStream ms = new MemoryStream();
            formData.FillFormDataStream(ms);//Fill formData

            contentType ??= HttpClientHelper.GetContentType(formData);

            return HttpPost(
                serviceProvider,
                url, cookieContainer, ms, null, null, encoding,
#if !NET462
                apiClient,
                certName,
#else
                cer,
#endif
                useAjax, headerAddition, timeOut, checkValidationResult, hasFormData, contentType);
        }

        /// <summary>
        /// Get string result using Post method
        /// </summary>
        /// <param name="serviceProvider">Server provider under .NET Core; keep null for .NET Framework</param>
        /// <param name="url"></param>
        /// <param name="cookieContainer"></param>
        /// <param name="postStream"></param>
        /// <param name="fileDictionary">Files to upload; Key: upload Name, Value: local file path or Base64-encoded file content</param>
        /// <param name="encoding"></param>
        /// <param name="certName">Unique certificate name; keep null if not needed</param>
        /// <param name="cer">Certificate; keep null if not needed</param>
        /// <param name="useAjax"></param>
        /// <param name="headerAddition">Additional header information</param>
        /// <param name="timeOut"></param>
        /// <param name="checkValidationResult">Automatically validate server certificate callback</param>
        /// <param name="contentType"></param>
        /// <param name="refererUrl"></param>
        /// <returns></returns>
        public static string HttpPost(
            IServiceProvider serviceProvider,
            string url, CookieContainer cookieContainer = null, Stream postStream = null,
            Dictionary<string, string> fileDictionary = null, string refererUrl = null,
            Encoding encoding = null,
#if !NET462
            ApiClient apiClient = null,
            string certName = null,
#else
            X509Certificate2 cer = null,
#endif
            bool useAjax = false, Dictionary<string, string> headerAddition = null, int timeOut = Config.TIME_OUT, bool checkValidationResult = false, bool hasFormData = false,
            string contentType = HttpClientHelper.DEFAULT_CONTENT_TYPE)
        {
#if !NET8_0_OR_GREATER
            if (cookieContainer == null)
            {
                cookieContainer = new CookieContainer();
            }
#endif

            var senparcResponse = HttpResponsePost(
                serviceProvider,
                url, cookieContainer, postStream, fileDictionary, refererUrl, encoding,
#if !NET462
                apiClient,
                certName,
#else
                cer,
#endif
                useAjax, headerAddition, timeOut, checkValidationResult, hasFormData, contentType);

            var response = senparcResponse.Result;//Get response information


#if NET462

            #region Replaced by method reuse
            /*
            
            var request = HttpPost_Common_Net45(url, cookieContainer, postStream, fileDictionary, refererUrl, encoding, cer, useAjax, timeOut, checkValidationResult);

            #region Write Binary Stream
            if (postStream != null)
            {
                postStream.Position = 0;

                //Write directly to stream
                Stream requestStream = request.GetRequestStream();

                byte[] buffer = new byte[1024];
                int bytesRead = 0;
                while ((bytesRead = postStream.Read(buffer, 0, buffer.Length)) != 0)
                {
                    requestStream.Write(buffer, 0, bytesRead);
                }

                //debug
                //postStream.Seek(0, SeekOrigin.Begin);
                //StreamReader sr = new StreamReader(postStream);
                //var postStr = sr.ReadToEnd();

                postStream.Close();//Close file access
            }
            #endregion
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            */

            #endregion

            //HttpWebResponse response = senparcResponse.Result;

            response.Cookies = cookieContainer.GetCookies(response.ResponseUri);

            using (Stream responseStream = response.GetResponseStream() ?? new MemoryStream())
            {
                using (StreamReader myStreamReader = new StreamReader(responseStream, encoding ?? Encoding.GetEncoding("utf-8")))
                {
                    string retString = myStreamReader.ReadToEnd();
                    return retString;
                }
            }
#else
            HttpClientHelper.SetResponseCookieContainer(cookieContainer, response);//Set Cookie

            //var response = senparcResponse.Result;

            if (response.Content.Headers.ContentType != null &&
                response.Content.Headers.ContentType.CharSet != null &&
                response.Content.Headers.ContentType.CharSet.ToLower().Contains("utf8"))
            {
                response.Content.Headers.ContentType.CharSet = "utf-8";
            }

            var retString = response.Content.ReadAsStringAsync().ConfigureAwait(false).GetAwaiter().GetResult();

            response.Dispose();

            return retString;

            //t1.Wait();
            //return t1.Result;
#endif
        }


        /// <summary>
        /// Get HttpWebResponse or HttpResponseMessage using Post method; typically used for testing when called independently)
        /// </summary>
        /// <param name="serviceProvider">Server provider under .NET Core; keep null for .NET Framework</param>
        /// <param name="url"></param>
        /// <param name="cookieContainer"></param>
        /// <param name="postStream"></param>
        /// <param name="fileDictionary">Files to upload; Key: upload Name, Value: local file path or Base64-encoded file content</param>
        /// <param name="encoding"></param>
        /// <param name="certName">Unique certificate name; keep null if not needed</param>
        /// <param name="cer">Certificate; keep null if not needed</param>
        /// <param name="useAjax"></param>
        /// <param name="headerAddition">Additional header information</param>
        /// <param name="timeOut"></param>
        /// <param name="checkValidationResult">Automatically validate server certificate callback</param>
        /// <param name="hasFormData"></param>
        /// <param name="contentType"></param>
        /// <param name="refererUrl"></param>
        /// <returns></returns>
        public static SenparcHttpResponse HttpResponsePost(
            IServiceProvider serviceProvider,
            string url, CookieContainer cookieContainer = null, Stream postStream = null,
            Dictionary<string, string> fileDictionary = null, string refererUrl = null,
            Encoding encoding = null,
#if !NET462
            ApiClient apiClient = null,
            string certName = null,
#else
            X509Certificate2 cer = null,
#endif
            bool useAjax = false, Dictionary<string, string> headerAddition = null, int timeOut = Config.TIME_OUT,
            bool checkValidationResult = false,
            bool hasFormData = false,
            string contentType = HttpClientHelper.DEFAULT_CONTENT_TYPE)
        {
#if !NET8_0_OR_GREATER
            if (cookieContainer == null)
            {
                cookieContainer = new CookieContainer();
            }
#endif

            var postStreamIsDefaultNull = postStream == null;
            if (postStreamIsDefaultNull)
            {
                postStream = new MemoryStream();
            }

#if NET462
            var request = HttpPost_Common_Net45(url, "POST", cookieContainer, postStream, fileDictionary, refererUrl, encoding, cer, useAjax, headerAddition, hasFormData, timeOut, checkValidationResult, contentType);

            #region Write Binary Stream
            if (postStream != null && postStream.Length > 0)
            {
                postStream.Position = 0;

                //Write directly to stream
                Stream requestStream = request.GetRequestStream();

                byte[] buffer = new byte[1024];
                int bytesRead = 0;
                while ((bytesRead = postStream.Read(buffer, 0, buffer.Length)) != 0)
                {
                    requestStream.Write(buffer, 0, bytesRead);
                }

                //debug
                //postStream.Seek(0, SeekOrigin.Begin);
                //StreamReader sr = new StreamReader(postStream);
                //var postStr = sr.ReadToEnd();

                postStream.Close();//Close file access
            }
            #endregion

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            return new SenparcHttpResponse(response);
#else
            var httpPost = HttpPost_Common_NetCoreAsync(serviceProvider, url, cookieContainer, postStream, fileDictionary, refererUrl, encoding, apiClient, certName, useAjax, headerAddition, timeOut, checkValidationResult, hasFormData, contentType).ConfigureAwait(false).GetAwaiter().GetResult();

            var client = httpPost.HttpClient;
            var hc = httpPost.HttpContent;

            HttpResponseMessage response;

            using (var cts = new System.Threading.CancellationTokenSource(timeOut))
            {
                try
                {
                    response = client.PostAsync(url, hc, cancellationToken: cts.Token).ConfigureAwait(false).GetAwaiter().GetResult();//Get response information
                }
                catch { throw; }
            }

            HttpClientHelper.SetResponseCookieContainer(cookieContainer, response);//Set Cookie

            try
            {
                if (postStreamIsDefaultNull && postStream.Length > 0)
                {
                    postStream.Close();
                }

                hc.Dispose();//Close HttpContent (StreamContent)
            }
            catch (BaseException ex)
            {
            }

            return new SenparcHttpResponse(response);
#endif
        }

        #endregion

        #region Asynchronous Methods

        /// <summary>
        /// Get string result using Post method (standard form submission)
        /// </summary>
        /// <param name="serviceProvider">Server provider under .NET Core; keep null for .NET Framework</param>
        /// <param name="url"></param>
        /// <param name="cookieContainer"></param>
        /// <param name="formData"></param>
        /// <param name="encoding"></param>
        /// <param name="certName">Unique certificate name; keep null if not needed</param>
        /// <param name="cer">Certificate; keep null if not needed</param>
        /// <param name="useAjax"></param>
        /// <param name="headerAddition">Additional header information</param>
        /// <param name="timeOut"></param>
        /// <param name="checkValidationResult">Automatically validate server certificate callback</param>
        /// <param name="contentType">Content-Type in the request header; defaults to <see cref="HttpClientHelper.DEFAULT_CONTENT_TYPE"/></param>
        /// <returns></returns>
        public static async Task<string> HttpPostAsync(
            IServiceProvider serviceProvider,
            string url, CookieContainer cookieContainer = null,
            Dictionary<string, string> formData = null, Encoding encoding = null,
#if !NET462
            ApiClient apiClient = null,
            string certName = null,
#else
            X509Certificate2 cer = null,
#endif
            bool useAjax = false, Dictionary<string, string> headerAddition = null,
            int timeOut = Config.TIME_OUT,
            bool checkValidationResult = false,
            string contentType = null

            )
        {
            var hasFormData = formData != null;

            MemoryStream ms = new MemoryStream();
            await formData.FillFormDataStreamAsync(ms).ConfigureAwait(false);//Fill formData

            contentType ??= HttpClientHelper.GetContentType(formData);

            return await HttpPostAsync(
                serviceProvider,
                url, cookieContainer, ms, null, null, encoding,
#if !NET462
                apiClient,
                certName,
#else
                cer,
#endif
                useAjax, headerAddition, hasFormData, timeOut, checkValidationResult, contentType).ConfigureAwait(false);
        }


        /// <summary>
        /// Get string result using Post method
        /// </summary>
        /// <param name="serviceProvider">Server provider under .NET Core; keep null for .NET Framework</param>
        /// <param name="url"></param>
        /// <param name="cookieContainer"></param>
        /// <param name="postStream"></param>
        /// <param name="fileDictionary">Files to upload; Key: upload Name, Value: local file path or Base64-encoded file content</param>
        /// <param name="certName">Unique certificate name; keep null if not needed</param>
        /// <param name="cer"></param>
        /// <param name="useAjax"></param>
        /// <param name="headerAddition">Additional header information</param>
        /// <param name="timeOut"></param>
        /// <param name="checkValidationResult">Automatically validate server certificate callback</param>
        /// <param name="contentType"></param>
        /// <param name="refererUrl"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public static async Task<string> HttpPostAsync(
            IServiceProvider serviceProvider,
            string url, CookieContainer cookieContainer = null, Stream postStream = null,
            Dictionary<string, string> fileDictionary = null, string refererUrl = null, Encoding encoding = null,
#if !NET462
            ApiClient apiClient = null,
            string certName = null,
#else
            X509Certificate2 cer = null,
#endif
            bool useAjax = false, Dictionary<string, string> headerAddition = null,
            bool hasFormData = false,
            int timeOut = Config.TIME_OUT, bool checkValidationResult = false,
            string contentType = HttpClientHelper.DEFAULT_CONTENT_TYPE)
        {
#if !NET8_0_OR_GREATER
            if (cookieContainer == null)
            {
                cookieContainer = new CookieContainer();
            }
#endif

            var postStreamIsDefaultNull = postStream == null;
            if (postStreamIsDefaultNull)
            {
                postStream = new MemoryStream();
            }

            //var dt1 = SystemTime.Now;
            //Console.WriteLine($"{System.Threading.Thread.CurrentThread.Name} - START - {dt1:HH:mm:ss.ffff}");

            var senparcResponse = await HttpResponsePostAsync(
                serviceProvider,
                url, cookieContainer, postStream, fileDictionary, refererUrl, encoding,
#if !NET462
                apiClient,
                certName,
#else
                cer,
#endif
                useAjax, headerAddition, hasFormData, timeOut, checkValidationResult, contentType).ConfigureAwait(false);

            var response = senparcResponse.Result;//Get response information

            //Console.WriteLine($"{System.Threading.Thread.CurrentThread.Name} - FINISH- {SystemTime.DiffTotalMS(dt1):###,###} ms");

#if NET462
            #region Replaced by method reuse
            /*

            var request = HttpPost_Common_Net45(url, cookieContainer, postStream, fileDictionary, refererUrl, encoding, cer, useAjax,headerAddition, timeOut, checkValidationResult);

            #region Write Binary Stream
            if (postStream != null && postStream.Length > 0)
            {
                postStream.Position = 0;

                //Write directly to stream
                Stream requestStream = await request.GetRequestStreamAsync().ConfigureAwait(false);

                byte[] buffer = new byte[1024];
                int bytesRead = 0;
                while ((bytesRead = await postStream.ReadAsync(buffer, 0, buffer.Length)) != 0).ConfigureAwait(false)
                {
                    await requestStream.WriteAsync(buffer, 0, bytesRead).ConfigureAwait(false);
                }


                //debug
                //postStream.Seek(0, SeekOrigin.Begin);
                //StreamReader sr = new StreamReader(postStream);
                //var postStr = await sr.ReadToEndAsync().ConfigureAwait(false);

                postStream.Close();//Close file access
            }

            #endregion
            HttpWebResponse response = (HttpWebResponse)(await request.GetResponseAsync()).ConfigureAwait(false);;
    */
            #endregion


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
            HttpClientHelper.SetResponseCookieContainer(cookieContainer, response);//Set Cookie

            #region Replaced by method reuse
            /*
            HttpContent hc;
            var client = HttpPost_Common_NetCore(url, out hc, cookieContainer, postStream, fileDictionary, refererUrl, encoding, cer, useAjax, timeOut, checkValidationResult);

            var response = await client.PostAsync(url, hc).ConfigureAwait(false);

            if (response.Content.Headers.ContentType.CharSet != null &&
                response.Content.Headers.ContentType.CharSet.ToLower().Contains("utf8"))
            {
                response.Content.Headers.ContentType.CharSet = "utf-8";
            }


            var retString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            try
            {
                if (postStreamIsDefaultNull && postStream.Length > 0)
                {
                    postStream.Close();
                }

                hc.Dispose();//Close HttpContent (StreamContent)
            }
            catch (BaseException ex)
            {
            }
            */
            #endregion

            var retString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            response.Dispose();

            return retString;
#endif
        }

        /// <summary>
        /// Get HttpWebResponse or HttpResponseMessage using Post method; typically used for testing when called independently)
        /// </summary>
        /// <param name="serviceProvider">Server provider under .NET Core; keep null for .NET Framework</param>
        /// <param name="url"></param>
        /// <param name="cookieContainer"></param>
        /// <param name="postStream"></param>
        /// <param name="fileDictionary">Files to upload; Key: upload Name, Value: local file path or Base64-encoded file content</param>
        /// <param name="encoding"></param>
        /// <param name="certName">Unique certificate name; keep null if not needed</param>
        /// <param name="cer">Certificate; keep null if not needed</param>
        /// <param name="useAjax"></param>
        /// <param name="headerAddition">Additional header information</param>
        /// <param name="timeOut"></param>
        /// <param name="checkValidationResult">Automatically validate server certificate callback</param>
        /// <param name="contentType"></param>
        /// <param name="refererUrl"></param>
        /// <returns></returns>
        public static async Task<SenparcHttpResponse> HttpResponsePostAsync(
            IServiceProvider serviceProvider,
            string url, CookieContainer cookieContainer = null, Stream postStream = null,
            Dictionary<string, string> fileDictionary = null, string refererUrl = null, Encoding encoding = null,
#if !NET462
            ApiClient apiClient = null,
            string certName = null,
#else
            X509Certificate2 cer = null,
#endif
            bool useAjax = false, Dictionary<string, string> headerAddition = null,
            bool hasFormData = false,
            int timeOut = Config.TIME_OUT,
            bool checkValidationResult = false,
            string contentType = HttpClientHelper.DEFAULT_CONTENT_TYPE)
        {
#if !NET8_0_OR_GREATER
            if (cookieContainer == null)
            {
                cookieContainer = new CookieContainer();
            }
#endif

            var postStreamIsDefaultNull = postStream == null;
            if (postStreamIsDefaultNull)
            {
                postStream = new MemoryStream();
            }

#if NET462
            var request = HttpPost_Common_Net45(url, "POST", cookieContainer, postStream, fileDictionary, refererUrl, encoding, cer, useAjax, headerAddition, hasFormData, timeOut, checkValidationResult, contentType);

            #region Write Binary Stream
            if (postStream != null && postStream.Length > 0)
            {
                postStream.Position = 0;

                //Write directly to stream
                Stream requestStream = await request.GetRequestStreamAsync().ConfigureAwait(false);

                byte[] buffer = new byte[1024];
                int bytesRead = 0;
                while ((bytesRead = await postStream.ReadAsync(buffer, 0, buffer.Length).ConfigureAwait(false)) != 0)
                {
                    await requestStream.WriteAsync(buffer, 0, bytesRead).ConfigureAwait(false);
                }

                //debug
                //postStream.Seek(0, SeekOrigin.Begin);
                //StreamReader sr = new StreamReader(postStream);
                //var postStr = sr.ReadToEnd();

                postStream.Close();//Close file access
            }
            #endregion

            HttpWebResponse response = (HttpWebResponse)(await request.GetResponseAsync().ConfigureAwait(false));
            return new SenparcHttpResponse(response);
#else
            var httpPost = await HttpPost_Common_NetCoreAsync(serviceProvider, url, cookieContainer, postStream, fileDictionary, refererUrl, encoding, apiClient, certName, useAjax, headerAddition, timeOut, checkValidationResult, hasFormData, contentType);

            var client = httpPost.HttpClient;
            var hc = httpPost.HttpContent;

            HttpResponseMessage response;

            using (var cts = new System.Threading.CancellationTokenSource(timeOut))
            {
                try
                {
                    response = await client.PostAsync(url, hc, cancellationToken: cts.Token).ConfigureAwait(false);//Get response information
                }
                catch { throw; }
            }

            HttpClientHelper.SetResponseCookieContainer(cookieContainer, response);//Set Cookie

            try
            {
                if (postStreamIsDefaultNull && postStream.Length > 0)
                {
                    postStream.Close();
                }

                hc.Dispose();//Close HttpContent (StreamContent)
            }
            catch (BaseException ex)
            {
            }

            return new SenparcHttpResponse(response);
#endif
        }


        #endregion

    }
}
