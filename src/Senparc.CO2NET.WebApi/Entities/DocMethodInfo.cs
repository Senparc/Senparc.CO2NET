using System;
using System.Collections.Generic;
using System.Text;

namespace Senparc.CO2NET.WebApi
{
    public class DocMethodInfo
    {
        public DocMethodInfo(string methodName, string paramsPart, string summary = null, Dictionary<string, string> parameters = null, string returns = null)
        {
            MethodName = methodName;
            ParamsPart = paramsPart;
            Summary = summary;
            Parameters = parameters ?? new Dictionary<string, string>();
            Returns = returns;
        }

        /// <summary>
        /// 方法名称
        /// </summary>
        public string MethodName { get; }

        /// <summary>
        /// 参数部分的原始字符串
        /// </summary>
        public string ParamsPart { get; }

        /// <summary>
        /// 方法概要说明
        /// </summary>
        public string Summary { get; }

        /// <summary>
        /// 参数字典，Key：参数名称，Value：参数说明
        /// </summary>
        public Dictionary<string, string> Parameters { get; }

        /// <summary>
        /// 返回值说明
        /// </summary>
        public string Returns { get; }
    }
}
