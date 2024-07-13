using Senparc.CO2NET.Cache.Dapr.ObjectCacheStrategy;

namespace Senparc.CO2NET.Cache.Dapr
{
    public class DaprCacheLock : BaseCacheLock
    {
        private readonly DaprStateObjectCacheStrategy _strategy;

        private readonly string _key;
        private readonly string _originalResourceName;

        public DaprCacheLock(IBaseCacheStrategy strategy, string resourceName, string key, int? retryCount, TimeSpan? retryDelay)
            : base(strategy, resourceName, key, retryCount, retryDelay)
        {
            _strategy = (DaprStateObjectCacheStrategy)strategy;
            _originalResourceName = resourceName;
            _key = key;
        }

        public override ICacheLock Lock()
        {
            return LockAsync().GetAwaiter().GetResult();
        }

        public override async Task<ICacheLock> LockAsync()
        {
            return await _strategy.BeginCacheLockAsync(_originalResourceName, _key, _retryCount, _retryDelay);
        }

        public override void UnLock()
        {
            _strategy.CacheUnlock(_resourceName, _key);
        }

        public async override Task UnLockAsync()
        {
            await _strategy.CacheUnlockAsync(_resourceName, _key);
        }
    }
}
