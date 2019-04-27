using System;
using System.Collections.Generic;
using System.Text;

namespace Senparc.CO2NET.APM
{
    /// <summary>
    /// APM 配置
    /// </summary>
    public class Config
    {
        /// <summary>
        /// APM 信息自动过期时间
        /// </summary>
        public static TimeSpan DataExpire = TimeSpan.FromHours(48);//2天

        /// <summary>
        /// 启用 APM
        /// </summary>
        public static bool EnableAPM = true;
    }
}
