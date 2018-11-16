using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
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

        /// <summary>
        /// 获取CPU信息
        /// </summary>
        /// <returns></returns>
        public static object GetCPUCounter()
        {
#if NET35 || NET40 || NET45
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

#if NET35 || NET40 || NET45
            //OperatingSystem os = Environment.OSVersion;
            //return os.ToString();
            return Environment.OSVersion.Platform.ToString();
#else
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return OSPlatform.Windows.ToString()
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return OSPlatform.Linux.ToString()
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
