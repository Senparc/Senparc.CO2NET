/*----------------------------------------------------------------
    Copyright (C) 2018 Senparc

    文件名：DateTime2Unix.cs
    文件功能描述：Senparc.Common.SDK 时间与时间戳互转


    创建标识：MartyZane - 20181030

----------------------------------------------------------------*/

using System;

namespace Senparc.Common.SDK
{
    /// <summary>
    /// 时间与时间戳互转
    /// </summary>
    public class DateTime2Unix
    {
        /// <summary>
        /// DateTime转换为JavaScript时间戳
        /// </summary>
        /// <returns></returns>
        public static long ConvertToJavaScriptUnix(global::System.DateTime? time = null)
        {
            if (time == null)
            {
                time = global::System.DateTime.Now;
            }
            var startTime = TimeZone.CurrentTimeZone.ToLocalTime(new global::System.DateTime(1970, 1, 1)); // 当地时区
            return (long)(time.Value - startTime).TotalMilliseconds; // 相差毫秒数
        }

        /// <summary>
        /// JavaScript时间戳转换为DateTime
        /// </summary>
        /// <param name="jsTimeStamp">JavaScript时间戳</param>
        /// <returns></returns>
        public static global::System.DateTime JavaScriptUnixConvertToDateTime(long jsTimeStamp)
        {
            var startTime = TimeZone.CurrentTimeZone.ToLocalTime(new global::System.DateTime(1970, 1, 1)); // 当地时区
            return startTime.AddMilliseconds(jsTimeStamp);
        }

        /// <summary>
        /// DateTime转换为时间戳
        /// </summary>
        /// <returns></returns>
        public static long ConvertToUnix(global::System.DateTime? time = null)
        {
            if (time == null)
            {
                time = global::System.DateTime.Now;
            }
            var startTime = TimeZone.CurrentTimeZone.ToLocalTime(new global::System.DateTime(1970, 1, 1)); // 当地时区
            return (long)(time.Value - startTime).TotalSeconds; // 相差秒数
        }

        /// <summary>
        /// 时间戳转换为DateTime
        /// </summary>
        /// <param name="unixTimeStamp">时间戳</param>
        /// <returns></returns>
        public static global::System.DateTime UnixConvertToDateTime(long unixTimeStamp)
        {
            var startTime = TimeZone.CurrentTimeZone.ToLocalTime(new global::System.DateTime(1970, 1, 1)); // 当地时区
            return startTime.AddSeconds(unixTimeStamp);
        }
    }
}
