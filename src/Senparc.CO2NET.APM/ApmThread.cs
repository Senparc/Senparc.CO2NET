using System;
using System.Collections.Generic;
using System.Threading;

namespace Senparc.CO2NET.APM
{
    /// <summary>
    /// APM 线程
    /// </summary>
    public class ApmThread
    {
        private DateTime LastRecordTime = DateTime.MinValue;

        /// <summary>
        /// 确保已经到达下一分钟
        /// </summary>
        /// <param name="recordTime"></param>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        private bool IsNextMinute(DateTime lastTime, DateTime dateTime)
        {
            return lastTime.Year < dateTime.Year ||
                   lastTime.Month < dateTime.Month ||
                   lastTime.Day < dateTime.Day ||
                   lastTime.Hour < dateTime.Hour ||
                   lastTime.Minute < dateTime.Minute;
        }

        public void Run()
        {
            while (true)
            {
                if (IsNextMinute(LastRecordTime, SystemTime.Now))
                {
                    //进行多余数据的清理


                    LastRecordTime = SystemTime.Now;
                }

                Thread.Sleep(1000 * 10);//间隔1分钟以内
            }
        }
    }
}
