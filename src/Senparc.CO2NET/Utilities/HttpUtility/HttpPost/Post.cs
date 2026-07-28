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

    Filename: Post.cs  
  
    Description: Post  
  
    Creation Identifier: Senparc - 20180602
  
    Modification Identifier: Senparc - 20150303  
    Modification Description: Organized interfaces  
  
    Modification Identifier: Senparc - 20150312  
    Modification Description: Opened proxy request timeout  
  
    Modification Identifier: zhanghao-kooboo - 20150316  
    Modification Description: Added  
  
    Modification Identifier: Senparc - 20150407  
    Modification Description: Modified Post request method to upload permanent video materials  
  
    Modification Identifier: Senparc - 20160720  
    Modification Description: Added asynchronous method PostFileGetJsonAsync (with one more parameter than the previous method)  
  
    Modification Identifier: Senparc - 20170409  
    Modification Description: v4.11.9 Modified Download method  
  
    Modification Identifier: Senparc - 20190429  
    Modification Description: v0.7.0 Optimized HttpClient, refactored RequestUtility (including Post and Get), introduced HttpClientFactory mechanism  
  
    Modification Identifier: Senparc - 20190521  
    Modification Description: v0.7.3 .NET Core provides multi-certificate registration feature  
  
    Modification Identifier: Senparc - 20221115  
    Modification Description: v2.1.3 Optimized simulated Form submission  
  
    Modification Identifier: Senparc - 20230110  
    Modification Description: v2.1.7 HttpUtility.Post methods provide contentType parameter  
  
    Modification Identifier: Senparc - 20241119  
    Modification Description: v3.0.0-beta3 Added ApiClient parameter

    Modification Identifier: Senparc - 20260722
    Modification Description: v4.1.0 Added JsonTypeInfo file, stream, and form POST Native AOT overloads

----------------------------------------------------------------*/



using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization.Metadata;
using Senparc.CO2NET.Helpers;
using System.Net.Http;
#if NET8_0_OR_GREATER
using System.Diagnostics.CodeAnalysis;
#endif

#if NET462
using System.Web.Script.Serialization;
using System.Security.Cryptography.X509Certificates;
#else
using Microsoft.Extensions.DependencyInjection;
#endif


namespace Senparc.CO2NET.HttpUtility
{
    /// <summary>
    /// Post request processing
    /// </summary>
    public static class Post
    {
        #region Synchronous Methods

        /// <summary>
        /// Send a Post request with optional file upload
        /// </summary>
        /// <typeparam name="T">Return data type (JSON entity type)</typeparam>
        /// <param name="serviceProvider">Server provider under .NET Core; keep null for .NET Framework</param>
        /// <param name="url">Request URL</param>
        /// <param name="cookieContainer">CookieContainer; set to null if not needed</param>
        /// <param name="encoding"></param>
        /// <param name="certName">Unique certificate name; keep null if not needed</param>
        /// <param name="cer">Certificate; keep null if not needed</param>
        /// <param name="useAjax"></param>
        /// <param name="timeOut">Proxy request timeout in milliseconds</param>
        /// <param name="fileDictionary">Files to Post (Dictionary Key=name, Value=absolute path)</param>
        /// <param name="postDataDictionary">Key-value pairs to Post (name, value)</param>
        /// <param name="contentType">Content-Type in the request header; defaults to <see cref="HttpClientHelper.DEFAULT_CONTENT_TYPE"/></param>
        /// <param name="afterReturnText">Return JSON text and trigger before deserialization; parameters are url and returnText</param>
        /// <returns></returns>
#if NET8_0_OR_GREATER
        [RequiresDynamicCode("Runtime JSON metadata may require dynamic code. Use the PostFileGetJson overload with JsonTypeInfo<T> for Native AOT.")]
        [RequiresUnreferencedCode("Runtime JSON metadata may be removed by trimming. Use the PostFileGetJson overload with JsonTypeInfo<T> for Native AOT.")]
#endif
        public static T PostFileGetJson<T>(
            IServiceProvider serviceProvider,
            string url, CookieContainer cookieContainer = null, Dictionary<string, string> fileDictionary = null,
            Dictionary<string, string> postDataDictionary = null,
            Encoding encoding = null,
#if !NET462
            ApiClient apiClient = null,
            string certName = null,
#else
            X509Certificate2 cer = null,
#endif
            bool useAjax = false,
            string contentType = null,
            Action<string, string> afterReturnText = null, int timeOut = Config.TIME_OUT)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                postDataDictionary.FillFormDataStream(ms); //Fill formData

                string returnText = RequestUtility.HttpPost(
                    serviceProvider,
                    url, cookieContainer, ms, fileDictionary, null, encoding,
#if !NET462
                    apiClient,
                    certName,
#else
                    cer,
#endif
                    useAjax, null, timeOut, contentType: contentType);

                afterReturnText?.Invoke(url, returnText);

                var result = SerializerHelper.GetObject<T>(returnText);
                return result;
            }
        }

        /// <summary>
        /// Send a Post file request and deserialize the response using source-generated metadata. This overload supports Native AOT.
        /// </summary>
        public static T PostFileGetJson<T>(
            JsonTypeInfo<T> jsonTypeInfo,
            IServiceProvider serviceProvider,
            string url, CookieContainer cookieContainer = null, Dictionary<string, string> fileDictionary = null,
            Dictionary<string, string> postDataDictionary = null,
            Encoding encoding = null,
#if !NET462
            ApiClient apiClient = null,
            string certName = null,
#else
            X509Certificate2 cer = null,
#endif
            bool useAjax = false,
            string contentType = null,
            Action<string, string> afterReturnText = null, int timeOut = Config.TIME_OUT)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                postDataDictionary.FillFormDataStream(ms);

                string returnText = RequestUtility.HttpPost(
                    serviceProvider,
                    url, cookieContainer, ms, fileDictionary, null, encoding,
#if !NET462
                    apiClient,
                    certName,
#else
                    cer,
#endif
                    useAjax, null, timeOut, contentType: contentType);

                afterReturnText?.Invoke(url, returnText);

                return SerializerHelper.GetObject(jsonTypeInfo, returnText);
            }
        }

        /// <summary>
        /// Send a Post request that may include a file stream
        /// </summary>
        /// <typeparam name="T">Return data type (JSON entity type)</typeparam>
        /// <param name="serviceProvider">Server provider under .NET Core; keep null for .NET Framework</param>
        /// <param name="url">Request URL</param>
        /// <param name="cookieContainer">CookieContainer; set to null if not needed</param>
        /// <param name="fileStream">File stream</param>
        /// <param name="encoding"></param>
        /// <param name="certName">Unique certificate name; keep null if not needed</param>
        /// <param name="cer">Certificate; keep null if not needed</param>
        /// <param name="useAjax">Whether to use an Ajax request</param>
        /// <param name="contentType">Content-Type in the request header; defaults to <see cref="HttpClientHelper.DEFAULT_CONTENT_TYPE"/></param>
        /// <param name="timeOut">Proxy request timeout in milliseconds</param>
        /// <param name="checkValidationResult">Automatically validate server certificate callback</param>
        /// <param name="afterReturnText">Return JSON text and trigger before deserialization; parameters are url and returnText</param>
        /// <returns></returns>
#if NET8_0_OR_GREATER
        [RequiresDynamicCode("Runtime JSON metadata may require dynamic code. Use the PostGetJson overload with JsonTypeInfo<T> for Native AOT.")]
        [RequiresUnreferencedCode("Runtime JSON metadata may be removed by trimming. Use the PostGetJson overload with JsonTypeInfo<T> for Native AOT.")]
#endif
        public static T PostGetJson<T>(
            IServiceProvider serviceProvider,
            string url, CookieContainer cookieContainer = null, Stream fileStream = null, Encoding encoding = null,
#if !NET462
            ApiClient apiClient = null,
            string certName = null,
#else
            X509Certificate2 cer = null,
#endif
            bool useAjax = false,
            string contentType = null,
            bool checkValidationResult = false, Action<string, string> afterReturnText = null,
            int timeOut = Config.TIME_OUT)
        {
            string returnText = RequestUtility.HttpPost(
                serviceProvider,
                url, cookieContainer, fileStream, null, null, encoding,
#if !NET462
                apiClient,
                certName,
#else
                cer,
#endif
                useAjax, null, timeOut, checkValidationResult, contentType: contentType);

            //SenparcTrace.SendApiLog(url, returnText);
            afterReturnText?.Invoke(url, returnText);

            var result = SerializerHelper.GetObject<T>(returnText);
            return result;
        }

        /// <summary>
        /// Send a Post stream request and deserialize the response using source-generated metadata. This overload supports Native AOT.
        /// </summary>
        public static T PostGetJson<T>(
            JsonTypeInfo<T> jsonTypeInfo,
            IServiceProvider serviceProvider,
            string url, CookieContainer cookieContainer = null, Stream fileStream = null, Encoding encoding = null,
#if !NET462
            ApiClient apiClient = null,
            string certName = null,
#else
            X509Certificate2 cer = null,
#endif
            bool useAjax = false,
            string contentType = null,
            bool checkValidationResult = false, Action<string, string> afterReturnText = null,
            int timeOut = Config.TIME_OUT)
        {
            string returnText = RequestUtility.HttpPost(
                serviceProvider,
                url, cookieContainer, fileStream, null, null, encoding,
#if !NET462
                apiClient,
                certName,
#else
                cer,
#endif
                useAjax, null, timeOut, checkValidationResult, contentType: contentType);

            afterReturnText?.Invoke(url, returnText);

            return SerializerHelper.GetObject(jsonTypeInfo, returnText);
        }

        /// <summary>
        /// Post form data and get JSON
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="serviceProvider">Server provider under .NET Core; keep null for .NET Framework</param>
        /// <param name="url"></param>
        /// <param name="cookieContainer">CookieContainer; set to null if not needed</param>
        /// <param name="formData">Form data; Key maps to name, Value maps to value</param>
        /// <param name="encoding"></param>
        /// <param name="certName">Unique certificate name; keep null if not needed</param>
        /// <param name="cer">Certificate; keep null if not needed</param>
        /// <param name="useAjax">Whether to use an Ajax request</param>
        /// <param name="contentType">Content-Type in the request header; defaults to <see cref="HttpClientHelper.DEFAULT_CONTENT_TYPE"/></param>
        /// <param name="timeOut">Proxy request timeout in milliseconds</param>
        /// <param name="afterReturnText">Return JSON text and trigger before deserialization; parameters are url and returnText</param>
        /// <returns></returns>
#if NET8_0_OR_GREATER
        [RequiresDynamicCode("Runtime JSON metadata may require dynamic code. Use the PostGetJson overload with JsonTypeInfo<T> for Native AOT.")]
        [RequiresUnreferencedCode("Runtime JSON metadata may be removed by trimming. Use the PostGetJson overload with JsonTypeInfo<T> for Native AOT.")]
#endif
        public static T PostGetJson<T>(
            IServiceProvider serviceProvider,
            string url, CookieContainer cookieContainer = null, Dictionary<string, string> formData = null, Encoding encoding = null,
#if !NET462
            ApiClient apiClient = null,
            string certName = null,
#else
            X509Certificate2 cer = null,
#endif
            bool useAjax = false,
            string contentType = null,
            Action<string, string> afterReturnText = null, int timeOut = Config.TIME_OUT)
        {
            string returnText = RequestUtility.HttpPost(
                serviceProvider,
                url, cookieContainer, formData, encoding,
#if !NET462
                apiClient,
                certName,
#else
                cer,
#endif
                useAjax, null, timeOut);

            //SenparcTrace.SendApiLog(url, returnText);
            afterReturnText?.Invoke(url, returnText);

            var result = SerializerHelper.GetObject<T>(returnText);
            return result;
        }

        /// <summary>
        /// Send a Post form and deserialize the response using source-generated metadata. This overload supports Native AOT.
        /// </summary>
        public static T PostGetJson<T>(
            JsonTypeInfo<T> jsonTypeInfo,
            IServiceProvider serviceProvider,
            string url, CookieContainer cookieContainer = null, Dictionary<string, string> formData = null, Encoding encoding = null,
#if !NET462
            ApiClient apiClient = null,
            string certName = null,
#else
            X509Certificate2 cer = null,
#endif
            bool useAjax = false,
            string contentType = null,
            Action<string, string> afterReturnText = null, int timeOut = Config.TIME_OUT)
        {
            string returnText = RequestUtility.HttpPost(
                serviceProvider,
                url, cookieContainer, formData, encoding,
#if !NET462
                apiClient,
                certName,
#else
                cer,
#endif
                useAjax, null, timeOut);

            afterReturnText?.Invoke(url, returnText);

            return SerializerHelper.GetObject(jsonTypeInfo, returnText);
        }

        /// <summary>
        /// Upload data using Post and download the file or result
        /// </summary>
        /// <param name="serviceProvider">Server provider under .NET Core; keep null for .NET Framework</param>
        /// <param name="url"></param>
        /// <param name="data"></param>
        /// <param name="stream"></param>
        public static void Download(
            IServiceProvider serviceProvider,
            string url, string data, Stream stream)
        {
#if NET462
            WebClient wc = new WebClient();
            var file = wc.UploadData(url, "POST", Encoding.UTF8.GetBytes(string.IsNullOrEmpty(data) ? "" : data));
            stream.Write(file, 0, file.Length);

            //foreach (var b in file)
            //{
            //    stream.WriteByte(b);
            //}
#else
            HttpClient httpClient = serviceProvider.GetRequiredService<SenparcHttpClient>().Client;
            HttpContent hc = new StringContent(data);
            var ht = httpClient.PostAsync(url, hc);
            ht.Wait();
            var ft = ht.Result.Content.ReadAsByteArrayAsync();
            ft.Wait();
            var file = ft.Result;
            stream.Write(file, 0, file.Length);
#endif

        }

        #endregion

        #region Asynchronous Methods

        /// <summary>
        /// [Async Method] Send a Post request with optional file upload
        /// </summary>
        /// <typeparam name="T">Return data type (JSON entity type)</typeparam>
        /// <param name="serviceProvider">Server provider under .NET Core; keep null for .NET Framework</param>
        /// <param name="url">Request URL</param>
        /// <param name="cookieContainer">CookieContainer; set to null if not needed</param>
        /// <param name="encoding"></param>
        /// <param name="certName">Unique certificate name; keep null if not needed</param>
        /// <param name="cer">Certificate; keep null if not needed</param>
        /// <param name="useAjax"></param>
        /// <param name="timeOut">Proxy request timeout in milliseconds</param>
        /// <param name="fileDictionary">Files to Post (Dictionary Key=name, Value=absolute path)</param>
        /// <param name="postDataDictionary">Key-value pairs to Post (name, value)</param>
        /// <param name="contentType">Content-Type in the request header; defaults to <see cref="HttpClientHelper.DEFAULT_CONTENT_TYPE"/></param>
        /// <param name="afterReturnText">Return JSON text and trigger before deserialization; parameters are url and returnText</param>
        /// <returns></returns>
#if NET8_0_OR_GREATER
        [RequiresDynamicCode("Runtime JSON metadata may require dynamic code. Use the PostFileGetJsonAsync overload with JsonTypeInfo<T> for Native AOT.")]
        [RequiresUnreferencedCode("Runtime JSON metadata may be removed by trimming. Use the PostFileGetJsonAsync overload with JsonTypeInfo<T> for Native AOT.")]
#endif
        public static async Task<T> PostFileGetJsonAsync<T>(
            IServiceProvider serviceProvider,
            string url, CookieContainer cookieContainer = null, Dictionary<string, string> fileDictionary = null, Dictionary<string, string> postDataDictionary = null,
            Encoding encoding = null,
#if !NET462
            ApiClient apiClient = null,
            string certName = null,
#else
            X509Certificate2 cer = null,
#endif
            bool useAjax = false,
            string contentType = null,
            Action<string, string> afterReturnText = null, int timeOut = Config.TIME_OUT)
        {
            var hasFormData = postDataDictionary != null;

            using (MemoryStream ms = new MemoryStream())
            {
                postDataDictionary.FillFormDataStream(ms); //Fill formData

                string returnText = await RequestUtility.HttpPostAsync(
                    serviceProvider,
                    url, cookieContainer, ms, fileDictionary, null, encoding,
#if !NET462
                    apiClient,
                    certName,
#else
                    cer,
#endif
                    useAjax, null, hasFormData, timeOut, contentType: contentType).ConfigureAwait(false);

                afterReturnText?.Invoke(url, returnText);

                var result = SerializerHelper.GetObject<T>(returnText);
                return result;
            }
        }

        /// <summary>
        /// Asynchronously send a Post file request and deserialize the response using source-generated metadata. This overload supports Native AOT.
        /// </summary>
        public static async Task<T> PostFileGetJsonAsync<T>(
            JsonTypeInfo<T> jsonTypeInfo,
            IServiceProvider serviceProvider,
            string url, CookieContainer cookieContainer = null, Dictionary<string, string> fileDictionary = null, Dictionary<string, string> postDataDictionary = null,
            Encoding encoding = null,
#if !NET462
            ApiClient apiClient = null,
            string certName = null,
#else
            X509Certificate2 cer = null,
#endif
            bool useAjax = false,
            string contentType = null,
            Action<string, string> afterReturnText = null, int timeOut = Config.TIME_OUT)
        {
            var hasFormData = postDataDictionary != null;

            using (MemoryStream ms = new MemoryStream())
            {
                postDataDictionary.FillFormDataStream(ms);

                string returnText = await RequestUtility.HttpPostAsync(
                    serviceProvider,
                    url, cookieContainer, ms, fileDictionary, null, encoding,
#if !NET462
                    apiClient,
                    certName,
#else
                    cer,
#endif
                    useAjax, null, hasFormData, timeOut, contentType: contentType).ConfigureAwait(false);

                afterReturnText?.Invoke(url, returnText);

                return SerializerHelper.GetObject(jsonTypeInfo, returnText);
            }
        }


        /// <summary>
        /// [Async Method] Send a Post request that may include a file stream
        /// </summary>
        /// <typeparam name="T">Return data type (JSON entity type)</typeparam>
        /// <param name="serviceProvider">Server provider under .NET Core; keep null for .NET Framework</param>
        /// <param name="url">Request URL</param>
        /// <param name="cookieContainer">CookieContainer; set to null if not needed</param>
        /// <param name="fileStream">File stream</param>
        /// <param name="encoding"></param>
        /// <param name="certName">Unique certificate name; keep null if not needed</param>
        /// <param name="cer">Certificate; keep null if not needed</param>
        /// <param name="useAjax">Whether to use an Ajax request</param>
        /// <param name="timeOut">Proxy request timeout in milliseconds</param>
        /// <param name="checkValidationResult">Automatically validate server certificate callback</param>
        /// <param name="contentType">Content-Type in the request header; defaults to <see cref="HttpClientHelper.DEFAULT_CONTENT_TYPE"/></param>
        /// <param name="afterReturnText">Return JSON text and trigger before deserialization; parameters are url and returnText</param>
        /// <returns></returns>
#if NET8_0_OR_GREATER
        [RequiresDynamicCode("Runtime JSON metadata may require dynamic code. Use the PostGetJsonAsync overload with JsonTypeInfo<T> for Native AOT.")]
        [RequiresUnreferencedCode("Runtime JSON metadata may be removed by trimming. Use the PostGetJsonAsync overload with JsonTypeInfo<T> for Native AOT.")]
#endif
        public static async Task<T> PostGetJsonAsync<T>(
            IServiceProvider serviceProvider,
            string url, CookieContainer cookieContainer = null, Stream fileStream = null, Encoding encoding = null,
#if !NET462
            ApiClient apiClient = null,
            string certName = null,
#else
            X509Certificate2 cer = null,
#endif
            bool useAjax = false, bool checkValidationResult = false,
            string contentType = null,
            Action<string, string> afterReturnText = null,
            int timeOut = Config.TIME_OUT)
        {
            string returnText = await RequestUtility.HttpPostAsync(
                serviceProvider,
                url, cookieContainer, fileStream, null, null, encoding,
#if !NET462
                apiClient,
                certName,
#else
                cer,
#endif
                useAjax, null, false, timeOut, checkValidationResult, contentType).ConfigureAwait(false);

            //SenparcTrace.SendApiLog(url, returnText);
            afterReturnText?.Invoke(url, returnText);

            var result = SerializerHelper.GetObject<T>(returnText);
            return result;
        }

        /// <summary>
        /// Asynchronously send a Post stream request and deserialize the response using source-generated metadata. This overload supports Native AOT.
        /// </summary>
        public static async Task<T> PostGetJsonAsync<T>(
            JsonTypeInfo<T> jsonTypeInfo,
            IServiceProvider serviceProvider,
            string url, CookieContainer cookieContainer = null, Stream fileStream = null, Encoding encoding = null,
#if !NET462
            ApiClient apiClient = null,
            string certName = null,
#else
            X509Certificate2 cer = null,
#endif
            bool useAjax = false, bool checkValidationResult = false,
            string contentType = null,
            Action<string, string> afterReturnText = null,
            int timeOut = Config.TIME_OUT)
        {
            string returnText = await RequestUtility.HttpPostAsync(
                serviceProvider,
                url, cookieContainer, fileStream, null, null, encoding,
#if !NET462
                apiClient,
                certName,
#else
                cer,
#endif
                useAjax, null, false, timeOut, checkValidationResult, contentType).ConfigureAwait(false);

            afterReturnText?.Invoke(url, returnText);

            return SerializerHelper.GetObject(jsonTypeInfo, returnText);
        }


        /// <summary>
        /// [Async Method] Post form data and get JSON
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="serviceProvider">Server provider under .NET Core; keep null for .NET Framework</param>
        /// <param name="url"></param>
        /// <param name="cookieContainer">CookieContainer; set to null if not needed</param>
        /// <param name="formData">Form data; Key maps to name, Value maps to value</param>
        /// <param name="encoding"></param>
        /// <param name="certName">Unique certificate name; keep null if not needed</param>
        /// <param name="cer">Certificate; keep null if not needed</param>
        /// <param name="useAjax">Whether to use an Ajax request</param>
        /// <param name="contentType">Content-Type in the request header; defaults to <see cref="HttpClientHelper.DEFAULT_CONTENT_TYPE"/></param>
        /// <param name="afterReturnText">Return JSON text and trigger before deserialization; parameters are url and returnText</param>
        /// <param name="timeOut">Proxy request timeout in milliseconds</param>
        /// <returns></returns>
#if NET8_0_OR_GREATER
        [RequiresDynamicCode("Runtime JSON metadata may require dynamic code. Use the PostGetJsonAsync overload with JsonTypeInfo<T> for Native AOT.")]
        [RequiresUnreferencedCode("Runtime JSON metadata may be removed by trimming. Use the PostGetJsonAsync overload with JsonTypeInfo<T> for Native AOT.")]
#endif
        public static async Task<T> PostGetJsonAsync<T>(
            IServiceProvider serviceProvider,
            string url, CookieContainer cookieContainer = null,
            Dictionary<string, string> formData = null, Encoding encoding = null,
#if !NET462
            ApiClient apiClient = null,
            string certName = null,
#else
            X509Certificate2 cer = null,
#endif
            bool useAjax = false,
            string contentType = null,
            Action<string, string> afterReturnText = null, int timeOut = Config.TIME_OUT)
        {

            string returnText = await RequestUtility.HttpPostAsync(
                serviceProvider,
                url, cookieContainer, formData, encoding,
#if !NET462
                apiClient,
                certName,
#else
                cer,
#endif
                useAjax, null, timeOut).ConfigureAwait(false);

            //SenparcTrace.SendApiLog(url, returnText);
            afterReturnText?.Invoke(url, returnText);

            var result = SerializerHelper.GetObject<T>(returnText);
            return result;
        }

        /// <summary>
        /// Asynchronously send a Post form and deserialize the response using source-generated metadata. This overload supports Native AOT.
        /// </summary>
        public static async Task<T> PostGetJsonAsync<T>(
            JsonTypeInfo<T> jsonTypeInfo,
            IServiceProvider serviceProvider,
            string url, CookieContainer cookieContainer = null,
            Dictionary<string, string> formData = null, Encoding encoding = null,
#if !NET462
            ApiClient apiClient = null,
            string certName = null,
#else
            X509Certificate2 cer = null,
#endif
            bool useAjax = false,
            string contentType = null,
            Action<string, string> afterReturnText = null, int timeOut = Config.TIME_OUT)
        {
            string returnText = await RequestUtility.HttpPostAsync(
                serviceProvider,
                url, cookieContainer, formData, encoding,
#if !NET462
                apiClient,
                certName,
#else
                cer,
#endif
                useAjax, null, timeOut).ConfigureAwait(false);

            afterReturnText?.Invoke(url, returnText);

            return SerializerHelper.GetObject(jsonTypeInfo, returnText);
        }

        /// <summary>
        /// [Async Method] Upload data using Post and download the file or result
        /// </summary>
        /// <param name="serviceProvider">Server provider under .NET Core; keep null for .NET Framework</param>
        /// <param name="url"></param>
        /// <param name="data"></param>
        /// <param name="stream"></param>
        public static async Task DownloadAsync(
            IServiceProvider serviceProvider,
            string url, string data, Stream stream)
        {
#if NET462
            WebClient wc = new WebClient();

            var fileBytes = await wc.UploadDataTaskAsync(url, "POST", Encoding.UTF8.GetBytes(string.IsNullOrEmpty(data) ? "" : data)).ConfigureAwait(false);
            await stream.WriteAsync(fileBytes, 0, fileBytes.Length).ConfigureAwait(false);//Can also write in segments
#else
            HttpClient httpClient = serviceProvider.GetRequiredService<SenparcHttpClient>().Client;
            HttpContent hc = new StringContent(data);
            var ht = await httpClient.PostAsync(url, hc).ConfigureAwait(false);
            var fileBytes = await ht.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
            await stream.WriteAsync(fileBytes, 0, fileBytes.Length).ConfigureAwait(false);//Can also write in segments
#endif

        }

        #endregion
    }
}
