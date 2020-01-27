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

    文件名：DataHelper.cs
    文件功能描述：数据帮助类


    创建标识：Senparc - 20180602

    修改标识：Senparc - 20181226
    修改描述：v0.4.3 修改 DateTime 为 DateTimeOffset

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
    /// 数据帮助类
    /// </summary>
    public class DataHelper
    {

        /// <summary>
        /// 确保已经到达下一分钟
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
        /// 获取CPU信息
        /// </summary>
        /// <returns></returns>
        public static object GetCPUCounter()
        {
#if NET45
            PerformanceCounter pc = new PerformanceCounter();
            pc.CategoryName = "Processor";
            pc.CounterName = "% Processor Time";
            pc.InstanceName = "_Total";
            dynamic Value_1 = pc.NextValue();
            System.Threading.Thread.Sleep(1000);
            dynamic Value_2 = pc.NextValue();
            return Value_2;
#else
            Process[] p = Process.GetProcesses();//获取进程信息
            Int64 totalMem = 0;
            string info = "";
            foreach (Process pr in p)
            {
                totalMem += pr.WorkingSet64 / 1024;
                info += pr.ProcessName + "内存：-----------" + (pr.WorkingSet64 / 1024).ToString() + "KB\r\n";//得到进程内存
            }
            return info;
#endif

        }

        /// <summary>
        /// 获取 系统名称
        /// </summary>
        /// <returns></returns>
        public static string GetOSPlatform()
        {
            //return Environment.OSVersion.Platform.ToString();

#if NET45
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
