/*----------------------------------------------------------------
    Copyright(C) 2024 Senparc

    文件名：WebApiException.cs
    文件功能描述：WebApi 异常


    创建标识：Senparc - 20210714

----------------------------------------------------------------*/

using System;

namespace Senparc.CO2NET.Exceptions
{
    /// <summary>
    /// 缓存异常
    /// </summary>
    public class WebApiException : BaseException
    {
        /// <summary>
        /// WebApi 异常
        /// </summary>
        /// <param name="message"></param>
        /// <param name="inner"></param>
        /// <param name="logged"></param>
        public WebApiException(string message, Exception inner = null, bool logged = false) : base(message, inner, logged)
        {
        }
    }
}
