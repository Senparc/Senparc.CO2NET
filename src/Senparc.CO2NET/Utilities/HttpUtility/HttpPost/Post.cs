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

    文件名：Post.cs
    文件功能描述：Post


    创建标识：Senparc - 20150211

    修改标识：Senparc - 20150303
    修改描述：整理接口

    修改标识：Senparc - 20150312
    修改描述：开放代理请求超时时间

    修改标识：zhanghao-kooboo - 20150316
    修改描述：增加

    修改标识：Senparc - 20150407
    修改描述：发起Post请求方法修改，为了上传永久视频素材
 
    修改标识：Senparc - 20160720
    修改描述：增加了PostFileGetJsonAsync的异步方法（与之前的方法多一个参数）

    修改标识：Senparc - 20170409
    修改描述：v4.11.9 修改Download方法

    修改标识：Senparc - 20190429
    修改描述：v0.7.0 优化 HttpClient，重构 RequestUtility（包括 Post 和 Get），引入 HttpClientFactory 机制

    修改标识：Senparc - 20190521
    修改描述：v0.7.3 .NET Core 提供多证书注册功能
    
----------------------------------------------------------------*/



using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Senparc.CO2NET.Helpers;
using System.Net.Http;

#if NET45
using System.Web.Script.Serialization;
using System.Security.Cryptography.X509Certificates;
#else
using Microsoft.Extensions.DependencyInjection;
#endif


namespace Senparc.CO2NET.HttpUtility
{
    /// <summary>
    /// Post 请求处理
    /// </summary>
    public static class Post
    {
        #region 同步方法

        /// <summary>
        /// 发起Post请求，可上传文件
        /// </summary>
        /// <typeparam name="T">返回数据类型（Json对应的实体）</typeparam>
        /// <param name="url">请求Url</param>
        /// <param name="cookieContainer">CookieContainer，如果不需要则设为null</param>
        /// <param name="encoding"></param>
        /// <param name="certName">证书唯一名称，如果不需要则保留null</param>
        /// <param name="cer">证书，如果不需要则保留null</param>
        /// <param name="useAjax"></param>
        /// <param name="timeOut">代理请求超时时间（毫秒）</param>
        /// <param name="fileDictionary">需要Post的文件（Dictionary 的 Key=name，Value=绝对路径）</param>
        /// <param name="postDataDictionary">需要Post的键值对（name,value）</param>
        /// <param name="afterReturnText">返回JSON本文，并在进行序列化之前触发，参数分别为：url、returnText</param>
        /// <returns></returns>
        public static T PostFileGetJson<T>(
            IServiceProvider serviceProvider,
            string url, CookieContainer cookieContainer = null, Dictionary<string, string> fileDictionary = null,
            Dictionary<string, string> postDataDictionary = null,
            Encoding encoding = null,
#if !NET45
            string certName = null,
#else
            X509Certificate2 cer = null,
#endif
            bool useAjax = false,
            Action<string, string> afterReturnText = null, int timeOut = Config.TIME_OUT)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                postDataDictionary.FillFormDataStream(ms); //填充formData

                string returnText = RequestUtility.HttpPost(
                    serviceProvider,
                    url, cookieContainer, ms, fileDictionary, null, encoding,
#if !NET45
                    certName,
#else
                    cer,
#endif
                    useAjax, null, timeOut);

                afterReturnText?.Invoke(url, returnText);

                var result = SerializerHelper.GetObject<T>(returnText);
                return result;
            }
        }

        /// <summary>
        /// 发起Post请求，可包含文件流
        /// </summary>
        /// <typeparam name="T">返回数据类型（Json对应的实体）</typeparam>
        /// <param name="url">请求Url</param>
        /// <param name="cookieContainer">CookieContainer，如果不需要则设为null</param>
        /// <param name="fileStream">文件流</param>
        /// <param name="encoding"></param>
        /// <param name="certName">证书唯一名称，如果不需要则保留null</param>
        /// <param name="cer">证书，如果不需要则保留null</param>
        /// <param name="useAjax">是否使用Ajax请求</param>
        /// <param name="timeOut">代理请求超时时间（毫秒）</param>
        /// <param name="checkValidationResult">验证服务器证书回调自动验证</param>
        /// <param name="afterReturnText">返回JSON本文，并在进行序列化之前触发，参数分别为：url、returnText</param>
        /// <returns></returns>
        public static T PostGetJson<T>(
            IServiceProvider serviceProvider,
            string url, CookieContainer cookieContainer = null, Stream fileStream = null, Encoding encoding = null,
#if !NET45
            string certName = null,
#else
            X509Certificate2 cer = null,
#endif
            bool useAjax = false, bool checkValidationResult = false, Action<string, string> afterReturnText = null,
            int timeOut = Config.TIME_OUT)
        {
            string returnText = RequestUtility.HttpPost(
                serviceProvider,
                url, cookieContainer, fileStream, null, null, encoding,
#if !NET45
                certName,
#else
                cer,
#endif
                useAjax, null, timeOut, checkValidationResult);

            //SenparcTrace.SendApiLog(url, returnText);
            afterReturnText?.Invoke(url, returnText);

            var result = SerializerHelper.GetObject<T>(returnText);
            return result;
        }

        /// <summary>
        /// Form表单Post数据，获取JSON
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="url"></param>
        /// <param name="cookieContainer">CookieContainer，如果不需要则设为null</param>
        /// <param name="formData">表单数据，Key对应name，Value对应value</param>
        /// <param name="encoding"></param>
        /// <param name="certName">证书唯一名称，如果不需要则保留null</param>
        /// <param name="cer">证书，如果不需要则保留null</param>
        /// <param name="useAjax">是否使用Ajax请求</param>
        /// <param name="timeOut">代理请求超时时间（毫秒）</param>
        /// <param name="afterReturnText">返回JSON本文，并在进行序列化之前触发，参数分别为：url、returnText</param>
        /// <returns></returns>
        public static T PostGetJson<T>(
            IServiceProvider serviceProvider,
            string url, CookieContainer cookieContainer = null, Dictionary<string, string> formData = null, Encoding encoding = null,
#if !NET45
            string certName = null,
#else
            X509Certificate2 cer = null,
#endif
            bool useAjax = false, Action<string, string> afterReturnText = null, int timeOut = Config.TIME_OUT)
        {
            string returnText = RequestUtility.HttpPost(
                serviceProvider,
                url, cookieContainer, formData, encoding,
#if !NET45
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
        /// 使用Post方法上传数据并下载文件或结果
        /// </summary>
        /// <param name="url"></param>
        /// <param name="data"></param>
        /// <param name="stream"></param>
        public static void Download(
            IServiceProvider serviceProvider,
            string url, string data, Stream stream)
        {
#if NET45
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

#if !NET35 && !NET40
        #region 异步方法

        /// <summary>
        /// 【异步方法】发起Post请求，可上传文件
        /// </summary>
        /// <typeparam name="T">返回数据类型（Json对应的实体）</typeparam>
        /// <param name="url">请求Url</param>
        /// <param name="cookieContainer">CookieContainer，如果不需要则设为null</param>
        /// <param name="encoding"></param>
        /// <param name="certName">证书唯一名称，如果不需要则保留null</param>
        /// <param name="cer">证书，如果不需要则保留null</param>
        /// <param name="useAjax"></param>
        /// <param name="timeOut">代理请求超时时间（毫秒）</param>
        /// <param name="fileDictionary">需要Post的文件（Dictionary 的 Key=name，Value=绝对路径）</param>
        /// <param name="postDataDictionary">需要Post的键值对（name,value）</param>
        /// <param name="afterReturnText">返回JSON本文，并在进行序列化之前触发，参数分别为：url、returnText</param>
        /// <returns></returns>
        public static async Task<T> PostFileGetJsonAsync<T>(
            IServiceProvider serviceProvider,
            string url, CookieContainer cookieContainer = null, Dictionary<string, string> fileDictionary = null, Dictionary<string, string> postDataDictionary = null,
            Encoding encoding = null,
#if !NET45
            string certName = null,
#else
            X509Certificate2 cer = null,
#endif            
            bool useAjax = false,
            Action<string, string> afterReturnText = null, int timeOut = Config.TIME_OUT)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                postDataDictionary.FillFormDataStream(ms); //填充formData

                string returnText = await RequestUtility.HttpPostAsync(
                    serviceProvider,
                    url, cookieContainer, ms, fileDictionary, null, encoding,
#if !NET45
                    certName,
#else
                    cer,
#endif
                    useAjax, null, timeOut).ConfigureAwait(false); ;

                afterReturnText?.Invoke(url, returnText);

                var result = SerializerHelper.GetObject<T>(returnText);
                return result;
            }
        }


        /// <summary>
        /// 【异步方法】发起Post请求，可包含文件流
        /// </summary>
        /// <typeparam name="T">返回数据类型（Json对应的实体）</typeparam>
        /// <param name="url">请求Url</param>
        /// <param name="cookieContainer">CookieContainer，如果不需要则设为null</param>
        /// <param name="fileStream">文件流</param>
        /// <param name="encoding"></param>
        /// <param name="certName">证书唯一名称，如果不需要则保留null</param>
        /// <param name="cer">证书，如果不需要则保留null</param>
        /// <param name="useAjax">是否使用Ajax请求</param>
        /// <param name="timeOut">代理请求超时时间（毫秒）</param>
        /// <param name="checkValidationResult">验证服务器证书回调自动验证</param>
        /// <param name="afterReturnText">返回JSON本文，并在进行序列化之前触发，参数分别为：url、returnText</param>
        /// <returns></returns>
        public static async Task<T> PostGetJsonAsync<T>(
            IServiceProvider serviceProvider,
            string url, CookieContainer cookieContainer = null, Stream fileStream = null, Encoding encoding = null,
#if !NET45
            string certName = null,
#else
            X509Certificate2 cer = null,
#endif
            bool useAjax = false, bool checkValidationResult = false, Action<string, string> afterReturnText = null,
            int timeOut = Config.TIME_OUT)
        {
            string returnText = await RequestUtility.HttpPostAsync(
                serviceProvider,
                url, cookieContainer, fileStream, null, null, encoding,
#if !NET45
                certName,
#else
                cer,
#endif
                useAjax, null, timeOut, checkValidationResult).ConfigureAwait(false); ;

            //SenparcTrace.SendApiLog(url, returnText);
            afterReturnText?.Invoke(url, returnText);

            var result = SerializerHelper.GetObject<T>(returnText);
            return result;
        }


        /// <summary>
        /// 【异步方法】Form表单Post数据，获取JSON
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="url"></param>
        /// <param name="cookieContainer">CookieContainer，如果不需要则设为null</param>
        /// <param name="formData">表单数据，Key对应name，Value对应value</param>
        /// <param name="encoding"></param>
        /// <param name="certName">证书唯一名称，如果不需要则保留null</param>
        /// <param name="cer">证书，如果不需要则保留null</param>
        /// <param name="useAjax">是否使用Ajax请求</param>
        /// <param name="timeOut">代理请求超时时间（毫秒）</param>
        /// <param name="afterReturnText">返回JSON本文，并在进行序列化之前触发，参数分别为：url、returnText</param>
        /// <returns></returns>
        public static async Task<T> PostGetJsonAsync<T>(
            IServiceProvider serviceProvider,
            string url, CookieContainer cookieContainer = null, Dictionary<string, string> formData = null, Encoding encoding = null,
#if !NET45
            string certName = null,
#else
            X509Certificate2 cer = null,
#endif
            bool useAjax = false, Action<string, string> afterReturnText = null, int timeOut = Config.TIME_OUT)
        {
            string returnText = await RequestUtility.HttpPostAsync(
                serviceProvider,
                url, cookieContainer, formData, encoding,
#if !NET45
                certName,
#else
                cer,
#endif
                useAjax, null, timeOut).ConfigureAwait(false); ;

            //SenparcTrace.SendApiLog(url, returnText);
            afterReturnText?.Invoke(url, returnText);

            var result = SerializerHelper.GetObject<T>(returnText);
            return result;
        }

        /// <summary>
        /// 【异步方法】使用Post方法上传数据并下载文件或结果
        /// </summary>
        /// <param name="url"></param>
        /// <param name="data"></param>
        /// <param name="stream"></param>
        public static async Task DownloadAsync(
            IServiceProvider serviceProvider,
            string url, string data, Stream stream)
        {
#if NET45
            WebClient wc = new WebClient();

            var fileBytes = await wc.UploadDataTaskAsync(url, "POST", Encoding.UTF8.GetBytes(string.IsNullOrEmpty(data) ? "" : data)).ConfigureAwait(false);
            await stream.WriteAsync(fileBytes, 0, fileBytes.Length).ConfigureAwait(false);//也可以分段写入
#else
            HttpClient httpClient = serviceProvider.GetRequiredService<SenparcHttpClient>().Client;
            HttpContent hc = new StringContent(data);
            var ht = await httpClient.PostAsync(url, hc).ConfigureAwait(false);
            var fileBytes = await ht.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
            await stream.WriteAsync(fileBytes, 0, fileBytes.Length).ConfigureAwait(false);//也可以分段写入
#endif

        }

        #endregion
#endif
    }
}
