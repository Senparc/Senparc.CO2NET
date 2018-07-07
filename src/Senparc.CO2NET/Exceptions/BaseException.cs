/*----------------------------------------------------------------
    Copyright(C) 2018 Senparc

    文件名：BaseException.cs
    文件功能描述：异常基类


    创建标识：Senparc - 20180602

----------------------------------------------------------------*/

using Senparc.CO2NET.Trace;
using System;


namespace Senparc.CO2NET.Exceptions
{
    /// <summary>
    /// 异常基类
    /// </summary>
#if NET35 || NET40 || NET45
    public class BaseException : ApplicationException
#else
    public class BaseException : Exception
#endif
    {
        /// <summary>
        /// BaseException 构造函数
        /// </summary>
        /// <param name="message"></param>
        /// <param name="logged"></param>
        public BaseException(string message, bool logged = false)
            : this(message, null, logged)
        {
        }

        /// <summary>
        /// WeixinException
        /// </summary>
        /// <param name="message">异常消息</param>
        /// <param name="inner">内部异常信息</param>
        /// <param name="logged">是否已经使用WeixinTrace记录日志，如果没有，WeixinException会进行概要记录</param>
        public BaseException(string message, Exception inner, bool logged = false)
            : base(message, inner)
        {
            if (!logged)
            {
                //WeixinTrace.Log(string.Format("WeixinException（{0}）：{1}", this.GetType().Name, message));
                SenparcTrace.BaseExceptionLog(this);
            }
        }
    }
}
