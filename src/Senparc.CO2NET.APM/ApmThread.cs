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
        /// 打开 CPU 状态监控
        /// </summary>
        public bool OpenCpuWatch { get; set; } = false;
        /// <summary>
        /// 打开内存状态监控
        /// </summary>
        public bool OpenMemoryWatch { get; set; } = false;


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
