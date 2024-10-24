using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServiceStack.Redis;
using ServiceStack.Redis.Generic;

namespace Senparc.Weixin.Cache.Redis
{
    public class RedisManager
    {
        /// <summary>
        /// Redis configuration file information
        /// </summary>
        private static RedisConfigInfo redisConfigInfo = RedisConfigInfo.GetConfig();

        private static PooledRedisClientManager prcm;

        /// <summary>
        /// Static constructor, initializes the connection pool management object
        /// </summary>
        static RedisManager()
        {
            CreateManager();
        }


        /// <summary>
        /// Creates the connection pool management object
        /// </summary>
        private static void CreateManager()
        {
            string[] writeServerList = SplitString(redisConfigInfo.WriteServerList, ",");
            string[] readServerList = SplitString(redisConfigInfo.ReadServerList, ",");

            prcm = new PooledRedisClientManager(readServerList, writeServerList,
                             new RedisClientManagerConfig
                             {
                                 MaxWritePoolSize = redisConfigInfo.MaxWritePoolSize,
                                 MaxReadPoolSize = redisConfigInfo.MaxReadPoolSize,
                                 AutoStart = redisConfigInfo.AutoStart,
                             });
        }

        private static string[] SplitString(string strSource, string split)
        {
            return strSource.Split(split.ToArray());
        }

        /// <summary>
        /// Client cache operation object
        /// </summary>
        public static IRedisClient GetClient()
        {
            if (prcm == null)
                CreateManager();

            return prcm.GetClient();
        }

        public static IRedisTypedClient<T> GetTypedClient<T>()
        {
            if (prcm == null)
                CreateManager();

            return prcm.GetClient().As<T>();
        }
    }
}
