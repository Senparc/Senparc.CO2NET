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
    Copyright (C) 2020 Senparc COCONET

    FileName：HttpExtensions.cs
    File Function Description：A series of Http extensions in ASP.NET Core


    Creation Identifier：Senparc - 20180526

    Modification Identifier：Senparc - 20180721
    Modification Description：v0.2.2  Added support for NETSTANDARD2_0

    -- Migrated from CO2NET to CO2NET.AspNet --
    
    Modification Identifier：Senparc - 20180721
    Modification Description：v0.1.0  Migrated from CO2NET to CO2NET.AspNet

----------------------------------------------------------------*/

#if !NET462

using Microsoft.AspNetCore.Http.Features;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace Microsoft.AspNetCore.Http
{
    /// <summary>
    /// RequestExtension
    /// </summary>
    public static class RequestExtension
    {
        private const string NullIpAddress = "::1";

        /// <summary>
        /// Determines whether it is a local request
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public static bool IsLocal(this HttpRequest req)
        {
            var connection = req.HttpContext.Connection;
            if (connection.RemoteIpAddress.IsSet())
            {
                //We have a remote address set up
                return connection.LocalIpAddress.IsSet()
                    //Is local is same as remote, then we are local
                    ? connection.RemoteIpAddress.Equals(connection.LocalIpAddress)
                    //else we are remote if the remote IP address is not a loopback address
                    : IPAddress.IsLoopback(connection.RemoteIpAddress);
            }

            return true;
        }

        ///// <summary>
        ///// Determines whether it is a local request
        ///// </summary>
        ///// <param name="req"></param>
        ///// <returns></returns>
        //public static bool IsLocal(this HttpRequest req)
        //{
        //    var connection = req.HttpContext.Connection;
        //    if (connection.RemoteIpAddress != null)
        //    {
        //        if (connection.LocalIpAddress != null)
        //        {
        //            return connection.RemoteIpAddress.Equals(connection.LocalIpAddress);
        //        }
        //        else
        //        {
        //            return IPAddress.IsLoopback(connection.RemoteIpAddress);
        //        }
        //    }

        //    // for in memory TestServer or when dealing with default connection info
        //    if (connection.RemoteIpAddress == null && connection.LocalIpAddress == null)
        //    {
        //        return true;
        //    }

        //    return false;
        //}

        private static bool IsSet(this IPAddress address)
        {
            return address != null && address.ToString() != NullIpAddress;
        }

        /// <summary>
        /// Determines whether the specified HTTP request is an AJAX request.
        /// </summary>
        /// 
        /// <returns>
        /// true if the specified HTTP request is an AJAX request; otherwise, false.
        /// </returns>
        /// <param name="request">The HTTP request.</param><exception cref="T:System.ArgumentNullException">The <paramref name="request"/> parameter is null (Nothing in Visual Basic).</exception>
        public static bool IsAjaxRequest(this HttpRequest request)
        {
            if (request == null)
                throw new ArgumentNullException("request");

            if (request.Headers != null)
                return request.Headers["X-Requested-With"] == "XMLHttpRequest";
            return false;
        }

        /// <summary>
        /// Usually a full path starting with /
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static string PathAndQuery(this HttpRequest request)
        {
            if (request == null)
                throw new ArgumentNullException("request");

            return request.Path + request.QueryString;
        }

        /// <summary>
        /// Get the source page
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static string UrlReferrer(this HttpRequest request)
        {
            return request.Headers["Referer"].ToString();
        }

        /// <summary>
        /// Return absolute address
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static string AbsoluteUri(this HttpRequest request)
        {
            string schemeUpper = request.Scheme.ToUpper();//Protocol (uppercase)
            var host = request.Host.Host;
            var port = request.Host.Port ?? -1;//Port (may be null in .NET Core)
            string portSetting = null;//Port part in Url
            if (port == -1 || //This condition only occurs in .net core when Host.Port == null
                (schemeUpper == "HTTP" && port == 80) ||
                (schemeUpper == "HTTPS" && port == 443))
            {
                portSetting = "";//Use default value
            }
            else
            {
                portSetting = ":" + port;//Add port
            }

            var absoluteUri = string.Concat(
                          request.Scheme,
                          "://",
                          host,//Without port number
                          portSetting,//Port number
                          request.PathBase.ToUriComponent(),
                          request.Path.ToUriComponent(),
                          request.QueryString.ToUriComponent());

            return absoluteUri;
        }

        /// <summary>
        /// Get client information
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static string UserAgent(this HttpRequest request)
        {
            return request.Headers["User-Agent"].ToString();
        }

        /// <summary>
        /// Get client address (IP)
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static IPAddress UserHostAddress(this HttpContext httpContext)
        {
            return httpContext.Features.Get<IHttpConnectionFeature>()?.RemoteIpAddress;
        }

    }
}
#endif