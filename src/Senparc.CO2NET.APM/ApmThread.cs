using System;
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
                //if ((SystemTime.Now- LastRecordTime))
                {

                }


                Thread.Sleep(1000 * 10);//间隔1分钟以内
            }
        }
    }
}
