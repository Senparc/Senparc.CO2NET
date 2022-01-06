/*----------------------------------------------------------------
    Copyright(C) 2022 Senparc

    文件名：Config.cs
    文件功能描述：APM 配置


    创建标识：Senparc - 201801113

    修改标识：Senparc - 20220106
    修改描述：v1.1 默认停用 APM（EnableAPM = false）

----------------------------------------------------------------*/

using System;

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
        public static bool EnableAPM = false;
    }
}
