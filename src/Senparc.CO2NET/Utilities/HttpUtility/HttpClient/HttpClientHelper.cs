using System;
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

    文件名：HttpClientHelper.cs
    文件功能描述：HttpClient 相关帮助类


    创建标识：Senparc - 20190429

    修改标识：Senparc - 20190521
    修改描述：v0.7.2.1 解决 GetHttpClientHandler() 方法中 cookieContainer 为 null 可能发生的异常

    修改标识：Senparc - 20190911
    修改描述：v0.8.10 优化 SetResponseCookieContainer() 方法，防止 null 异常（理论上不会出现）

----------------------------------------------------------------*/

using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Senparc.CO2NET.Extensions;
using Senparc.CO2NET.Trace;
using Senparc.CO2NET.Exceptions;

#if !NET35 && !NET40
using System.Net.Http;
#endif

namespace Senparc.CO2NET.HttpUtility
{
    /// <summary>
    /// HttpClient 相关帮助类
    /// </summary>
    public static class HttpClientHelper
    {
        internal const string DEFAULT_CONTENT_TYPE = "text/xml";

        /// <summary>
        /// 获取 Content
        /// </summary>
        /// <param name="formData">提交表单字段</param>
        /// <returns></returns>
        internal static string GetContentType(Dictionary<string, string> formData)
        {
            string contentType = DEFAULT_CONTENT_TYPE;
            if (formData != null && formData.Count > 0)
            {
                contentType = "application/x-www-form-urlencoded";//如果需要提交表单，则使用特定的ContentType
            }
            return contentType;
        }


#if !NET45

        /// <summary>
        /// 获取 HttpClientHandler 对象
        /// </summary>
        /// <param name="cookieContainer"></param>
        /// <param name="webProxy"></param>
        /// <returns></returns>
        public static HttpClientHandler GetHttpClientHandler(CookieContainer cookieContainer = null, IWebProxy webProxy = null, DecompressionMethods decompressionMethods = DecompressionMethods.None)
        {
            var httpClientHandler = new HttpClientHandler()
            {
                UseProxy = webProxy != null,
                Proxy = webProxy,
                UseCookies = cookieContainer != null,
                //CookieContainer = cookieContainer,//如果为null，赋值的时候会出现异常
                AutomaticDecompression = decompressionMethods
            };

            if (cookieContainer != null)
            {
                httpClientHandler.CookieContainer = cookieContainer;
            }
            return httpClientHandler;
        }

        /// <summary>
        /// 从 Response 中设置 Cookie 到 CookieContainer
        /// </summary>
        /// <param name="cookieContainer"></param>
        /// <param name="response"></param>
        public static void SetResponseCookieContainer(CookieContainer cookieContainer, HttpResponseMessage response)
        {
            if (cookieContainer == null || response == null)
            {
                return;
            }

            //收集Cookie
            try
            {
                IEnumerable<string> setCookieHeaders = null;
                if (cookieContainer != null && response.Headers != null &&
                    response.Headers.Contains(Microsoft.Net.Http.Headers.HeaderNames.SetCookie) && response.Headers.TryGetValues(Microsoft.Net.Http.Headers.HeaderNames.SetCookie, out setCookieHeaders))
                {
                    if (setCookieHeaders == null)
                    {
                        throw new Exception("setCookieHeaders is null");
                    }

                    foreach (var header in setCookieHeaders)
                    {
                        //Console.WriteLine("Header:" + header.ToJson());
                        cookieContainer.SetCookies(response.RequestMessage.RequestUri, header);
                    }
                }
            }
            catch (Exception ex)
            {
                //理论上这里不应该抛出异常
                _ = new HttpException($"SetResponseCookieContainer 过程失败！{ex.Message}", ex);
            }

        }
#endif

    }
}

