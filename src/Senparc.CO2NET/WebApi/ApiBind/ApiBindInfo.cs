using Senparc.CO2NET.Exceptions;
using Senparc.CO2NET.WebApi;
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
        /// 分组目录
        /// </summary>
        public string Category { get; private set; }
        /// <summary>
        /// 单个分组下唯一的名称。默认为：[类名].[方法名]
        /// </summary>
        public string GlobalName { get; private set; }

        public string Name { get; private set; }

        ///// <summary>
        ///// 绑定 API 方法对象信息
        ///// </summary>
        //public MethodInfo MethodInfo { get; set; }

        /// <summary>
        /// 绑定 API 的整个方法
        /// </summary>
        public Type ClassType { get; set; }

        /// <summary>
        /// 绑定 API 方法对象信息
        /// </summary>
        public MethodInfo MethodInfo { get; set; }

        /// <summary>
        /// ApiBind 属性作用范围：类或方法
        /// </summary>
        public ApiBindOn ApiBindOn { get; set; }

        //TODO: 添加 ignore 忽略属性
        //TODO: 根据模块可以进行忽略或开启

        public ApiBindInfo(ApiBindOn apiBindOn, string category, string globalName, string name, ApiBindAttribute apiBindAttribute, MethodInfo methodInfo)
        {
            ApiBindOn = apiBindOn;
            Category = category;
            GlobalName = globalName;
            Name = name;
            ApiBindAttribute = apiBindAttribute;
            MethodInfo = methodInfo;
            ClassType = methodInfo.DeclaringType;
        }
    }
}
