using Dapr.Client;
using Google.Protobuf;
using Senparc.CO2NET.Cache.Dapr;
using System.Collections.Immutable;

namespace Senparc.CO2NET.Cache.Dapr.ObjectCacheStrategy
{
    public class DaprStateObjectCacheStrategy : BaseCacheStrategy, IBaseObjectCacheStrategy
    {

        public DaprClient Client { get; set; }

        protected DaprStateObjectCacheStrategy()
        {
            var builder = new DaprClientBuilder();
            if (DaprStateManager.HttpEndPoint != null)
            {
                builder.UseHttpEndpoint(DaprStateManager.HttpEndPoint);
            }
            Client = builder.Build();
        }

        public static IBaseObjectCacheStrategy Instance
        {
            get
            {
                return Nested.instance;//返回Nested类中的静态成员instance
            }
        }

        class Nested
        {
            static Nested()
            {
            }
            //将instance设为一个初始化的LocalCacheStrategy新实例
            internal static readonly DaprStateObjectCacheStrategy instance = new DaprStateObjectCacheStrategy();
        }

        public bool CheckExisted(string key, bool isFullKey = false)
        {
            var asyncTask = CheckExistedAsync(key, isFullKey);
            asyncTask.ConfigureAwait(false);
            return asyncTask.Result;
        }

        public async Task<bool> CheckExistedAsync(string key, bool isFullKey = false)
        {
            var cacheKey = GetFinalKey(key, isFullKey);
            var byteStringResult = await Client.GetStateAsync<ByteString?>(DaprStateManager.StoreName, cacheKey);
            return byteStringResult != null;
        }

        public object Get(string key, bool isFullKey = false)
        {
            return GetAsync(key, isFullKey).GetAwaiter().GetResult();
        }

        public async Task<object> GetAsync(string key, bool isFullKey = false)
        {
            var cacheKey = GetFinalKey(key,isFullKey);
            return await Client.GetStateAsync<object>(DaprStateManager.StoreName, cacheKey);
        }

        public T Get<T>(string key, bool isFullKey = false)
        {
            return GetAsync<T>(key, isFullKey).GetAwaiter().GetResult();
        }

        public async Task<T> GetAsync<T>(string key, bool isFullKey = false)
        {
            var cacheKey = GetFinalKey(key,isFullKey);
            return await Client.GetStateAsync<T>(DaprStateManager.StoreName, cacheKey);
        }

        [Obsolete("此方法不被支持，不要使用", true)]
        public IDictionary<string, object> GetAll()
        {
            throw new NotImplementedException();
        }

        [Obsolete("此方法不被支持，不要使用", true)]
        public Task<IDictionary<string, object>> GetAllAsync()
        {
            throw new NotImplementedException();
        }

        [Obsolete("此方法不被支持，不要使用", true)]
        public long GetCount()
        {
            throw new NotImplementedException();
        }

        [Obsolete("此方法不被支持，不要使用", true)]
        public Task<long> GetCountAsync()
        {
            throw new NotImplementedException();
        }

        [Obsolete("此方法已过期，请使用 Set(TKey key, TValue value) 方法")]
        public void InsertToCache(string key, object value, TimeSpan? expiry = null)
        {
            Set(key, value, expiry);
        }

        public void RemoveFromCache(string key, bool isFullKey = false)
        {
            RemoveFromCacheAsync(key, isFullKey).GetAwaiter();
        }

        public async Task RemoveFromCacheAsync(string key, bool isFullKey = false)
        {
            var cacheKey = GetFinalKey(key, isFullKey);
            await Client.DeleteStateAsync(DaprStateManager.StoreName, cacheKey);
        }

        public void Set(string key, object value, TimeSpan? expiry = null, bool isFullKey = false)
        {
            SetAsync(key, value, expiry, isFullKey).GetAwaiter();
        }

        public async Task SetAsync(string key, object value, TimeSpan? expiry = null, bool isFullKey = false)
        {
            var cacheKey = GetFinalKey(key, isFullKey);

            if (expiry == null)
            {
                await Client.SaveStateAsync(DaprStateManager.StoreName, cacheKey, value);
                return;
            }

            string ttlInSeconds = ((int)expiry.Value.TotalSeconds).ToString();

            var metadata = new Dictionary<string, string>
            {
                { "ttlInSeconds", ttlInSeconds }
            };

            IReadOnlyDictionary<string, string> readOnlyMetaData = metadata.ToImmutableDictionary();

            await Client.SaveStateAsync(DaprStateManager.StoreName, cacheKey, value, null, readOnlyMetaData);
            return;
        }

        public void Update(string key, object value, TimeSpan? expiry = null, bool isFullKey = false)
        {
            Set(key, value, expiry, isFullKey);
        }

        public async Task UpdateAsync(string key, object value, TimeSpan? expiry = null, bool isFullKey = false)
        {
            await SetAsync(key, value, expiry, isFullKey);
        }

        public override ICacheLock BeginCacheLock(string resourceName, string key, int retryCount = 0, TimeSpan retryDelay = default)
        {
            return BeginCacheLockAsync(resourceName, key, retryCount, retryDelay).GetAwaiter().GetResult();
        }

        public override async Task<ICacheLock> BeginCacheLockAsync(string resourceName, string key, int retryCount = 0, TimeSpan retryDelay = default)
        {
            var cacheLock = new DaprCacheLock(this, resourceName, key, retryCount, retryDelay);
            var ttl = (int)cacheLock.GetTotalTtl(retryCount, retryDelay);

            if (ttl <= 0)
            {
                throw new ArgumentException("retryCount乘以retryDelay所得时间不能小于1秒");
            }

            await Client.Lock(DaprStateManager.StoreName, resourceName, key, (int)cacheLock.GetTotalTtl(retryCount, retryDelay));
            return cacheLock;
        }

        public async Task CacheUnlockAsync(string resourceName, string key)
        {
            await Client.Unlock(DaprStateManager.StoreName, resourceName, key);
        }
    }
}
