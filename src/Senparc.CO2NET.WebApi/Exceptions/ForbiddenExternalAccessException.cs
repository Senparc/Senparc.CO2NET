using Senparc.CO2NET.Exceptions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Senparc.CO2NET.WebApi.Exceptions
{
    /// <summary>
    /// 禁止外部访问
    /// </summary>
    public class ForbiddenExternalAccessException : BaseException
    {
        public ForbiddenExternalAccessException(string message = "WebApiEngine 已禁止外部访问", bool logged = false) : base(message, logged)
        {
        }

    }
}
