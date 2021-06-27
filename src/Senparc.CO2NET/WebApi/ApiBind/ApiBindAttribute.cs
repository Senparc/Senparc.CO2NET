using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Senparc.CO2NET
{
    /// <summary>
    /// 自动绑定属性
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, Inherited = true, AllowMultiple = true)]
    public class ApiBindAttribute : Attribute
    {
        /// <summary>
        /// 目录（平台类型），用于输出 API 的 Url 时分组
        /// </summary>
        public string Category { get; set; }
        /// <summary>
        /// 平台内唯一名称（请使用宇宙唯一名称，如： [namespace].[ClassName].[MethodName]）
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// ApiBindAttributes 构造函数
        /// </summary>
        public ApiBindAttribute() { }

        /// <summary>
        /// 自动绑定属性
        /// </summary>
        /// <param name="category">目录（平台类型），用于输出 API 的 Url 时分组</param>
        /// <param name="name">平台内唯一名称（如使用 PlatformType.General，请使用宇宙唯一名称）</param>
        public ApiBindAttribute(string category, string name)
        {
            Category = category;
            Name = name;
        }
    }
}
