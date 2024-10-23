using Senparc.CO2NET.Cache.Dapr.ObjectCacheStrategy;
using System.Security.Cryptography.X509Certificates;

namespace Senparc.CO2NET.Cache.Dapr
{
    public class Register
    {
        /// <summary>
        /// Set connection string (not immediately enabled)
        /// </summary>
        /// <param name="redisConfigurationString"></param>
        public static void SetConfigurationOption(string httpEndPoint,string stateStoreName, string lockstoreName)
        {
            DaprStateManager.StateStoreName = stateStoreName;
            DaprStateManager.LockStoreName = lockstoreName;
            DaprStateManager.HttpEndPoint = httpEndPoint;
        }

        /// <summary>
        /// Use Redis with key-value storage immediately (recommended)
        /// </summary>
        public static void UseDaprStateNow()
        {
            CacheStrategyFactory.RegisterObjectCacheStrategy(() => DaprStateObjectCacheStrategy.Instance);// Key-value Redis
        }
    }
}
