using System;
using System.Collections.Generic;
using System.Threading;

namespace Senparc.CO2NET.APM
{
    /// <summary>
    /// APM thread
    /// </summary>
    public class ApmThread
    {
        private DateTimeOffset LastRecordTime = DateTime.MinValue;

        /// <summary>
        /// Enable CPU status monitoring
        /// </summary>
        public bool OpenCpuWatch { get; set; } = false;
        /// <summary>
        /// Enable memory status monitoring
        /// </summary>
        public bool OpenMemoryWatch { get; set; } = false;


        public void Run()
        {
            while (true)
            {
                if (DataHelper.IsLaterMinute(LastRecordTime, SystemTime.Now))
                {
                    //Perform statistics and clean up excess data

                    //Perform data cleanup




                    LastRecordTime = SystemTime.Now;
                }

                Thread.Sleep(1000 * 10);//Within 1 minute interval
            }
        }
    }
}
