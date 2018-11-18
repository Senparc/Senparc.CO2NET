using Senparc.CO2NET.Exceptions;
using Senparc.CO2NET.Trace;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Senparc.CO2NET.APM.Exceptions
{
    /// <summary>
    /// APM 异常
    /// </summary>
    public class APMException : BaseException
    {
        public APMException(string message, string domain, string kindName, string method, Exception inner = null) :
            base(message, inner, true)
        {
            SenparcTrace.SendCustomLog("APM 执行出错", $@"Domain: {domain}
KindName: {kindName}
Message: {message}
Exception: {inner?.ToString()}");
        }
    }
}
