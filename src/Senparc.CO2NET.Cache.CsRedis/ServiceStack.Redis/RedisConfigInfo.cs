using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Senparc.Weixin.Cache.Redis
{
    /// <summary>
    /// Redis configuration information
    /// </summary>
    public sealed class RedisConfigInfo : ConfigurationSection
    {
        /// <summary>
        /// Get configuration information, the default configuration node name is RedisConfig
        /// </summary>
        /// <returns></returns>
        public static RedisConfigInfo GetConfig()
        {
            RedisConfigInfo section = (RedisConfigInfo)ConfigurationManager.GetSection("RedisConfig");
            return section;
        }

        /// <summary>
        /// Get configuration information
        /// </summary>
        /// <param name="sectionName">The sectionName of the configuration node</param>
        /// <returns></returns>
        public static RedisConfigInfo GetConfig(string sectionName)
        {
            RedisConfigInfo section = (RedisConfigInfo)ConfigurationManager.GetSection(sectionName);
            if (section == null)
                throw new ConfigurationErrorsException("Section " + sectionName + " is not found.");
            return section;
        }
        /// <summary>
        /// Writable Redis connection address
        /// </summary>
        [ConfigurationProperty("WriteServerList", IsRequired = false)]
        public string WriteServerList
        {
            get
            {
                return (string)base["WriteServerList"];
            }
            set
            {
                base["WriteServerList"] = value;
            }
        }


        /// <summary>
        /// Readable Redis connection address
        /// </summary>
        [ConfigurationProperty("ReadServerList", IsRequired = false)]
        public string ReadServerList
        {
            get
            {
                return (string)base["ReadServerList"];
            }
            set
            {
                base["ReadServerList"] = value;
            }
        }


        /// <summary>
        /// Maximum number of write connections
        /// </summary>
        [ConfigurationProperty("MaxWritePoolSize", IsRequired = false, DefaultValue = 5)]
        public int MaxWritePoolSize
        {
            get
            {
                int _maxWritePoolSize = (int)base["MaxWritePoolSize"];
                return _maxWritePoolSize > 0 ? _maxWritePoolSize : 5;
            }
            set
            {
                base["MaxWritePoolSize"] = value;
            }
        }


        /// <summary>
        /// Maximum number of read connections
        /// </summary>
        [ConfigurationProperty("MaxReadPoolSize", IsRequired = false, DefaultValue = 5)]
        public int MaxReadPoolSize
        {
            get
            {
                int _maxReadPoolSize = (int)base["MaxReadPoolSize"];
                return _maxReadPoolSize > 0 ? _maxReadPoolSize : 5;
            }
            set
            {
                base["MaxReadPoolSize"] = value;
            }
        }


        /// <summary>
        /// Auto restart
        /// </summary>
        [ConfigurationProperty("AutoStart", IsRequired = false, DefaultValue = true)]
        public bool AutoStart
        {
            get
            {
                return (bool)base["AutoStart"];
            }
            set
            {
                base["AutoStart"] = value;
            }
        }



        /// <summary>
        /// Local cache expiration time, unit: seconds
        /// </summary>
        [ConfigurationProperty("LocalCacheTime", IsRequired = false, DefaultValue = 36000)]
        public int LocalCacheTime
        {
            get
            {
                return (int)base["LocalCacheTime"];
            }
            set
            {
                base["LocalCacheTime"] = value;
            }
        }


        /// <summary>
        /// Whether to log, this setting is only used to troubleshoot issues during redis runtime, if redis works normally, please turn off this option
        /// </summary>
        [ConfigurationProperty("RecordeLog", IsRequired = false, DefaultValue = false)]
        public bool RecordeLog
        {
            get
            {
                return (bool)base["RecordeLog"];
            }
            set
            {
                base["RecordeLog"] = value;
            }
        }

    }
}
