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

    文件名：SenparcHttpClient.cs
    文件功能描述：SenparcHttpClient，用于提供 HttpClientFactory 的自定义类


    创建标识：Senparc - 20190429

    修改标识：Senparc - 20190521
    修改描述：v0.7.3 .NET Core 提供多证书注册功能

    修改标识：Senparc - 20200220
    修改描述：v1.1.100 重构 SenparcDI

----------------------------------------------------------------*/


#if !NET45
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;

namespace Senparc.CO2NET.HttpUtility
{
    /// <summary>
    /// SenparcHttpClient，用于提供 HttpClientFactory 的自定义类
    /// </summary>
    public class SenparcHttpClient
    {
        /// <summary>
        /// HttpClient 对象
        /// </summary>
        public HttpClient Client { get; private set; }

        /// <summary>
        /// 从 HttpClientFactory 的唯一名称中获取 HttpClient 对象，并加载到 SenparcHttpClient 中
        /// </summary>
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
        /// SenparcHttpClient 构造函数
        /// </summary>
        /// <param name="httpClient"></param>
        public SenparcHttpClient(HttpClient httpClient)
        {
            //httpClient.DefaultRequestHeaders.Add("Accept", "application/vnd.github.v3+json");
            //httpClient.DefaultRequestHeaders.Add("User-Agent", "HttpClientFactory-Sample");
            Client = httpClient;
        }

        //public void SetHandler(HttpClientHandler handler)
        //{
        //}

        public void SetCookie(Uri uri, CookieContainer cookieContainer)
        {
            if (cookieContainer == null)
            {
                return;
            }

            var cookieHeader = cookieContainer.GetCookieHeader(uri);
            Client.DefaultRequestHeaders.Add(HeaderNames.Cookie, cookieHeader);
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