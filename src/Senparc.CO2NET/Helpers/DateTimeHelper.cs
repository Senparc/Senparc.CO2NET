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
    
    文件名：DateTimeHelper.cs
    文件功能描述：时间处理
    
    
    创建标识：Senparc - 20150211
    
    修改标识：Senparc - 20150303
    修改描述：整理接口


    ----  CO2NET   ----
    ----  split from Senparc.Weixin/Helpers/DateTimeHelper.cs  ----

    修改标识：Senparc - 20180601
    修改描述：v0.1.0 移植 DateTimeHelper
 
    修改标识：Senparc - 20180802
    修改描述：v0.2.6 增加 GetUnixDateTime() 方法，标记过期 GetWeixinDateTime() 方法
    
    修改标识：Senparc - 20181226
    修改描述：1、v0.4.3 修改 DateTime 为 DateTimeOffset
              2、添加 支持 DateTimeOffset 类型参数的 GetUnixDateTime() 重写方法

    修改标识：Senparc - 20181227
    修改描述：添加 GetDateTimeOffsetFromXml() 重写方法

----------------------------------------------------------------*/


using System;

namespace Senparc.CO2NET.Helpers
{
    /// <summary>
    /// 微信日期处理帮助类
    /// </summary>
    public class DateTimeHelper
    {
        /// <summary>
        /// Unix起始时间
        /// </summary>
        public readonly static DateTimeOffset BaseTime = new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero);

        /// <summary>
        /// 转换微信DateTime时间到C#时间
        /// </summary>
        /// <param name="dateTimeFromXml">微信DateTime</param>
        /// <returns></returns>
        public static DateTime GetDateTimeFromXml(long dateTimeFromXml)
        {
            return GetDateTimeOffsetFromXml(dateTimeFromXml).LocalDateTime;
        }

        /// <summary>
        /// 转换微信DateTime时间到C#时间
        /// </summary>
        /// <param name="dateTimeFromXml">微信DateTime</param>
        /// <returns></returns>
        public static DateTime GetDateTimeFromXml(string dateTimeFromXml)
        {
            return GetDateTimeFromXml(long.Parse(dateTimeFromXml));
        }

        /// <summary>
        /// 转换微信DateTimeOffset时间到C#时间
        /// </summary>
        /// <param name="dateTimeFromXml">微信DateTime</param>
        /// <returns></returns>
        public static DateTimeOffset GetDateTimeOffsetFromXml(long dateTimeFromXml)
        {
            return BaseTime.AddSeconds(dateTimeFromXml).ToLocalTime();
        }

        /// <summary>
        /// 转换微信DateTimeOffset时间到C#时间
        /// </summary>
        /// <param name="dateTimeFromXml">微信DateTime</param>
        /// <returns></returns>
        public static DateTimeOffset GetDateTimeOffsetFromXml(string dateTimeFromXml)
        {
            return GetDateTimeFromXml(long.Parse(dateTimeFromXml));
        }

        /// <summary>
        /// 获取微信DateTime（UNIX时间戳）
        /// </summary>
        /// <param name="dateTime">时间</param>
        /// <returns></returns>
        [Obsolete("请使用 GetUnixDateTime(dateTime) 方法")]
        public static long GetWeixinDateTime(DateTime dateTime)
        {
            return GetUnixDateTime(dateTime);
        }

        /// <summary>
        /// 获取Unix时间戳
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static long GetUnixDateTime(DateTimeOffset dateTime)
        {
            return (long)(dateTime - BaseTime).TotalSeconds;
        }

        /// <summary>
        /// 获取Unix时间戳
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static long GetUnixDateTime(DateTime dateTime)
        {
            return (long)(dateTime.ToUniversalTime() - BaseTime).TotalSeconds;
        }
    }
}
