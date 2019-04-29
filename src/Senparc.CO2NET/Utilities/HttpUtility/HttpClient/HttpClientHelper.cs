using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

#if !NET35 && !NET40
using System.Net.Http;
#endif

namespace Senparc.CO2NET.HttpUtility
{
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


#if NETSTANDARD2_0

        /// <summary>
        /// 获取 HttpClientHandler 对象
        /// </summary>
        /// <param name="cookieContainer"></param>
        /// <param name="webProxy"></param>
        /// <returns></returns>
        public static HttpClientHandler GetHttpClientHandler(CookieContainer cookieContainer = null, IWebProxy webProxy = null, DecompressionMethods decompressionMethods = DecompressionMethods.None)
        {
            return new HttpClientHandler()
            {
                UseProxy = webProxy != null,
                Proxy = webProxy,
                UseCookies = cookieContainer != null,
                CookieContainer = cookieContainer,
                AutomaticDecompression = decompressionMethods
            };
        }

        /// <summary>
        /// 从 Response 中设置 Cookie 到 CookieContainer
        /// </summary>
        /// <param name="cookieContainer"></param>
        /// <param name="response"></param>
        public static void SetResponseCookieContainer(CookieContainer cookieContainer, HttpResponseMessage response)
        {
            if (cookieContainer == null || response==null)
            {
                return;
            }

            //收集Cookie
            if (cookieContainer != null && response.Headers.TryGetValues(Microsoft.Net.Http.Headers.HeaderNames.SetCookie, out var setCookieHeaders))
            {
                foreach (var header in setCookieHeaders)
                {
                    cookieContainer.SetCookies(response.RequestMessage.RequestUri, header);
                }
            }
        }
#endif

    }
}

