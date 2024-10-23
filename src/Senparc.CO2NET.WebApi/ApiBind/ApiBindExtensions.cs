/*----------------------------------------------------------------
    Copyright (C) 2023 Senparc

    FileName: ApiBindExtensions.cs
    File Function Description: Extension methods for ApiBindAttribute


    Creation Identifier: Senparc - 20210713

----------------------------------------------------------------*/

using System.Reflection;
using System.Text.RegularExpressions;

namespace Senparc.CO2NET.WebApi
{
    public static class ApiBindExtensions
    {
        /// <summary>
        /// Get the namespace of the dynamic assembly
        /// </summary>
        /// <param name="methodInfo"></param>
        /// <param name="realAssemblyName">The real name of the current assembly (AssemblyName.Name)</param>
        /// <returns></returns>
        public static string GetDynamicCategory(this ApiBindAttribute attr, MethodInfo methodInfo, string realAssemblyName)
        {
            string newNameSpace;
            string categoryName = attr.GetCategoryName(realAssemblyName);
            if (!string.IsNullOrEmpty(categoryName))
            {
                newNameSpace = $"Senparc.DynamicWebApi.{Regex.Replace(categoryName, @"[\s\(\)]", "")}";//TODO: Can be replaced with cached namespace or other more specific prefixes
            }
            else
            {
                newNameSpace = $"Senparc.DynamicWebApi.{methodInfo.DeclaringType.Assembly.GetName().Name}";
            }
            return newNameSpace;
        }
    }
}
