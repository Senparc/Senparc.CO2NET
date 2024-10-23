/*----------------------------------------------------------------
    Copyright (C) 2024 Senparc

    FileName：WebCodingExtensions.cs
    File Function Description：Web encoding extension class

    Creation Identifier：Senparc - 20180602

    Modification Identifier：Senparc - 20181217
    Modification Description：v0.4.1 Added encoding type selection for UrlEncode() and UrlDecode() methods in .net framework environment

----------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace Senparc.CO2NET.Extensions
{

    /// <summary>
    /// HTML and URL encoding/decoding for web pages
    /// </summary>
    public static class WebCodingExtensions
    {
        /// <summary>
        /// Encapsulates System.Web.HttpUtility.HtmlEncode
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        public static string HtmlEncode(this string html)
        {
#if NET462
            return System.Web.HttpUtility.HtmlEncode(html);
#else
            return WebUtility.HtmlEncode(html);
#endif
        }
        /// <summary>
        /// Encapsulates System.Web.HttpUtility.HtmlDecode
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        public static string HtmlDecode(this string html)
        {
#if NET462
            return System.Web.HttpUtility.HtmlDecode(html);
#else
            return WebUtility.HtmlDecode(html);
#endif
        }

#if NET462
        /// <summary>
        /// Encapsulates System.Web.HttpUtility.UrlEncode
        /// <para>Note: In .NET Core, escaped letters are uppercase</para>
        /// </summary>
        /// <param name="url"></param>
        /// <param name="encoding">Encoding, default is UTF8</param>
        /// <returns></returns>
        public static string UrlEncode(this string url, Encoding encoding = null)
        {
            encoding = encoding ?? Encoding.UTF8;
            return System.Web.HttpUtility.UrlEncode(url, encoding);
        }
#else
        /// <summary>
        /// Encapsulates WebUtility.UrlEncode
        /// <para>Note: In .NET Core, escaped letters are uppercase</para>
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static string UrlEncode(this string url)
        {
            return WebUtility.UrlEncode(url);// Escaped letters are uppercase
        }
#endif

#if NET462
        /// <summary>
        /// Encapsulates System.Web.HttpUtility.UrlDecode
        /// </summary>
        /// <param name="url"></param>
        /// <param name="encoding">Encoding, default is UTF8</param>
        /// <returns></returns>
        public static string UrlDecode(this string url, Encoding encoding = null)
        {
            encoding = encoding ?? Encoding.UTF8;
            return System.Web.HttpUtility.UrlDecode(url, encoding);
        }
#else
        /// <summary>
        /// Encapsulates WebUtility.UrlDecode
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static string UrlDecode(this string url)
        {
            return WebUtility.UrlDecode(url);
        }
#endif


        /// <summary>
        /// <para>Encodes parameter names/values in the URL to a valid format.</para>
        /// <para>Can solve issues like: If the parameter name is tvshow and the value is Tom&amp;Jerry, without encoding, the URL might be: http://a.com/?tvshow=Tom&amp;Jerry&amp;year=1965. After encoding: http://a.com/?tvshow=Tom%26Jerry&amp;year=1965 </para>
        /// <para>Characters that often cause issues in practice: '&amp;', '?', '=' etc.</para>
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static string AsUrlData(this string data)
        {
            if (data == null)
            {
                return null;
            }
            return Uri.EscapeDataString(data);
        }
    }
}
