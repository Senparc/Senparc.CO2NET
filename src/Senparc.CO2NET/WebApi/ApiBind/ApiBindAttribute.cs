/*----------------------------------------------------------------
    Copyright (C) 2024 Senparc

    FileName: ApiBindAttribute.cs
    File Function Description: ApiBindAttribute Attribute


    Creation Identifier: Senparc - 20210627

    Modification Identifier: Senparc - 20230614
    Modification Description: v1.4.1 Ignore added virtual keyword

    Modification Identifier：Senparc - 20241119
    Modification Description：v3.0.0-beta3 add GetGlobalName() method and rename GetName() to GetApiBindAttrName()

----------------------------------------------------------------*/

using Senparc.CO2NET.WebApi;
using System;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Senparc.CO2NET
{
    /// <summary>
    /// Auto bind property
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = true)]
    public class ApiBindAttribute : Attribute
    {
        /// <summary>
        /// Category (platform type), used for grouping when outputting API Url
        /// </summary>
        private string Category { get; set; }
        /// <summary>
        /// Unique name within the platform (please use a globally unique name, e.g., [namespace].[ClassName].[MethodName])
        /// </summary>
        private string Name { get; set; }
        /// <summary>
        /// Whether to ignore the current tag
        /// </summary>
        public virtual bool Ignore { get; set; }

        /// <summary>
        /// Base class of ApiController, default is ControllerBase
        /// </summary>
        public Type BaseApiControllerType { get; set; }
        /// <summary>
        /// Base class order of ApiController, the one with the largest number will be used last (supports negative numbers)
        /// </summary>
        public short BaseApiControllerOrder { get; set; }

        /// <summary>
        /// Request method
        /// </summary>
        public ApiRequestMethod ApiRequestMethod { get; set; }

        /// <summary>
        /// Constructor of ApiBindAttributes
        /// </summary>
        public ApiBindAttribute() { }

        internal ApiBindAttribute(bool ignore)
        {
            Ignore = ignore;
        }


        /// <summary>
        /// Auto bind property
        /// </summary>
        /// <param name="category">Category (platform type), used for grouping when outputting API Url</param>
        public ApiBindAttribute(string category) : this(category, null, WebApi.ApiRequestMethod.GlobalDefault)
        {
        }


        /// <summary>
        /// Auto bind property
        /// </summary>
        /// <param name="category">Category (platform type), used for grouping when outputting API Url</param>
        /// <param name="name">Unique name within the platform (if using PlatformType.General, please use a globally unique name)</param>
        public ApiBindAttribute(string category, string name) : this(category, name, WebApi.ApiRequestMethod.GlobalDefault)
        {
        }

        /// <summary>
        /// Auto bind property
        /// </summary>
        /// <param name="category">Category (platform type), used for grouping when outputting API Url</param>
        /// <param name="name">Unique name within the platform (if using PlatformType.General, please use a globally unique name)</param>
        /// <param name="apiRequestMethod">Current API request type, if null, the globally defined type of this engine will be used</param>
        public ApiBindAttribute(string category, string name, ApiRequestMethod apiRequestMethod)
            : this(category, name, WebApi.ApiRequestMethod.GlobalDefault, null, 0)
        {
        }



        /// <summary>
        /// Auto bind property
        /// </summary>
        /// <param name="category">Category (platform type), used for grouping when outputting API Url</param>
        /// <param name="name">Unique name within the platform (if using PlatformType.General, please use a globally unique name). If input empty string, default value is "ClassTypeName.MethodName"</param>
        /// <param name="apiRequestMethod">Current API request type, if null, the globally defined type of this engine will be used</param>
        /// <param name="baseApiControllerType">Base class of ApiController, default is ControllerBase</param>
        /// <param name="baseApiControllerOrder">Base class order of ApiController, the one with the largest number will be used last (supports negative numbers)</param>
        public ApiBindAttribute(string category, string name, ApiRequestMethod apiRequestMethod, Type baseApiControllerType, short baseApiControllerOrder)
        {
            Category = category;
            Name = name;
            ApiRequestMethod = apiRequestMethod;
            BaseApiControllerType = baseApiControllerType;
            BaseApiControllerOrder = baseApiControllerOrder;
        }

        /// <summary>
        /// Get non-null, displayable, usable Category name
        /// </summary>
        /// <param name="realAssemblyName">Real name of the current assembly (AssemblyName.Name)</param>
        /// <returns></returns>
        public string GetCategoryName(string realAssemblyName)
        {
            return Category ?? realAssemblyName;
        }

        /// <summary>
        /// Get non-null, displayable, usable Category name
        /// </summary>
        /// <param name="methodInfo">Current method name</param>
        /// <returns></returns>
        public string GetCategoryName(MethodInfo methodInfo)
        {
            return GetCategoryName(methodInfo.DeclaringType.Assembly.GetName().Name);
        }

        /// <summary>
        /// Get non-null, displayable, usable globally unique Name
        /// </summary>
        /// <param name="methodInfo">Current method name</param>
        /// <returns></returns>
        public string GetApiBindAttrName(MethodInfo methodInfo)
        {
            return Name ?? $"{methodInfo.DeclaringType.Name}.{methodInfo.Name}";
        }

        /// <summary>
        /// Get globally unique name
        /// </summary>
        /// <returns></returns>
        public string GetGlobalName(MethodInfo methodInfo)
        {
            var categoryName = GetCategoryName(methodInfo);
            var apiBindAttrName = this.GetApiBindAttrName(methodInfo);
            return ApiBindAttribute.GetGlobalName(categoryName, apiBindAttrName);
        }


        /// <summary>
        /// Get globally unique name
        /// </summary>
        /// <param name="category">Category (platform type), used for grouping when outputting API Url</param>
        /// <param name="apiBindAttrName">Common name across assemblies (e.g., CustomApi.SendText)</param>
        /// <returns></returns>
        public static string GetGlobalName(string category, string apiBindAttrName)
        {
            Console.WriteLine($"category / apiBindAttrName:{category} /{apiBindAttrName}");
            return $"{category}:{apiBindAttrName}";//TODO: Generate globally unique name
        }


        //public string GetApiPath()
        //{ 

        //}
    }
}
