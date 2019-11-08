/*----------------------------------------------------------------
    Copyright(C) 2019 Senparc

    文件名：HttpException.cs
    文件功能描述：HttpClient 等网络请求异常


    创建标识：Senparc - 20190907

----------------------------------------------------------------*/

using System;

namespace Senparc.CO2NET.Exceptions
{
    /// <summary>
    /// 缓存异常
    /// </summary>
    public class HttpException : BaseException
    {
        /// <summary>
        /// 缓存异常构造函数
        /// </summary>
        /// <param name="message"></param>
        /// <param name="inner"></param>
        /// <param name="logged"></param>
        public HttpException(string message, Exception inner = null, bool logged = false) : base(message, inner, logged)
        {
        }
    }
}
