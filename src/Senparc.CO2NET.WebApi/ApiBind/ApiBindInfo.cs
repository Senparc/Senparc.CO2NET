using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Senparc.CO2NET
{
    /// <summary>
    /// ApiBind 属性所绑定的方法信息（Mapping）
    /// </summary>
    public class ApiBindInfo
    {
        /// <summary>
        /// ApiBindAttribute
        /// </summary>
        public ApiBindAttribute ApiBindAttribute { get; set; }

        /// <summary>
        /// 绑定 API 方法对象信息
        /// </summary>
        public MethodInfo MethodInfo { get; set; }

        public ApiBindInfo(ApiBindAttribute apiBindAttribute, MethodInfo methodInfo) {
            ApiBindAttribute = apiBindAttribute;
            MethodInfo = methodInfo;
        }
    }
}
