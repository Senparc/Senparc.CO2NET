/*----------------------------------------------------------------
    Copyright (C) 2023 Senparc

    文件名：ApiBindAttribute.cs
    文件功能描述：ApiBindAttribute 特性


    创建标识：Senparc - 20210627

    修改标识：Senparc - 20230614
    修改描述：v1.4.1 Ignore 添加 virtual 关键字

----------------------------------------------------------------*/

using Senparc.CO2NET.WebApi;
using System;
using System.Reflection;

namespace Senparc.CO2NET
{
    /// <summary>
    /// 自动绑定属性
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = true)]
    public class ApiBindAttribute : Attribute
    {
        /// <summary>
        /// 目录（平台类型），用于输出 API 的 Url 时分组
        /// </summary>
        private string Category { get; set; }
        /// <summary>
        /// 平台内唯一名称（请使用宇宙唯一名称，如： [namespace].[ClassName].[MethodName]）
        /// </summary>
        private string Name { get; set; }
        /// <summary>
        /// 是否忽略当前标签
        /// </summary>
        public virtual bool Ignore { get; set; }

        /// <summary>
        /// ApiController 的基类，默认为 ControllerBase
        /// </summary>
        public Type BaseApiControllerType { get; set; }
        /// <summary>
        /// ApiController 的基类排序，最后会使用数字最大的一个（支持负数）
        /// </summary>
        public short BaseApiControllerOrder { get; set; }

        /// <summary>
        /// 请求方法
        /// </summary>
        public ApiRequestMethod ApiRequestMethod { get; set; }

        /// <summary>
        /// ApiBindAttributes 构造函数
        /// </summary>
        public ApiBindAttribute() { }

        internal ApiBindAttribute(bool ignore)
        {
            Ignore = ignore;
        }


        /// <summary>
        /// 自动绑定属性
        /// </summary>
        /// <param name="category">目录（平台类型），用于输出 API 的 Url 时分组</param>
        public ApiBindAttribute(string category) : this(category, null, WebApi.ApiRequestMethod.GlobalDefault)
        {
        }


        /// <summary>
        /// 自动绑定属性
        /// </summary>
        /// <param name="category">目录（平台类型），用于输出 API 的 Url 时分组</param>
        /// <param name="name">平台内唯一名称（如使用 PlatformType.General，请使用宇宙唯一名称）</param>
        public ApiBindAttribute(string category, string name) : this(category, name, WebApi.ApiRequestMethod.GlobalDefault)
        {
        }

        /// <summary>
        /// 自动绑定属性
        /// </summary>
        /// <param name="category">目录（平台类型），用于输出 API 的 Url 时分组</param>
        /// <param name="name">平台内唯一名称（如使用 PlatformType.General，请使用宇宙唯一名称）</param>
        /// <param name="apiRequestMethod">当前 API 请求的类型，如果为 null，则使用本次引擎全局定义的 </param>
        public ApiBindAttribute(string category, string name, ApiRequestMethod apiRequestMethod) : this(category, name, WebApi.ApiRequestMethod.GlobalDefault, null, 0)
        {
        }



        /// <summary>
        /// 自动绑定属性
        /// </summary>
        /// <param name="category">目录（平台类型），用于输出 API 的 Url 时分组</param>
        /// <param name="name">平台内唯一名称（如使用 PlatformType.General，请使用宇宙唯一名称）</param>
        /// <param name="apiRequestMethod">当前 API 请求的类型，如果为 null，则使用本次引擎全局定义的 </param>
        /// <param name="baseApiControllerType">ApiController 的基类，默认为 ControllerBase</param>
        /// <param name="baseApiControllerOrder">ApiController 的基类排序，最后会使用数字最大的一个（支持负数）</param>
        public ApiBindAttribute(string category, string name, ApiRequestMethod apiRequestMethod, Type baseApiControllerType, short baseApiControllerOrder)
        {
            Category = category;
            Name = name;
            ApiRequestMethod = apiRequestMethod;
            BaseApiControllerType = baseApiControllerType;
            BaseApiControllerOrder = baseApiControllerOrder;
        }

        /// <summary>
        /// 获取不为空的可显示、使用的Category名称
        /// </summary>
        /// <param name="realAssemblyName">真实的当前程序集的名称（AssemblyName.Name）</param>
        /// <returns></returns>
        public string GetCategoryName(string realAssemblyName)
        {
            return Category ?? realAssemblyName;
        }

        /// <summary>
        /// 获取不为空的可显示、使用的 Category 名称
        /// </summary>
        /// <param name="methodInfo">当前方法名称</param>
        /// <returns></returns>
        public string GetCategoryName(MethodInfo methodInfo)
        {
            return GetCategoryName(methodInfo.DeclaringType.Assembly.GetName().Name);
        }

        /// <summary>
        /// 获取不为空的可显示、使用的全局唯一 Name
        /// </summary>
        /// <param name="methodInfo">当前方法名称</param>
        /// <returns></returns>
        public string GetName(MethodInfo methodInfo)
        {
            return Name ?? $"{methodInfo.DeclaringType.Name}.{methodInfo.Name}";
        }

    }
}
