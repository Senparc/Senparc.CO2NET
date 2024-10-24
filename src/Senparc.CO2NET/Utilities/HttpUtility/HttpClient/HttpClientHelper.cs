using System;
#region Apache License Version 2.0
/*----------------------------------------------------------------

Copyright 2024 Suzhou Senparc Network Technology Co.,Ltd.

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

    FileName：HttpClientHelper.cs
    File Function Description：HttpClient related helper class


    Creation Identifier：Senparc - 20190429

    Modification Identifier：Senparc - 20190521
    Modification Description：v0.7.2.1 Fixed potential exception when cookieContainer is null in GetHttpClientHandler() method

    Modification Identifier：Senparc - 20190911
    Modification Description：v0.8.10 Optimized SetResponseCookieContainer() method to prevent null exception (theoretically should not occur)

----------------------------------------------------------------*/

using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Senparc.CO2NET.Extensions;
using Senparc.CO2NET.Trace;
using Senparc.CO2NET.Exceptions;
using System.Net.Http;

namespace Senparc.CO2NET.HttpUtility
{
    /// <summary>
    /// HttpClient related helper class
    /// </summary>
    public static class HttpClientHelper
    {
        internal const string DEFAULT_CONTENT_TYPE = "text/xml";//"application/octet-stream"

        /// <summary>
        /// Get Content
        /// </summary>
        /// <param name="formData">Form submission fields</param>
        /// <returns></returns>
        internal static string GetContentType(Dictionary<string, string> formData)
        {
            string contentType = DEFAULT_CONTENT_TYPE;
            if (formData != null && formData.Count > 0)
            {
                //contentType = "application/x-www-form-urlencoded";//Use specific ContentType if form submission is needed
            }
            return contentType;
        }


#if !NET462

        /// <summary>
        /// Get HttpClientHandler object
        /// </summary>
        /// <param name="cookieContainer"></param>
        /// <param name="webProxy"></param>
        /// <param name="decompressionMethods"></param>
        /// <returns></returns>
        public static HttpClientHandler GetHttpClientHandler(CookieContainer cookieContainer = null, IWebProxy webProxy = null, DecompressionMethods decompressionMethods = DecompressionMethods.None)
        {
            var httpClientHandler = new HttpClientHandler()
            {
                UseProxy = webProxy != null,
                Proxy = webProxy,
                UseCookies = cookieContainer != null,
                //CookieContainer = cookieContainer,//If null, an exception will occur when assigning
                AutomaticDecompression = decompressionMethods
            };

            if (cookieContainer != null)
            {
                httpClientHandler.CookieContainer = cookieContainer;
            }
            return httpClientHandler;
        }

        /// <summary>
        /// Set Cookie from Response to CookieContainer
        /// </summary>
        /// <param name="cookieContainer"></param>
        /// <param name="response"></param>
        public static void SetResponseCookieContainer(CookieContainer cookieContainer, HttpResponseMessage response)
        {
            if (cookieContainer == null || response == null)
            {
                return;
            }

            //Collect Cookie
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
                //Theoretically, no exception should be thrown here
                _ = new HttpException($"SetResponseCookieContainer 过程失败！{ex.Message}", ex);
            }

        }
#endif

    }
}

