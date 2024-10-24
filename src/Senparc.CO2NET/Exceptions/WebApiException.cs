/*----------------------------------------------------------------
    Copyright(C) 2024 Senparc

    FileName: WebApiException.cs
    File Function Description: WebApi Exception


    Creation Identifier: Senparc - 20210714

----------------------------------------------------------------*/

using System;

namespace Senparc.CO2NET.Exceptions
{
    /// <summary>
    /// Cache Exception
    /// </summary>
    public class WebApiException : BaseException
    {
        /// <summary>
        /// WebApi Exception
        /// </summary>
        /// <param name="message"></param>
        /// <param name="inner"></param>
        /// <param name="logged"></param>
        public WebApiException(string message, Exception inner = null, bool logged = false) : base(message, inner, logged)
        {
        }
    }
}
