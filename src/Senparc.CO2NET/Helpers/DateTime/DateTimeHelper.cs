#region Apache License Version 2.0
/*----------------------------------------------------------------

Copyright 2023 Suzhou Senparc Network Technology Co.,Ltd.

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
    Copyright (C) 2024 Senparc
    
    FileName：DateTimeHelper.cs
    File Function Description：Time processing
    
    
    Creation Identifier：Senparc - 20150211
    
    Modification Identifier：Senparc - 20150303
    Modification Description：Organize interface


    ----  CO2NET   ----
    ----  split from Senparc.Weixin/Helpers/DateTimeHelper.cs  ----

    Modification Identifier：Senparc - 20180601
    Modification Description：v0.1.0 Port DateTimeHelper
 
    Modification Identifier：Senparc - 20180802
    Modification Description：v0.2.6 Add GetUnixDateTime() method, mark GetWeixinDateTime() method as obsolete
    
    Modification Identifier：Senparc - 20181226
    Modification Description：1. v0.4.3 Change DateTime to DateTimeOffset
              2. Add GetUnixDateTime() overload method supporting DateTimeOffset type parameter

    Modification Identifier：Senparc - 20181227
    Modification Description：Add GetDateTimeOffsetFromXml() overload method

    Modification Identifier：Senparc - 20230326
    Modification Description：v2.0.5 Add WaitingFor() method

    Modification Identifier：Senparc - 20240728
    Modification Description：v2.4.3 Optimize GetDateTimeOffsetFromXml() method, add timezoneId parameter

    Modification Identifier：Senparc - 20240824
    Modification Description：v2.5.1 Restore GetDateTimeOffsetFromXml() method #297 Thanks to @zhaoyangguang

----------------------------------------------------------------*/


using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Senparc.CO2NET.Helpers
{
    /// <summary>
    /// WeChat date processing helper class
    /// </summary>
    public class DateTimeHelper
    {
        /// <summary>
        /// Unix start time
        /// </summary>
        public readonly static DateTimeOffset BaseTime = new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero);

        /// <summary>
        /// Convert WeChat DateTime to C# DateTime
        /// </summary>
        /// <param name="dateTimeFromXml">WeChat DateTime</param>
        /// <returns></returns>
        public static DateTime GetDateTimeFromXml(long dateTimeFromXml)
        {
            return GetDateTimeOffsetFromXml(dateTimeFromXml).LocalDateTime;
        }

        /// <summary>
        /// Convert WeChat DateTime to C# DateTime
        /// </summary>
        /// <param name="dateTimeFromXml">WeChat DateTime</param>
        /// <returns></returns>
        public static DateTime GetDateTimeFromXml(string dateTimeFromXml)
        {
            return GetDateTimeFromXml(long.Parse(dateTimeFromXml));
        }

        /// <summary>
        /// Convert WeChat DateTimeOffset to C# DateTime
        /// </summary>
        /// <param name="dateTimeFromXml">WeChat DateTime</param>
        /// <returns></returns>
        public static DateTimeOffset GetDateTimeOffsetFromXml(long dateTimeFromXml)
        {
            //DateTimeOffset utcDateTime = BaseTime.AddSeconds(dateTimeFromXml);
            DateTimeOffset utcDateTime = DateTimeOffset.FromUnixTimeSeconds(dateTimeFromXml);
            return utcDateTime;
        }


        /// <summary>
        /// Convert WeChat DateTimeOffset to C# DateTime
        /// </summary>
        /// <param name="dateTimeFromXml">WeChat DateTime</param>
        /// <returns></returns>
        public static DateTimeOffset GetDateTimeOffsetFromXml(string dateTimeFromXml)
        {
            return GetDateTimeFromXml(long.Parse(dateTimeFromXml));
        }

        /// <summary>
        /// Get WeChat DateTime (UNIX timestamp)
        /// </summary>
        /// <param name="dateTime">Time</param>
        /// <returns></returns>
        [Obsolete("请使用 GetUnixDateTime(dateTime) 方法")]
        public static long GetWeixinDateTime(DateTime dateTime)
        {
            return GetUnixDateTime(dateTime);
        }

        /// <summary>
        /// Get Unix timestamp
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static long GetUnixDateTime(DateTimeOffset dateTime)
        {
            return (long)(dateTime - BaseTime).TotalSeconds;
        }

        /// <summary>
        /// Get Unix timestamp
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static long GetUnixDateTime(DateTime dateTime)
        {
            return (long)(dateTime.ToUniversalTime() - BaseTime).TotalSeconds;
        }

        /// <summary>
        /// Auto wait
        /// </summary>
        /// <param name="waitingTime">Total waiting time</param>
        /// <param name="waitingInterval">Interval between each wait</param>
        /// <param name="work">Method to execute before each wait (can be null)</param>
        /// <returns></returns>
        public static async Task WaitingFor(TimeSpan waitingTime, TimeSpan waitingInterval, Action work = null)
        {
            var startTime = SystemTime.Now;
            while (true)
            {
                work?.Invoke();

                var delayTime = Task.Delay(waitingInterval);

                await delayTime;

                if (SystemTime.NowDiff(startTime) >= waitingTime)
                {
                    break;
                }
            }
        }
    }
}
