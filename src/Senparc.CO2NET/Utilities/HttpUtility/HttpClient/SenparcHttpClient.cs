
#if NETSTANDARD2_0
using Microsoft.Net.Http.Headers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;

namespace Senparc.CO2NET.HttpUtility
{
    public class SenparcHttpClient
    {
        public HttpClient Client { get; private set; }

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