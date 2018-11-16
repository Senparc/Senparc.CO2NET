using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Senparc.CO2NET.APM
{
    public class DataHelper
    {

        /// <summary>
        /// 确保已经到达下一分钟
        /// </summary>
        /// <param name="recordTime"></param>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static bool IsLaterMinute(DateTime lastTime, DateTime dateTime)
        {
            return lastTime.Year < dateTime.Year ||
                   lastTime.Month < dateTime.Month ||
                   lastTime.Day < dateTime.Day ||
                   lastTime.Hour < dateTime.Hour ||
                   lastTime.Minute < dateTime.Minute;
        }
    }
}
