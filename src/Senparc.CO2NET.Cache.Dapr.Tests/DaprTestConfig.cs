using Senparc.CO2NET.Cache.Dapr.ObjectCacheStrategy;

namespace Senparc.CO2NET.Cache.Dapr.Tests
{
    internal static class DaprTestConfig
    {
        internal static IBaseObjectCacheStrategy GetCacheStrategy()
        {
            DaprStateManager.StoreName = "statestore";
            DaprStateManager.HttpEndPoint = "http://localhost:3500";
            CacheStrategyFactory.RegisterObjectCacheStrategy(() => DaprStateObjectCacheStrategy.Instance);
            return CacheStrategyFactory.GetObjectCacheStrategyInstance();
        }
    }
}
