/*----------------------------------------------------------------
    Copyright(C) 2024 Senparc

    FileName：CacheException.cs
    File Function Description：Cache Exception


    Creation Identifier：Senparc - 20180728

----------------------------------------------------------------*/

using System;

namespace Senparc.CO2NET.Exceptions
{
    /// <summary>
    /// Cache Exception
    /// </summary>
    public class CacheException : BaseException
    {
        /// <summary>
        /// Cache Exception Constructor
        /// </summary>
        /// <param name="message"></param>
        /// <param name="inner"></param>
        /// <param name="logged"></param>
        public CacheException(string message, Exception inner = null, bool logged = false) : base(message, inner, logged)
        {
        }
    }
}
