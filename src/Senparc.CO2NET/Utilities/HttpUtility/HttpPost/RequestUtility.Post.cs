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

    文件名：RequestUtility.Post.cs
    文件功能描述：获取请求结果（Post）


    创建标识：Senparc - 20171006

    修改描述：移植Post方法过来

    修改标识：Senparc - 20180516
    修改描述：v4.21.1-rc1  解决 RequestUtility.HttpResponsePost() 和 HttpPostAsync() 方法
                           在 .net core 下没有及时关闭 postStream 的问题

    修改标识：Senparc - 20180602
    修改描述：v4.22.2 完善 RequestUtility.HttpPost_Common_NetCore() 字符串信息提交过程

    -- CO2NET --

    修改标识：Senparc - 20181009
    修改描述：v0.2.15 Post 方法添加 headerAddition参数

    修改标识：Senparc - 20190429
    修改描述：v0.7.0 优化 HttpClient，重构 RequestUtility（包括 Post 和 Get），引入 HttpClientFactory 机制

    修改标识：Senparc - 20190521
    修改描述：v0.7.3 .NET Core 提供多证书注册功能

    修改标识：Senparc - 20190811
    修改描述：v0.8.7 RequestUtility.Post() 方法添加新功能：使用文件流模拟 Form 表单提交

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
using Senparc.CO2NET.Utilities.HttpUtility.HttpPost;

#if NET45
using System.Web;
#else
using System.Net.Http;
using System.Net.Http.Headers;
#endif
#if !NET45
using Senparc.CO2NET.WebProxy;
using Senparc.CO2NET.Exceptions;
using System.Linq;
#endif

namespace Senparc.CO2NET.HttpUtility
{
    /// <summary>
    /// HTTP 请求工具类
    /// </summary>
    public static partial class RequestUtility
    {
        #region 静态公共方法




#if NET45

        /// <summary>
        /// 给.NET Framework使用的HttpPost请求公共设置方法
        /// </summary>
        /// <param name="url"></param>
        /// <param name="cookieContainer"></param>
        /// <param name="postStream"></param>
        /// <param name="fileDictionary">需要上传的文件，Key：对应要上传的Name，Value：本地文件名，或文件内容的Base64编码</param>
        /// <param name="refererUrl"></param>
        /// <param name="encoding"></param>
        /// <param name="cer"></param>
        /// <param name="useAjax"></param>
        /// <param name="headerAddition">header附加信息</param>
        /// <param name="timeOut"></param>
        /// <param name="checkValidationResult"></param>
        /// <param name="contentType"></param>
        /// <returns></returns>
        public static HttpWebRequest HttpPost_Common_Net45(string url, CookieContainer cookieContainer = null,
            Stream postStream = null, Dictionary<string, string> fileDictionary = null, string refererUrl = null,
            Encoding encoding = null, X509Certificate2 cer = null, bool useAjax = false, Dictionary<string, string> headerAddition = null,
            int timeOut = Config.TIME_OUT, bool checkValidationResult = false, string contentType = HttpClientHelper.DEFAULT_CONTENT_TYPE)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "POST";
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

            #region 处理Form表单文件上传
            var formUploadFile = fileDictionary != null && fileDictionary.Count > 0;//是否用Form上传文件
            if (formUploadFile)
            {
                contentType = "multipart/form-data";

                //通过表单上传文件
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
                                //fileNameOrFileData 中储存的储存的是 Stream
                                var fileName = Path.GetFileName(formFileData.GetAvaliableFileName(SystemTime.NowTicks.ToString()));
                                formdata = string.Format(fileFormdataTemplate, file.Key, fileName);
                            }
                            else
                            {
                                //fileNameOrFileData 中储存的储存的可能是文件地址或备注

                                //准备文件流
                                using (var fileStream = FileHelper.GetFileStream(fileNameOrFileData))
                                {
                                    if (fileStream != null)
                                    {
                                        //存在文件
                                        memoryStream.Seek(0, SeekOrigin.Begin);
                                        fileStream.CopyTo(memoryStream);
                                        formdata = string.Format(fileFormdataTemplate, file.Key, Path.GetFileName(fileNameOrFileData));
                                        fileStream.Dispose();
                                    }
                                    else
                                    {
                                        //不存在文件或只是注释
                                        formdata = string.Format(dataFormdataTemplate, file.Key, file.Value);
                                    }
                                }
                            }

                            //统一处理
                            var formdataBytes = Encoding.UTF8.GetBytes(postStream.Length == 0 ? formdata.Substring(2, formdata.Length - 2) : formdata);//第一行不需要换行
                            postStream.Write(formdataBytes, 0, formdataBytes.Length);

                            //写入文件
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
                //结尾
                var footer = Encoding.UTF8.GetBytes("\r\n--" + boundary + "--\r\n");
                postStream.Write(footer, 0, footer.Length);

                //request.ContentType = string.Format("multipart/form-data; boundary={0}", boundary);//request.ContentType在下方统一设置
                contentType = string.Format("multipart/form-data; boundary={0}", boundary);
            }
            else
            {
                if (postStream.Length > 0)
                {
                    if (contentType == HttpClientHelper.DEFAULT_CONTENT_TYPE)
                    {
                        //如果ContentType是默认值，则设置成为二进制流
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

#if !NET45
        /// <summary>
        /// 给.NET Core使用的HttpPost请求公共设置方法
        /// </summary>
        /// <param name="url"></param>
        /// <param name="hc"></param>
        /// <param name="cookieContainer"></param>
        /// <param name="postStream"></param>
        /// <param name="fileDictionary">需要上传的文件，Key：对应要上传的Name，Value：本地文件名，或文件内容的Base64编码</param>
        /// <param name="refererUrl"></param>
        /// <param name="encoding"></param>
        /// <param name="certName">证书唯一名称，如果不需要则保留null</param>
        /// <param name="useAjax"></param>
        /// <param name="headerAddition">header附加信息</param>
        /// <param name="timeOut"></param>
        /// <param name="checkValidationResult"></param>
        /// <param name="contentType"></param>
        /// <returns></returns>
        public static HttpClient HttpPost_Common_NetCore(
            IServiceProvider serviceProvider,
            string url, out HttpContent hc, CookieContainer cookieContainer = null,
            Stream postStream = null, Dictionary<string, string> fileDictionary = null, string refererUrl = null,
            Encoding encoding = null, string certName = null, bool useAjax = false, Dictionary<string, string> headerAddition = null,
            int timeOut = Config.TIME_OUT, bool checkValidationResult = false, string contentType = HttpClientHelper.DEFAULT_CONTENT_TYPE)
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

            //TODO:此处 handler并没有被使用到，因此 cer 实际无法传递（这个也是 .net core 目前针对多 cer 场景的一个问题）

            var senparcHttpClient = SenparcHttpClient.GetInstanceByName(serviceProvider, certName);
            senparcHttpClient.SetCookie(new Uri(url), cookieContainer);//设置Cookie

            HttpClient client = senparcHttpClient.Client;
            HttpClientHeader(client, refererUrl, useAjax, headerAddition, timeOut);

        #region 处理Form表单文件上传

            var formUploadFile = fileDictionary != null && fileDictionary.Count > 0;//是否用Form上传文件
            if (formUploadFile)
            {
                contentType = "multipart/form-data";

                //通过表单上传文件
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

                        //准备文件流
                        var memoryStream = new MemoryStream();//这里不能释放，否则如在请求的时候 memoryStream 已经关闭会发生错误
                        if (formFileData.TryLoadStream(memoryStream).ConfigureAwait(false).GetAwaiter().GetResult())
                        {
                            //fileNameOrFileData 中储存的储存的是 Stream
                            fileName = Path.GetFileName(formFileData.GetAvaliableFileName(SystemTime.NowTicks.ToString()));
                        }
                        else
                        {
                            //fileNameOrFileData 中储存的储存的可能是文件地址或备注
                            using (var fileStream = FileHelper.GetFileStream(fileNameOrFileData))
                            {
                                if (fileStream != null)
                                {
                                    //存在文件
                                    fileStream.CopyTo(memoryStream);//TODO:可以使用异步方法
                                    fileName = Path.GetFileName(fileNameOrFileData);
                                    fileStream.Dispose();
                                }
                                else
                                {
                                    //只是注释
                                    multipartFormDataContent.Add(new StringContent(file.Value), "\"" + file.Key + "\"");
                                }
                            }
                        }

                        if (memoryStream.Length > 0)
                        {
                            //有文件内容
                            //multipartFormDataContent.Add(new StreamContent(memoryStream), file.Key, Path.GetFileName(fileName)); //报流已关闭的异常

                            memoryStream.Seek(0, SeekOrigin.Begin);
                            multipartFormDataContent.Add(CreateFileContent(memoryStream, file.Key, fileName), file.Key, fileName);
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
                if (postStream.Length > 0)
                {
                    if (contentType == HttpClientHelper.DEFAULT_CONTENT_TYPE)
                    {
                        //如果ContentType是默认值，则设置成为二进制流
                        contentType = "application/octet-stream";
                    }

                    //contentType = "application/x-www-form-urlencoded";
                }

                hc = new StreamContent(postStream);

                hc.Headers.ContentType = new MediaTypeHeaderValue(contentType);

                //使用Url格式Form表单Post提交的时候才使用application/x-www-form-urlencoded
                //去掉注释以测试Request.Body为空的情况
                //hc.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");
            }

            //HttpContentHeader(hc, timeOut);
        #endregion

            if (!string.IsNullOrEmpty(refererUrl))
            {
                client.DefaultRequestHeaders.Referrer = new Uri(refererUrl);
            }

            return client;
        }

#endif

        #endregion

        #region 同步方法

        /// <summary>
        /// 使用Post方法获取字符串结果，常规提交
        /// </summary>
        /// <returns></returns>
        public static string HttpPost(
            IServiceProvider serviceProvider,
            string url, CookieContainer cookieContainer = null, Dictionary<string, string> formData = null,
            Encoding encoding = null,
#if !NET45
            string certName = null,
#else
            X509Certificate2 cer = null,
#endif            
            bool useAjax = false, Dictionary<string, string> headerAddition = null, int timeOut = Config.TIME_OUT,
            bool checkValidationResult = false)
        {
            MemoryStream ms = new MemoryStream();
            formData.FillFormDataStream(ms);//填充formData

            string contentType = HttpClientHelper.GetContentType(formData);

            return HttpPost(
                serviceProvider,
                url, cookieContainer, ms, null, null, encoding,
#if !NET45
                certName,
#else
                cer,
#endif
                useAjax, headerAddition, timeOut, checkValidationResult, contentType);
        }

        /// <summary>
        /// 使用Post方法获取字符串结果
        /// </summary>
        /// <param name="url"></param>
        /// <param name="cookieContainer"></param>
        /// <param name="postStream"></param>
        /// <param name="fileDictionary">需要上传的文件，Key：对应要上传的Name，Value：本地文件名，或文件内容的Base64编码</param>
        /// <param name="encoding"></param>
        /// <param name="certName">证书唯一名称，如果不需要则保留null</param>
        /// <param name="cer">证书，如果不需要则保留null</param>
        /// <param name="useAjax"></param>
        /// <param name="headerAddition">header 附加信息</param>
        /// <param name="timeOut"></param>
        /// <param name="checkValidationResult">验证服务器证书回调自动验证</param>
        /// <param name="contentType"></param>
        /// <param name="refererUrl"></param>
        /// <returns></returns>
        public static string HttpPost(
            IServiceProvider serviceProvider,
            string url, CookieContainer cookieContainer = null, Stream postStream = null,
            Dictionary<string, string> fileDictionary = null, string refererUrl = null, Encoding encoding = null,
#if !NET45
            string certName = null,
#else
            X509Certificate2 cer = null,
#endif
            bool useAjax = false, Dictionary<string, string> headerAddition = null, int timeOut = Config.TIME_OUT, bool checkValidationResult = false,
            string contentType = HttpClientHelper.DEFAULT_CONTENT_TYPE)
        {
            if (cookieContainer == null)
            {
                cookieContainer = new CookieContainer();
            }

            var senparcResponse = HttpResponsePost(
                serviceProvider,
                url, cookieContainer, postStream, fileDictionary, refererUrl, encoding,
#if !NET45
                certName,
#else
                cer,
#endif
                useAjax, headerAddition, timeOut, checkValidationResult, contentType);

            var response = senparcResponse.Result;//获取响应信息


#if NET45

            #region 已经使用方法重用
            /*
            
            var request = HttpPost_Common_Net45(url, cookieContainer, postStream, fileDictionary, refererUrl, encoding, cer, useAjax, timeOut, checkValidationResult);

            #region 输入二进制流
            if (postStream != null)
            {
                postStream.Position = 0;

                //直接写入流
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

                postStream.Close();//关闭文件访问
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
            HttpClientHelper.SetResponseCookieContainer(cookieContainer, response);//设置 Cookie

            //var response = senparcResponse.Result;

            if (response.Content.Headers.ContentType != null &&
                response.Content.Headers.ContentType.CharSet != null &&
                response.Content.Headers.ContentType.CharSet.ToLower().Contains("utf8"))
            {
                response.Content.Headers.ContentType.CharSet = "utf-8";
            }

            var retString = response.Content.ReadAsStringAsync().ConfigureAwait(false).GetAwaiter().GetResult();
            return retString;

            //t1.Wait();
            //return t1.Result;
#endif
        }


        /// <summary>
        /// 使用Post方法获取HttpWebResponse或HttpResponseMessage对象，本方法独立使用时通常用于测试）
        /// </summary>
        /// <param name="url"></param>
        /// <param name="cookieContainer"></param>
        /// <param name="postStream"></param>
        /// <param name="fileDictionary">需要上传的文件，Key：对应要上传的Name，Value：本地文件名，或文件内容的Base64编码</param>
        /// <param name="encoding"></param>
        /// <param name="certName">证书唯一名称，如果不需要则保留null</param>
        /// <param name="cer">证书，如果不需要则保留null</param>
        /// <param name="useAjax"></param>
        /// <param name="headerAddition">header附加信息</param>
        /// <param name="timeOut"></param>
        /// <param name="checkValidationResult">验证服务器证书回调自动验证</param>
        /// <param name="contentType"></param>
        /// <param name="refererUrl"></param>
        /// <returns></returns>
        public static SenparcHttpResponse HttpResponsePost(
            IServiceProvider serviceProvider,
            string url, CookieContainer cookieContainer = null, Stream postStream = null,
            Dictionary<string, string> fileDictionary = null, string refererUrl = null, Encoding encoding = null,
#if !NET45
            string certName = null,
#else
            X509Certificate2 cer = null,
#endif
            bool useAjax = false, Dictionary<string, string> headerAddition = null, int timeOut = Config.TIME_OUT,
            bool checkValidationResult = false, string contentType = HttpClientHelper.DEFAULT_CONTENT_TYPE)
        {
            if (cookieContainer == null)
            {
                cookieContainer = new CookieContainer();
            }

            var postStreamIsDefaultNull = postStream == null;
            if (postStreamIsDefaultNull)
            {
                postStream = new MemoryStream();
            }

#if NET45
            var request = HttpPost_Common_Net45(url, cookieContainer, postStream, fileDictionary, refererUrl, encoding, cer, useAjax, headerAddition, timeOut, checkValidationResult, contentType);

            #region 输入二进制流
            if (postStream != null && postStream.Length > 0)
            {
                postStream.Position = 0;

                //直接写入流
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

                postStream.Close();//关闭文件访问
            }
            #endregion

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            return new SenparcHttpResponse(response);
#else
            HttpContent hc;
            var client = HttpPost_Common_NetCore(serviceProvider, url, out hc, cookieContainer, postStream, fileDictionary, refererUrl, encoding, certName, useAjax, headerAddition, timeOut, checkValidationResult, contentType);

            var response = client.PostAsync(url, hc).ConfigureAwait(false).GetAwaiter().GetResult();//获取响应信息

            HttpClientHelper.SetResponseCookieContainer(cookieContainer, response);//设置 Cookie

            try
            {
                if (postStreamIsDefaultNull && postStream.Length > 0)
                {
                    postStream.Close();
                }

                hc.Dispose();//关闭HttpContent（StreamContent）
            }
            catch (BaseException ex)
            {
            }

            return new SenparcHttpResponse(response);
#endif
        }

        #endregion

#if !NET35 && !NET40
        #region 异步方法

        /// <summary>
        /// 使用Post方法获取字符串结果，常规提交
        /// </summary>
        /// <returns></returns>
        public static async Task<string> HttpPostAsync(
            IServiceProvider serviceProvider,
            string url, CookieContainer cookieContainer = null,
            Dictionary<string, string> formData = null, Encoding encoding = null,
#if !NET45
            string certName = null,
#else
            X509Certificate2 cer = null,
#endif
            bool useAjax = false, Dictionary<string, string> headerAddition = null, int timeOut = Config.TIME_OUT,
            bool checkValidationResult = false)
        {
            MemoryStream ms = new MemoryStream();
            await formData.FillFormDataStreamAsync(ms).ConfigureAwait(false);//填充formData

            string contentType = HttpClientHelper.GetContentType(formData);

            return await HttpPostAsync(
                serviceProvider,
                url, cookieContainer, ms, null, null, encoding,
#if !NET45
                certName,
#else
                cer,
#endif
                useAjax, headerAddition, timeOut, checkValidationResult, contentType).ConfigureAwait(false); ;
        }


        /// <summary>
        /// 使用Post方法获取字符串结果
        /// </summary>
        /// <param name="url"></param>
        /// <param name="cookieContainer"></param>
        /// <param name="postStream"></param>
        /// <param name="fileDictionary">需要上传的文件，Key：对应要上传的Name，Value：本地文件名，或文件内容的Base64编码</param>
        /// <param name="certName">证书唯一名称，如果不需要则保留null</param>
        /// <param name="cer"></param>
        /// <param name="useAjax"></param>
        /// <param name="headerAddition">header附加信息</param>
        /// <param name="timeOut"></param>
        /// <param name="checkValidationResult">验证服务器证书回调自动验证</param>
        /// <param name="contentType"></param>
        /// <param name="refererUrl"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public static async Task<string> HttpPostAsync(
            IServiceProvider serviceProvider,
            string url, CookieContainer cookieContainer = null, Stream postStream = null,
            Dictionary<string, string> fileDictionary = null, string refererUrl = null, Encoding encoding = null,
#if !NET45
            string certName = null,
#else
            X509Certificate2 cer = null,
#endif
            bool useAjax = false, Dictionary<string, string> headerAddition = null, int timeOut = Config.TIME_OUT, bool checkValidationResult = false,
            string contentType = HttpClientHelper.DEFAULT_CONTENT_TYPE)
        {
            if (cookieContainer == null)
            {
                cookieContainer = new CookieContainer();
            }

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
#if !NET45
                certName,
#else
                cer,
#endif
                useAjax, headerAddition, timeOut, checkValidationResult, contentType).ConfigureAwait(false); ;

            var response = senparcResponse.Result;//获取响应信息



            //Console.WriteLine($"{System.Threading.Thread.CurrentThread.Name} - FINISH- {SystemTime.DiffTotalMS(dt1):###,###} ms");


#if NET45
            #region 已经使用方法重用
            /*

            var request = HttpPost_Common_Net45(url, cookieContainer, postStream, fileDictionary, refererUrl, encoding, cer, useAjax,headerAddition, timeOut, checkValidationResult);

            #region 输入二进制流
            if (postStream != null && postStream.Length > 0)
            {
                postStream.Position = 0;

                //直接写入流
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

                postStream.Close();//关闭文件访问
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
            HttpClientHelper.SetResponseCookieContainer(cookieContainer, response);//设置 Cookie

            #region 已经使用方法重用
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

                hc.Dispose();//关闭HttpContent（StreamContent）
            }
            catch (BaseException ex)
            {
            }
            */
            #endregion

            var retString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            return retString;
#endif
        }

        /// <summary>
        /// 使用Post方法获取HttpWebResponse或HttpResponseMessage对象，本方法独立使用时通常用于测试）
        /// </summary>
        /// <param name="url"></param>
        /// <param name="cookieContainer"></param>
        /// <param name="postStream"></param>
        /// <param name="fileDictionary">需要上传的文件，Key：对应要上传的Name，Value：本地文件名，或文件内容的Base64编码</param>
        /// <param name="encoding"></param>
        /// <param name="certName">证书唯一名称，如果不需要则保留null</param>
        /// <param name="cer">证书，如果不需要则保留null</param>
        /// <param name="useAjax"></param>
        /// <param name="headerAddition">header附加信息</param>
        /// <param name="timeOut"></param>
        /// <param name="checkValidationResult">验证服务器证书回调自动验证</param>
        /// <param name="contentType"></param>
        /// <param name="refererUrl"></param>
        /// <returns></returns>
        public static async Task<SenparcHttpResponse> HttpResponsePostAsync(
            IServiceProvider serviceProvider,
            string url, CookieContainer cookieContainer = null, Stream postStream = null,
            Dictionary<string, string> fileDictionary = null, string refererUrl = null, Encoding encoding = null,
#if !NET45
            string certName = null,
#else
            X509Certificate2 cer = null,
#endif
            bool useAjax = false, Dictionary<string, string> headerAddition = null, int timeOut = Config.TIME_OUT,
            bool checkValidationResult = false, string contentType = HttpClientHelper.DEFAULT_CONTENT_TYPE)
        {
            if (cookieContainer == null)
            {
                cookieContainer = new CookieContainer();
            }

            var postStreamIsDefaultNull = postStream == null;
            if (postStreamIsDefaultNull)
            {
                postStream = new MemoryStream();
            }

#if NET45
            var request = HttpPost_Common_Net45(url, cookieContainer, postStream, fileDictionary, refererUrl, encoding, cer, useAjax, headerAddition, timeOut, checkValidationResult, contentType);

            #region 输入二进制流
            if (postStream != null && postStream.Length > 0)
            {
                postStream.Position = 0;

                //直接写入流
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

                postStream.Close();//关闭文件访问
            }
            #endregion

            HttpWebResponse response = (HttpWebResponse)(await request.GetResponseAsync().ConfigureAwait(false));
            return new SenparcHttpResponse(response);
#else
            HttpContent hc;
            var client = HttpPost_Common_NetCore(serviceProvider, url, out hc, cookieContainer, postStream, fileDictionary, refererUrl, encoding, certName, useAjax, headerAddition, timeOut, checkValidationResult, contentType);

            var response = await client.PostAsync(url, hc).ConfigureAwait(false);//获取响应信息

            HttpClientHelper.SetResponseCookieContainer(cookieContainer, response);//设置 Cookie

            try
            {
                if (postStreamIsDefaultNull && postStream.Length > 0)
                {
                    postStream.Close();
                }

                hc.Dispose();//关闭HttpContent（StreamContent）
            }
            catch (BaseException ex)
            {
            }

            return new SenparcHttpResponse(response);
#endif
        }


        #endregion
#endif

    }
}
