#region Apache License Version 2.0
/*----------------------------------------------------------------

Copyright 2019 Suzhou Senparc Network Technology Co.,Ltd.

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
    Copyright (C) 2020 Senparc
    
    文件名：ObjectExtensions.cs
    文件功能描述：对象扩展类
    
    
    创建标识：Senparc - 20180602
    
    修改标识：Senparc - 20180901
    修改描述：v0.2.10 ObjectExtensions.ToJson() 方法提供 indented 方法，支持缩进格式

    修改标识：Senparc - 20160722
    修改描述：v4.11.5 修复WeixinJsonConventer.Serialize中的错误。感谢 @jiehanlin
    
    修改标识：Senparc - 20180526
    修改描述：v4.22.0-rc1 将 JsonSetting 继承 JsonSerializerSettings，使用 Newtonsoft.Json 进行序列化
    

    ----  CO2NET   ----
    ----  split from Senparc.Weixin/Helpers/Conventers/WeixinJsonConventer.cs.cs  ----

    修改标识：Senparc - 20180602
    修改描述：v0.1.0 1、移植 JsonSetting
                     2、重命名 WeixinJsonContractResolver 为 JsonContractResolver
                     3、重命名 WeiXinJsonSetting 为 JsonSettingWrap

    修改标识：Senparc - 20180721
    修改描述：v0.2.1 优化序列化特性识别

    修改标识：Senparc - 20190108
    修改描述：v0.5.1 ToJson() 方法添加 jsonSerializerSettings 参数

----------------------------------------------------------------*/

using Newtonsoft.Json;

namespace Senparc.CO2NET.Extensions
{
    /// <summary>
    /// 扩展方法
    /// </summary>
    public static class ObjectExtensions
    {
        /// <summary>
        /// 把数据转换为Json格式（使用Newtonsoft.Json.dll）
        /// </summary>
        /// <param name="data">数据</param>
        /// <param name="indented">是否使用缩进格式</param>
        /// <param name="jsonSerializerSettings">序列化设置（默认为null）</param>
        /// <returns></returns>
        public static string ToJson(this object data, bool indented = false, JsonSerializerSettings jsonSerializerSettings = null)
        {
            var formatting = indented ? Newtonsoft.Json.Formatting.Indented : Newtonsoft.Json.Formatting.None;
            return Newtonsoft.Json.JsonConvert.SerializeObject(data, formatting, jsonSerializerSettings);
        }

        /// <summary>
        /// string.IsNullOrWhiteSpace()的扩展方法
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool IsNullOrWhiteSpace(this string str)
        {
#if NET35
            return string.IsNullOrEmpty(str.Trim());
#else
            return string.IsNullOrWhiteSpace(str);
#endif
        }

        /// <summary>
        /// string.IsNullOrEmpty()的扩展方法
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool IsNullOrEmpty(this string str)
        {
            return string.IsNullOrEmpty(str);
        }

        /// <summary>
        /// string.Format()的扩展方法
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
