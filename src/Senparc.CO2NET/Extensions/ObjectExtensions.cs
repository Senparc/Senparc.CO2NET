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

----------------------------------------------------------------*/

using Newtonsoft.Json;

namespace Senparc.CO2NET.Extensions
{
    /// <summary>
    /// Extension method
    /// </summary>
    public static class ObjectExtensions
    {
        /// <summary>
        /// Convert data to Json format (using Newtonsoft.Json.dll)
        /// </summary>
        /// <param name="data">Data</param>
        /// <param name="indented">Whether to use indented format</param>
        /// <param name="jsonSerializerSettings">Serialization settings (default is null)</param>
        /// <returns></returns>
        public static string ToJson(this object data, bool indented = false, JsonSerializerSettings jsonSerializerSettings = null)
        {
            var formatting = indented ? Newtonsoft.Json.Formatting.Indented : Newtonsoft.Json.Formatting.None;
            return Newtonsoft.Json.JsonConvert.SerializeObject(data, formatting, jsonSerializerSettings);
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
