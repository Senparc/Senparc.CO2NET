#region Apache License Version 2.0
/*----------------------------------------------------------------

Copyright 2024 Suzhou Senparc Network Technology Co.,Ltd.

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

    FileName：RedisManager.cs
    File Function Description：Redis connection and database management interface.

    Creation Identifier：Senparc - 20160309

    Modification Identifier：Senparc - 20150319
    Modification Description：Renamed file Manager.cs to RedisManager.cs (class name unchanged);
              Used new singleton approach.

----------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StackExchange.Redis;

namespace Senparc.CO2NET.Cache.Redis
{
    /// <summary>
    /// Redis connection management
    /// </summary>
    public class RedisManager
    {
        #region ConnectionMultiplexer 单例

        /// <summary>
        /// _redis(ConnectionMultiplexer) singleton
        /// </summary>
        internal static ConnectionMultiplexer _redis
        {
            get
            {
                return NestedRedis.instance;//Returns the static member instance in the Nested class
            }
        }

        class NestedRedis
        {
            static NestedRedis()
            {
            }
            //Sets instance to a new initialized ConnectionMultiplexer instance
            internal static readonly ConnectionMultiplexer instance = GetManager();
        }

        #endregion

        /// <summary>
        /// Connection string settings
        /// </summary>
        public static string ConfigurationOption { get; set; }


        /// <summary>
        /// ConnectionMultiplexer
        /// </summary>
        public static ConnectionMultiplexer Manager
        {
            get { return _redis; }
        }

        /// <summary>
        /// Default connection string
        /// </summary>
        /// <returns></returns>
        private static string GetDefaultConnectionString()
        {
            return "localhost";
        }

        private static ConnectionMultiplexer GetManager(string connectionString = null)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                if (ConfigurationOption == null)
                {
                    connectionString = GetDefaultConnectionString();
                }
                else
                {
                    return ConnectionMultiplexer.Connect(ConfigurationOptions.Parse(ConfigurationOption));
                }
            }

            //            var redisConfigInfo = RedisConfigInfo.GetConfig();
            //            #region options settings description

            //            /*
            //abortConnect ： When true, no connection will be created if no servers are available
            //allowAdmin ： When true, allows the use of certain dangerous commands
            //channelPrefix：Prefix for all pub/sub channels
            //connectRetry ：Number of times to retry connecting
            //connectTimeout：Timeout duration
            //configChannel： Broadcast channel name for communicating configuration changes
            //defaultDatabase ： Default 0 to -1
            //keepAlive ： Keep active connection for x seconds
            //name:ClientName
            //password:password
            //proxy:Proxy such as twemproxy
            //resolveDns : Specify DNS resolution
            //serviceName ： Not currently implemented (intended for use with sentinel)
            //ssl={bool} ： Use SSL encryption
            //sslHost={string}	： Force server to use specific SSL identity
            //syncTimeout={int} ： Asynchronous timeout
            //tiebreaker={string}：Key to use for selecting a server in an ambiguous master scenario
            //version={string} ： Redis version level (useful when the server does not make this available)
            //writeBuffer={int} ： Output buffer size
            //    */

            //            #endregion
            //            var options = new ConfigurationOptions()
            //            {
            //                ServiceName = redisConfigInfo.ServerList,

            //            };

            return ConnectionMultiplexer.Connect(connectionString);
        }
    }
}
