/*----------------------------------------------------------------
    Copyright(C) 2024 Senparc

    FileName：BaseException.cs
    File Function Description：Base class for exceptions


    Creation Identifier：Senparc - 20180602

----------------------------------------------------------------*/

using Senparc.CO2NET.Trace;
using System;


namespace Senparc.CO2NET.Exceptions
{
    /// <summary>
    /// Base class for exceptions
    /// </summary>
#if NET462
    public class BaseException : ApplicationException
#else
    public class BaseException : Exception
#endif
    {
        /// <summary>
        /// BaseException constructor
        /// </summary>
        /// <param name="message"></param>
        /// <param name="logged"></param>
        public BaseException(string message, bool logged = false)
            : this(message, null, logged)
        {
        }

        /// <summary>
        /// BaseException
        /// </summary>
        /// <param name="message">Exception message</param>
        /// <param name="inner">Inner exception information</param>
        /// <param name="logged">Whether the log has been recorded using WeixinTrace. If not, BaseException will record a summary</param>
        public BaseException(string message, Exception inner, bool logged = false)
            : base(message, inner)
        {
            if (!logged)
            {
                //SenparcTrace.Log(string.Format("BaseException（{0}）：{1}", this.GetType().Name, message));
                SenparcTrace.BaseExceptionLog(this);
            }
        }
    }
}
