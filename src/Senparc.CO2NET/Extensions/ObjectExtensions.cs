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
    Copyright (C) 2026 Senparc
    
    FileName：ObjectExtensions.cs
    File Function Description：Object extension class
    
    
    Creation Identifier：Senparc - 20180602
    
    Modification Identifier：Senparc - 20180901
    Modification Description：v0.2.10 ObjectExtensions.ToJson() method provides indented method, supports indented format

    Modification Identifier：Senparc - 20160722
    Modification Description：v4.11.5 Fixed error in WeixinJsonConventer.Serialize. Thanks to @jiehanlin
    
    Modification Identifier：Senparc - 20180526
    Modification Description：v4.22.0-rc1 JsonSetting inherits JsonSerializerSettings, uses Newtonsoft.Json for serialization
    

    ----  CO2NET   ----
    ----  split from Senparc.Weixin/Helpers/Conventers/WeixinJsonConventer.cs.cs  ----

    Modification Identifier：Senparc - 20180602
    Modification Description：v0.1.0 1. Ported JsonSetting
                     2. Renamed WeixinJsonContractResolver to JsonContractResolver
                     3. Renamed WeiXinJsonSetting to JsonSettingWrap

    Modification Identifier：Senparc - 20180721
    Modification Description：v0.2.1 Optimized serialization feature recognition

    Modification Identifier：Senparc - 20190108
    Modification Description：v0.5.1 Added jsonSerializerSettings parameter to ToJson() method

    Modification Identifier: Senparc - 20260721
    Modification Description: v4.0.0 Migrated ToJson to System.Text.Json and added JsonTypeInfo Native AOT overloads

    Modification Identifier: Senparc - 20260722
    Modification Description: v4.1.0 Added Native AOT diagnostics for reflection-based ToJson paths while preserving existing call patterns

----------------------------------------------------------------*/

using Senparc.CO2NET.Helpers.Serializers;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
#if NET8_0_OR_GREATER
using System.Diagnostics.CodeAnalysis;
#endif

namespace Senparc.CO2NET.Extensions
{
    /// <summary>
    /// Extension method
    /// </summary>
    public static class ObjectExtensions
    {
        /// <summary>
        /// Convert data to JSON using System.Text.Json.
        /// </summary>
        /// <param name="data">Data</param>
        /// <param name="indented">Whether to use indented format</param>
        /// <param name="jsonSerializerSettings">Serialization settings. Supports JsonSerializerOptions and the legacy Newtonsoft settings object.</param>
        /// <returns></returns>
#if NET8_0_OR_GREATER
        [RequiresDynamicCode("Runtime JSON metadata may require dynamic code. Use the JsonTypeInfo overload for Native AOT.")]
        [RequiresUnreferencedCode("Runtime JSON metadata may be removed by trimming. Use the JsonTypeInfo overload for Native AOT.")]
#endif
        public static string ToJson(this object data, bool indented = false, object jsonSerializerSettings = null)
        {
            return SystemTextJsonSerializer.Serialize(data, indented, jsonSerializerSettings);
        }

        /// <summary>
        /// Convert data to JSON using explicit System.Text.Json options.
        /// </summary>
#if NET8_0_OR_GREATER
        [RequiresDynamicCode("Runtime JSON metadata may require dynamic code. Use the JsonTypeInfo overload for Native AOT.")]
        [RequiresUnreferencedCode("Runtime JSON metadata may be removed by trimming. Use the JsonTypeInfo overload for Native AOT.")]
#endif
        public static string ToJson(this object data, JsonSerializerOptions jsonSerializerOptions, bool indented = false)
        {
            return SystemTextJsonSerializer.Serialize(data, indented, jsonSerializerOptions);
        }

        /// <summary>
        /// Convert data to JSON using source-generated metadata. This overload is Native AOT safe.
        /// </summary>
        public static string ToJson<T>(this T data, JsonTypeInfo<T> jsonTypeInfo)
        {
            return SystemTextJsonSerializer.Serialize(data, jsonTypeInfo);
        }

        /// <summary>
        /// Convert data to JSON using non-generic source-generated metadata. This overload is Native AOT safe.
        /// </summary>
        public static string ToJson(this object data, JsonTypeInfo jsonTypeInfo)
        {
            return SystemTextJsonSerializer.Serialize(data, jsonTypeInfo);
        }

        /// <summary>
        /// Extension method for string.IsNullOrWhiteSpace()
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool IsNullOrWhiteSpace(this string str)
        {
            return string.IsNullOrWhiteSpace(str);
        }

        /// <summary>
        /// Extension method for string.IsNullOrEmpty()
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool IsNullOrEmpty(this string str)
        {
            return string.IsNullOrEmpty(str);
        }

        /// <summary>
        /// Extension method for string.Format()
        /// </summary>
        /// <param name="format"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static string FormatWith(this string format, params object[] args)
        {
            return string.Format(format, args);
        }
    }
}
