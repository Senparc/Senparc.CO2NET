#region Apache License Version 2.0
/*----------------------------------------------------------------

Copyright 2025 Suzhou Senparc Network Technology Co.,Ltd.

Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file
except in compliance with the License. You may obtain a copy of the License at

http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software distributed under the
License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND,
either express or implied. See the License for the specific language governing permissions
and limitations under the License.

Detail: https://github.com/Senparc/Senparc.CO2NET/blob/master/LICENSE

----------------------------------------------------------------*/
#endregion Apache License Version 2.0

/*----------------------------------------------------------------
    Copyright (C) 2025 Senparc

    FileName：EntityUtility.cs
    File Function Description：Entity utility class


    Creation Identifier：Senparc - 20160808（v4.6.0）

    
    Modification Identifier：Senparc - 20180623
    Modification Description：v0.1.3 Added EntityUtility.FillSystemType<>() method to handle Int32[] and Int64[]

----------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using Senparc.CO2NET.Helpers;

namespace Senparc.CO2NET.Utilities
{
    /// <summary>
    /// Entity utility class
    /// </summary>
    public static class EntityUtility
    {
        /// <summary>
        /// Convert object to specified type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="convertibleValue"></param>
        /// <returns></returns>
        public static T ConvertTo<T>(this IConvertible convertibleValue)
        {
            if (null == convertibleValue)
            {
                return default(T);
            }
            
            var t = typeof(T);
#if NET462
            if (t.IsGenericType)
#else
            if (t.GetTypeInfo().IsGenericType)
#endif
            {
                if (t.GetGenericTypeDefinition() != typeof(Nullable<>))
                {
                    throw new InvalidCastException(string.Format("Invalid cast from type \"{0}\" to type \"{1}\".", convertibleValue.GetType().FullName, typeof(T).FullName));
                }

                t = Nullable.GetUnderlyingType(t);
            }

            return (T)Convert.ChangeType(convertibleValue, t);
        }


        /// <summary>
        /// Fill value to property
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <param name="prop"></param>
        /// <param name="value"></param>
        public static void FillSystemType<T>(T entity, PropertyInfo prop, IConvertible value)
        {
            FillSystemType(entity, prop, value, prop.PropertyType);
        }

        /// <summary>
        /// Fill value to property (force using specified type)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <param name="prop"></param>
        /// <param name="value"></param>
        /// <param name="specialType"></param>
        public static void FillSystemType<T>(T entity, PropertyInfo prop, IConvertible value, Type specialType)
        {
            object setValue = null;
            if (value.GetType() != specialType)
            {
                switch (specialType.Name)
                {
                    case "Boolean":
                        setValue = value.ConvertTo<bool>();
                        break;
                    case "DateTime":
                        setValue = DateTimeHelper.GetDateTimeFromXml(value.ToString());
                        break;
                    case "DateTimeOffset":
                        setValue = DateTimeHelper.GetDateTimeOffsetFromXml(value.ToString());
                        break;
                    case "Int32":
                        setValue = value.ConvertTo<int>();
                        break;
                    case "Int32[]":
                        setValue = Array.ConvertAll(value.ConvertTo<string>().Split(','), int.Parse);
                        break;
                    case "Int64":
                        setValue = value.ConvertTo<long>();
                        break;
                    case "Int64[]":
                        setValue = Array.ConvertAll(value.ConvertTo<string>().Split(','), long.Parse);
                        break;
                    case "Double":
                        setValue = value.ConvertTo<double>();
                        break;
                    case "String":
                        setValue = value.ToString(CultureInfo.InvariantCulture);
                        break;
                    default:
                        setValue = value;
                        break;
                }
            }

            switch (specialType.Name)
            {
                case "Nullable`1": //Nullable object
                    {
                        if (!string.IsNullOrEmpty(value as string))
                        {
                            var genericArguments = prop.PropertyType.GetGenericArguments();
                            FillSystemType(entity, prop, value, genericArguments[0]);
                        }
                        else
                        {
                            prop.SetValue(entity, null, null);//Default is usually null
                        }
                        break;
                    }
                //case "String":
                //    goto default;
                //case "Boolean":
                //case "DateTime":
                //case "Int32":
                //case "Int64":
                //case "Double":
                default:
                    prop.SetValue(entity, setValue ?? value, null);
                    break;
            }
        }

        ///// <summary>
        ///// Convert ApiData to Dictionary type
        ///// </summary>
        ///// <param name="apiData"></param>
        //public static Dictionary<string, string> ConvertDataEntityToDictionary<T>(T apiData)
        //    where T : IApiData
        //{
        //    Dictionary<string, string> dic = new Dictionary<string, string>();
        //    var props = typeof(T).GetProperties(BindingFlags.Public);
        //    foreach (var propertyInfo in props)
        //    {
        //        dic[propertyInfo.Name] = (propertyInfo.GetValue(apiData) ?? "").ToString();
        //    }
        //    return dic;
        //}
    }
}
