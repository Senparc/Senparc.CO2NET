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

        public void Run()
        {
            while (true)
            {
                if (DataHelper.IsLaterMinute(LastRecordTime, SystemTime.Now))
                {
                    //进行统计并清理多余数据

                    //进行数据清理




                    LastRecordTime = SystemTime.Now;
                }

                Thread.Sleep(1000 * 10);//间隔1分钟以内
            }
        }
    }
}
