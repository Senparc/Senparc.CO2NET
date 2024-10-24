/*----------------------------------------------------------------
    Copyright(C) 2024 Senparc

    FileName: HttpException.cs
    File Function Description: HttpClient and other network request exceptions


    Creation Identifier: Senparc - 20190907

----------------------------------------------------------------*/

using System;

namespace Senparc.CO2NET.Exceptions
{
    /// <summary>
    /// HttpClient and other network request exceptions
    /// </summary>
    public class HttpException : BaseException
    {
        /// <summary>
        /// HttpClient and other network request exceptions constructor
        /// </summary>
        /// <param name="message"></param>
        /// <param name="inner"></param>
        /// <param name="logged"></param>
        public HttpException(string message, Exception inner = null, bool logged = false) : base(message, inner, logged)
        {
        }
    }
}
