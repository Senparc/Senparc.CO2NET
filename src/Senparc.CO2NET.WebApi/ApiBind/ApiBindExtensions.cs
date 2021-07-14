/*----------------------------------------------------------------
    Copyright (C) 2021 Senparc

    文件名：ApiBindExtensions.cs
    文件功能描述：ApiBindAttribute 的扩展方法


    创建标识：Senparc - 20210713

----------------------------------------------------------------*/

using System.Reflection;
using System.Text.RegularExpressions;

namespace Senparc.CO2NET.WebApi
{
    public static class ApiBindExtensions
    {
        /// <summary>
        /// 获取动态程序集的命名空间
        /// </summary>
        /// <param name="methodInfo"></param>
        /// <param name="realAssemblyName">真实的当前程序集的名称（AssemblyName.Name）</param>
        /// <returns></returns>
        public static string GetDynamicCategory(this ApiBindAttribute attr, MethodInfo methodInfo, string realAssemblyName)
        {
            string newNameSpace;
            string categoryName = attr.GetCategoryName(realAssemblyName);
            if (!string.IsNullOrEmpty(categoryName))
            {
                newNameSpace = $"Senparc.DynamicWebApi.{Regex.Replace(categoryName, @"[\s\.\(\)]", "")}";//TODO:可以换成缓存命名空间等更加特殊的前缀
            }
            else
            {
                newNameSpace = $"Senparc.DynamicWebApi.{methodInfo.DeclaringType.Assembly.GetName().Name}";
            }
            return newNameSpace;
        }
    }
}
