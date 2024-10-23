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

    FileName：DataHelper.cs
    File Function Description：Data Helper Class


    Creation Identifier：Senparc - 20180602

    Modification Identifier：Senparc - 20181226
    Modification Description：v0.4.3 Change DateTime to DateTimeOffset

 ----------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Senparc.CO2NET.APM
{
    /// <summary>
    /// Data Helper Class
    /// </summary>
    public class DataHelper
    {

        /// <summary>
        /// Ensure it has reached the next minute
        /// </summary>
        /// <param name="lastTime"></param>
        /// <param name="currentDateTime"></param>
        /// <returns></returns>
        public static bool IsLaterMinute(DateTimeOffset lastTime, DateTimeOffset currentDateTime)
        {
            return lastTime.Year < currentDateTime.Year ||
                   lastTime.Month < currentDateTime.Month ||
                   lastTime.Day < currentDateTime.Day ||
                   lastTime.Hour < currentDateTime.Hour ||
                   lastTime.Minute < currentDateTime.Minute;
        }

        /// <summary>
        /// Get CPU information
        /// </summary>
        /// <returns></returns>
        public static object GetCPUCounter()
        {
#if NET462
            PerformanceCounter pc = new PerformanceCounter();
            pc.CategoryName = "Processor";
            pc.CounterName = "% Processor Time";
            pc.InstanceName = "_Total";
            dynamic Value_1 = pc.NextValue();
            System.Threading.Thread.Sleep(1000);
            dynamic Value_2 = pc.NextValue();
            return Value_2;
#else
            Process[] p = Process.GetProcesses();//Get process information
            Int64 totalMem = 0;
            string info = "";
            foreach (Process pr in p)
            {
                totalMem += pr.WorkingSet64 / 1024;
                info += pr.ProcessName + "内存：-----------" + (pr.WorkingSet64 / 1024).ToString() + "KB\r\n";//Get process memory
            }
            return info;
#endif

        }

        /// <summary>
        /// Get system name
        /// </summary>
        /// <returns></returns>
        public static string GetOSPlatform()
        {
            //return Environment.OSVersion.Platform.ToString();

#if NET462
            //OperatingSystem os = Environment.OSVersion;
            //return os.ToString();
            return Environment.OSVersion.Platform.ToString();
#else
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return OSPlatform.Windows.ToString();
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return OSPlatform.Linux.ToString();
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                return OSPlatform.OSX.ToString();
            }
            else
            {
                return "Unknown";
            }
#endif
        }
    }
}
