/*----------------------------------------------------------------
    Copyright(C) 2024 Senparc

    FileName: Config.cs
    File Function Description: APM Configuration


    Creation Identifier: Senparc - 201801113

    Modification Identifier: Senparc - 20220106
    Modification Description: v1.1 APM disabled by default (EnableAPM = false)

----------------------------------------------------------------*/

using System;

namespace Senparc.CO2NET.APM
{
    /// <summary>
    /// APM Configuration
    /// </summary>
    public class Config
    {
        /// <summary>
        /// APM information auto-expiration time (default is 20 minutes)
        /// </summary>
        public static TimeSpan DataExpire = TimeSpan.FromMinutes(20);//20 minutes

        /// <summary>
        /// Enable APM
        /// </summary>
        public static bool EnableAPM = false;
    }
}
