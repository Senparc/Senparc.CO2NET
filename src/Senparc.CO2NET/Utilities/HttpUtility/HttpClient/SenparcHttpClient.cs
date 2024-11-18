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

    FileName：SenparcHttpClient.cs
    File Function Description：SenparcHttpClient, used to provide custom class for HttpClientFactory


    Creation Identifier：Senparc - 20190429

    Modification Identifier：Senparc - 20190521
    Modification Description：v0.7.3 .NET Core provides multi-certificate registration function

    Modification Identifier：Senparc - 20200220
    Modification Description：v1.1.100 refactored SenparcDI

    Modification Identifier：Senparc - 20221115
    Modification Description：v2.1.3 special handling for Cookie in .NET 7.0
    
    Modification Identifier：Senparc - 20241119
    Modification Description：v3.0.0-beta3 Add ApiClientName property

----------------------------------------------------------------*/


#if !NET462
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;
using Senparc.CO2NET.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;

namespace Senparc.CO2NET.HttpUtility
{
    /// <summary>
    /// SenparcHttpClient, used to provide custom class for HttpClientFactory
    /// </summary>
    public class SenparcHttpClient
    {
        /// <summary>
        /// HttpClient object
        /// </summary>
        public HttpClient Client { get; private set; }

        /// <summary>
        /// ApiClient Name
        /// </summary>
        public string ApiClientName { get; set; }

        /// <summary>
        /// Get HttpClient object from the unique name of HttpClientFactory and load it into SenparcHttpClient
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <param name="httpClientName"></param>
        /// <returns></returns>
        public static SenparcHttpClient GetInstanceByName(IServiceProvider serviceProvider, string httpClientName)
        {
            if (!string.IsNullOrEmpty(httpClientName))
            {
                var clientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();
                var httpClient = clientFactory.CreateClient(httpClientName);
                return new SenparcHttpClient(httpClient);
            }

            return serviceProvider.GetRequiredService<SenparcHttpClient>();
        }

        /// <summary>
        /// Get HttpClient object from the unique name of HttpClientFactory and load it into SenparcHttpClient
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <param name="httpClientType"></param>
        /// <returns></returns>
        public static SenparcHttpClient GetInstanceByType(IServiceProvider serviceProvider, Type httpClientType = null)
        {
            if (httpClientType != null || typeof(HttpClient).IsAssignableFrom(httpClientType))
            {
                var clientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();
                var httpClient = serviceProvider.GetService(httpClientType) as HttpClient;
                return new SenparcHttpClient(httpClient);
            }

            return serviceProvider.GetRequiredService<SenparcHttpClient>();
        }

        /// <summary>
        /// SenparcHttpClient constructor
        /// </summary>
        /// <param name="httpClient"></param>
        public SenparcHttpClient(HttpClient httpClient)
        {
            //httpClient.DefaultRequestHeaders.Add("Accept", "application/vnd.github.v3+json");
            //httpClient.DefaultRequestHeaders.Add("User-Agent", "HttpClientFactory-Sample");
            Client = httpClient;
        }

        public void SetCookie(Uri uri, CookieContainer cookieContainer)
        {
            if (cookieContainer == null)
            {
                return;
            }

            var cookieHeader = cookieContainer.GetCookieHeader(uri);

            // .NET 7 will throw an exception if an empty string is passed here:
            // System.FormatException: The format of value '' is invalid.
            if (!cookieHeader.IsNullOrEmpty())
            {
                Client.DefaultRequestHeaders.Add(HeaderNames.Cookie, cookieHeader);
            }
        }


        ///// <summary>
        ///// Read web cookies
        ///// </summary>
        //public static CookieContainer ReadCookies(this HttpResponseMessage response)
        //{
        //    var pageUri = response.RequestMessage.RequestUri;

        //    var cookieContainer = new CookieContainer();
        //    IEnumerable<string> cookies;
        //    if (response.Headers.TryGetValues("set-cookie", out cookies))
        //    {
        //        foreach (var c in cookies)
        //        {
        //            cookieContainer.SetCookies(pageUri, c);
        //        }
        //    }

        //    return cookieContainer;
        //}

    }
}
#endif