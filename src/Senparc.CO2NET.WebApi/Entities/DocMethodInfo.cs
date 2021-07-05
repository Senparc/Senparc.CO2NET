using System;
using System.Collections.Generic;
using System.Text;

namespace Senparc.CO2NET.WebApi
{
    public class DocMethodInfo
    {
        public DocMethodInfo(string methodName, string paramsPart)
        {
            MethodName = methodName;
            ParamsPart = paramsPart;
        }

        public string MethodName { get; }
        public string ParamsPart { get; }
    }
}
