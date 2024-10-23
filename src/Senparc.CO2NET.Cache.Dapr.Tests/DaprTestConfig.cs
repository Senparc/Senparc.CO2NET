using Senparc.CO2NET.Cache.Dapr.ObjectCacheStrategy;

namespace Senparc.CO2NET.Cache.Dapr.Tests
{
    internal static class DaprTestConfig
    {
        internal static IBaseObjectCacheStrategy GetCacheStrategy()
        {
            DaprStateManager.StateStoreName = "statestore";
            DaprStateManager.LockStoreName = "lockstore";
            DaprStateManager.HttpEndPoint = "http://localhost:3500";
            CacheStrategyFactory.RegisterObjectCacheStrategy(() => DaprStateObjectCacheStrategy.Instance);
            return CacheStrategyFactory.GetObjectCacheStrategyInstance();
        }
    }
}
