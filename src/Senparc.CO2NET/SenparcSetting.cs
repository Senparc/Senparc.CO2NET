/*----------------------------------------------------------------
    Copyright (C) 2024 Senparc

    File Name: SenparcSetting.cs
    File Function Description: CO2NET Global Settings

    Creation Identifier: Senparc - 20180704

    Modification Identifier: Senparc - 20180707
    Modification Description: v0.1.9 Added constructor with isDebug parameter

    Modification Identifier: Senparc - 20180707
    Modification Description: v0.1.11 Provided BuildFromWebConfig() method

----------------------------------------------------------------*/

namespace Senparc.CO2NET
{
    /// <summary>
    /// CO2NET Global Settings
    /// </summary>
    public class SenparcSetting
    {
        /// <summary>
        /// Indicates whether it is in Debug state
        /// </summary>
        public bool IsDebug { get; set; }

        /// <summary>
        /// The first-level namespace for default cache keys, default value: DefaultCache
        /// </summary>
        public string DefaultCacheNamespace { get; set; }

        /// <summary>
        /// Senparc unified proxy identifier
        /// </summary>
        public string SenparcUnionAgentKey { get; set; }


        #region 分布式缓存

        /// <summary>
        /// Redis connection string
        /// </summary>
        public string Cache_Redis_Configuration { get; set; }

        /// <summary>
        /// Memcached connection string
        /// </summary>
        public string Cache_Memcached_Configuration { get; set; }


        #endregion


        /// <summary>
        /// SenparcSetting constructor
        /// </summary>
        public SenparcSetting() : this(false)
        {

        }

        /// <summary>
        /// SenparcSetting constructor
        /// </summary>
        public SenparcSetting(bool isDebug)
        {
            IsDebug = isDebug;
        }

#if NET462
        /// <summary>
        /// Automatically generate SenparcSetting from Web.Config file
        /// </summary>
        /// <param name="isDebug">Set the global Debug state for CO2NET</param>
        /// <returns></returns>
        public static SenparcSetting BuildFromWebConfig(bool isDebug)
        {
            var senparcSetting = new SenparcSetting(isDebug);

            senparcSetting.DefaultCacheNamespace = System.Configuration.ConfigurationManager.AppSettings["DefaultCacheNamespace"];
            senparcSetting.SenparcUnionAgentKey = System.Configuration.ConfigurationManager.AppSettings["SenparcUnionAgentKey"];
            senparcSetting.Cache_Redis_Configuration = System.Configuration.ConfigurationManager.AppSettings["Cache_Redis_Configuration"];
            senparcSetting.Cache_Memcached_Configuration = System.Configuration.ConfigurationManager.AppSettings["Cache_Memcached_Configuration"];
            return senparcSetting;
        }
#endif
    }
}
