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
        /// APM 信息自动过期时间（默认为20分钟）
        /// </summary>
        public static TimeSpan DataExpire = TimeSpan.FromMinutes(20);//20分钟

        /// <summary>
        /// 启用 APM
        /// </summary>
        public static bool EnableAPM = true;
    }
}
