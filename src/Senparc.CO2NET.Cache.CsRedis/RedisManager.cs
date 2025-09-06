
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Senparc.CO2NET.Cache.CsRedis
{
    public class RedisManager
    {
        public static string ConfigurationOption { get; set; }

        /// <summary>
        /// Determine whether the current project can enable Redis through the default configuration
        /// </summary>
        /// <returns></returns>
        public static bool CanUseRedis(string connectionStr = null)
        {
            connectionStr ??= RedisManager.ConfigurationOption;
            return !string.IsNullOrEmpty(connectionStr) ||
                    (!string.IsNullOrEmpty(Config.SenparcSetting.Cache_Redis_Configuration) &&
                    Config.SenparcSetting.Cache_Redis_Configuration != "Redis配置" &&
                    Config.SenparcSetting.Cache_Redis_Configuration != "#{Cache_Redis_Configuration}#");
        }
}
}
