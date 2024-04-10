using Senparc.CO2NET.Cache.Dapr.ObjectCacheStrategy;

namespace Senparc.CO2NET.Cache.Dapr
{
    public class Register
    {
        /// <summary>
        /// 设置连接字符串（不立即启用）
        /// </summary>
        /// <param name="redisConfigurationString"></param>
        public static void SetConfigurationOption(string storeName, string httpEndPoint)
        {
            DaprStateManager.StoreName = storeName;
            DaprStateManager.HttpEndPoint = httpEndPoint;
        }

        /// <summary>
        /// 立即使用键值对方式储存的 Redis（推荐）
        /// </summary>
        public static void UseDaprStateNow()
        {
            CacheStrategyFactory.RegisterObjectCacheStrategy(() => DaprStateObjectCacheStrategy.Instance);//键值Redis
        }
    }
}
