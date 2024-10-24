/*----------------------------------------------------------------
    Copyright (C) 2024 Senparc

    FileName：MemcachedObjectCacheStrategy.cs
    File Function Description：Local Lock


    Creation Identifier：Senparc - 20161025

    Modification Identifier：Senparc - 20170205
    Modification Description：v0.2.0 Refactor distributed lock

    Modification Identifier：Senparc - 20170205
    Modification Description：v1.3.0 In core, MemcachedObjectCacheStrategy.GetMemcachedClientConfiguration() method added injection parameters

    --CO2NET--

    Modification Identifier：Senparc - 20180714
    Modification Description：v3.0.0 1. Provide expired cache strategy
                     2. Implement MemcachedObjectCacheStrategy.GetAll() and Count() methods

    Modification Identifier：Senparc - 20180802
    Modification Description：v3.1.0 Memcached cache service connection information automatically obtained and registered from Config.SenparcSetting

    Modification Identifier：Senparc - 20200220
    Modification Description：v1.1.100 Refactor SenparcDI

    Modification Identifier：Senparc - 20230527
    Modification Description：v4.1.3 MemcachedObjectCacheStrategy.Get() method added pure string check

----------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Enyim.Caching;
using Enyim.Caching.Configuration;
using Enyim.Caching.Memcached;
using Senparc.CO2NET.Exceptions;
using Newtonsoft.Json.Linq;

#if !NET462
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;
#else
//using static System.Threading.Tasks.TasksExtension;
#endif

namespace Senparc.CO2NET.Cache.Memcached
{
    public class MemcachedObjectCacheStrategy : BaseCacheStrategy, IBaseObjectCacheStrategy
    {
        public MemcachedClient Cache { get; set; }
        private MemcachedClientConfiguration _config;
        private static Dictionary<string, int> _serverlist;// = SiteConfig.MemcachedAddresss; TODO: Global registration configuration

        /// <summary>
        /// <para>Whether to store all cache keys</para>
        /// <para>Working principle: Since Enyim.Caching does not support traversing cache keys, this class extends the function of synchronously storing all cache keys. Once enabled, you can use the GetAll() and Count() methods.</para>
        /// <para>Note: 1. The storage and synchronization process will consume performance, so use it with caution under extremely low latency requirements!</para>
        /// <para>2. All keys during the off period will not be synchronized, so please decide whether to enable it at the first time the project starts to avoid recording only part of the keys!</para>
        /// </summary>
        public static bool StoreKey { get; set; }

        /// Registration list
        /// </summary>
        /// <param name="serverlist">Key: Server address (usually IP), Value: Port</param>
        public static void RegisterServerList(Dictionary<string, int> serverlist)
        {
            _serverlist = serverlist;
        }

        /// <summary>
        /// Registration list
        /// </summary>
        /// <param name="configurationString">Connection string</param>
        public static void RegisterServerList(string configurationString)
        {
            if (!string.IsNullOrEmpty(configurationString))
            {
                var dic = new Dictionary<string, int>();
                var servers = configurationString.Split(';');
                foreach (var server in servers)
                {
                    try
                    {
                        var serverData = server.Split(':');
                        dic[serverData[0]] = int.Parse(serverData[1]);

                    }
                    catch (Exception ex)
                    {
                        Senparc.CO2NET.Trace.SenparcTrace.BaseExceptionLog(new CacheException(ex.Message, ex));
                    }
                }

#if !NET462
                if (dic.Count() > 0)
                {
                    SenparcDI.GlobalServiceCollection.AddSenparcMemcached(options =>
                    {
                        foreach (var item in dic)
                        {
                            options.AddServer(item.Key, item.Value);
                        }
                    });
                }
#endif


                RegisterServerList(dic);
            }
        }

        #region 单例

        static MemcachedObjectCacheStrategy()
        {
            //Automatically register connection string information
            if ((_serverlist == null || _serverlist.Count == 0) &&
                !string.IsNullOrEmpty(Config.SenparcSetting.Cache_Memcached_Configuration)
                && Config.SenparcSetting.Cache_Memcached_Configuration != "Memcached配置")
            {
                RegisterServerList(Config.SenparcSetting.Cache_Memcached_Configuration);
            }

            StoreKey = false;


            // //Initialize memcache server pool
            //SockIOPool pool = SockIOPool.GetInstance();
            ////Set Memcache pool connection point server.
            //pool.SetServers(serverlist);
            ////Other parameters can be configured as needed

            //pool.InitConnections = 3;
            //pool.MinConnections = 3;
            //pool.MaxConnections = 5;

            //pool.SocketConnectTimeout = 1000;
            //pool.SocketTimeout = 3000;

            //pool.MaintenanceSleep = 30;
            //pool.Failover = true;

            //pool.Nagle = false;
            //pool.Initialize();

            //cache = new MemcachedClient();
            //cache.EnableCompression = false;

            #region 内部为测试代码，因为调用RegisterServerList()静态方法前会先执行此静态构造函数，此时_serverlist还没有被初始化，故会出错

            //            try
            //            {
            //                //config.Authentication.Type = typeof(PlainTextAuthenticator);
            //                //config.Authentication.Parameters["userName"] = "username";
            //                //config.Authentication.Parameters["password"] = "password";
            //                //config.Authentication.Parameters["zone"] = "zone";//domain?   ——Jeffrey 2015.10.20
            //                DateTime dt1 = SystemTime.Now;
            //                var config = GetMemcachedClientConfiguration();
            //                //var cache = new MemcachedClient(config);'


            //#if NET462
            //                var cache = new MemcachedClient(config);
            //#else
            //                var cache = new MemcachedClient(null, config);
            //#endif

            //                var testKey = Guid.NewGuid().ToString();
            //                var testValue = Guid.NewGuid().ToString();
            //                cache.Store(StoreMode.Set, testKey, testValue);
            //                var storeValue = cache.Get(testKey);
            //                if (storeValue as string != testValue)
            //                {
            //                    throw new Exception("MemcachedStrategy failed, not cached!");
            //                }
            //                cache.Remove(testKey);
            //                DateTime dt2 = SystemTime.Now;

            //                SenparcTrace.Log(string.Format("MemcachedStrategy successfully enabled, startup and test time: {0}ms", (dt2 - dt1).TotalMilliseconds));
            //            }
            //            catch (Exception ex)
            //            {
            //                //TODO: Log the same
            //                SenparcTrace.Log(string.Format("MemcachedStrategy static constructor exception: {0}", ex.Message));
            //            }

            #endregion
        }


        /// <summary>
        /// Constructor of LocalCacheStrategy
        /// </summary>
        MemcachedObjectCacheStrategy(/*ILoggerFactory loggerFactory, IOptions<MemcachedClientOptions> optionsAccessor*/)
        {
            _config = GetMemcachedClientConfiguration();
#if NET462
            Cache = new MemcachedClient(_config);
#else
            var serviceProvider = SenparcDI.GlobalServiceCollection.BuildServiceProvider();
            ILoggerFactory loggerFactory = serviceProvider.GetService<ILoggerFactory>();
            Cache = new MemcachedClient(loggerFactory, _config);
#endif
        }

        //Static LocalCacheStrategy
        public static IBaseObjectCacheStrategy Instance
        {
            get
            {
                return Nested.instance;//Return the static member instance in the Nested class
            }
        }

        class Nested
        {
            static Nested()
            {
            }
            //Set instance to a new initialized LocalCacheStrategy instance
            internal static readonly MemcachedObjectCacheStrategy instance = new MemcachedObjectCacheStrategy();
        }

        #endregion

        #region 配置

#if NET462
        private static MemcachedClientConfiguration GetMemcachedClientConfiguration()
#else
        private static MemcachedClientConfiguration GetMemcachedClientConfiguration(/*ILoggerFactory loggerFactory, IOptions<MemcachedClientOptions> optionsAccessor*/)
#endif
        {
            //Create a new one each time

#if NET462
            var config = new MemcachedClientConfiguration();
            foreach (var server in _serverlist)
            {
                config.Servers.Add(new IPEndPoint(IPAddress.Parse(server.Key), server.Value));
            }
            config.Protocol = MemcachedProtocol.Binary;

#else
            var serviceProvider = SenparcDI.GlobalServiceCollection.BuildServiceProvider();
            ILoggerFactory loggerFactory = serviceProvider.GetService<ILoggerFactory>();
            IOptions<MemcachedClientOptions> optionsAccessor = serviceProvider.GetService<IOptions<MemcachedClientOptions>>();

            var config = new MemcachedClientConfiguration(loggerFactory, optionsAccessor);
#endif
            return config;
        }


        #endregion

        /// <summary>
        /// Get the cache key for storing Keys information
        /// </summary>
        /// <param name="cacheStrategy"></param>
        /// <returns></returns>
        public static string GetKeyStoreKey(BaseCacheStrategy cacheStrategy)
        {
            var keyStoreFinalKey = cacheStrategy.GetFinalKey("CO2NET_KEY_STORE");
            return keyStoreFinalKey;
        }

        #region IContainerCacheStrategy 成员

        #region 同步方法

        [Obsolete("此方法已过期，请使用 Set(TKey key, TValue value) 方法", true)]
        public void InsertToCache(string key, object value, TimeSpan? expiry = null)
        {
            Set(key, value, expiry, false);
        }


        public void Set(string key, object value, TimeSpan? expiry = null, bool isFullKey = false)
        {
            if (string.IsNullOrEmpty(key) || value == null)
            {
                return;
            }

            var cacheKey = GetFinalKey(key, isFullKey);

            //TODO: Adding an absolute expiration time will cause immediate invalidation (null upon retrieval), a bug in lower versions of memcache

            var newKey = StoreKey ? !CheckExisted(cacheKey, true) : false;

            var json = value.SerializeToCache();
            if (expiry.HasValue)
            {
                Cache.Store(StoreMode.Set, cacheKey, json, expiry.Value);
            }
            else
            {
                Cache.Store(StoreMode.Set, cacheKey, json);
            }


            //Since Enyim.Caching does not support traversing Keys, it needs to be stored separately
            if (newKey)
            {
                var keyStoreFinalKey = MemcachedObjectCacheStrategy.GetKeyStoreKey(this);
                List<string> keys;
                if (!CheckExisted(keyStoreFinalKey, true))
                {
                    keys = new List<string>();
                }
                else
                {
                    keys = Get<List<string>>(keyStoreFinalKey, true);
                }
                keys.Add(cacheKey);
                Cache.Store(StoreMode.Set, keyStoreFinalKey, keys.SerializeToCache());
            }

        }

        public virtual void RemoveFromCache(string key, bool isFullKey = false)
        {
            if (string.IsNullOrEmpty(key))
            {
                return;
            }
            var cacheKey = GetFinalKey(key, isFullKey);
            Cache.Remove(cacheKey);

            if (StoreKey)
            {
                //Remove key
                var keyStoreFinalKey = MemcachedObjectCacheStrategy.GetKeyStoreKey(this);
                if (CheckExisted(keyStoreFinalKey, true))
                {
                    var keys = Get<List<string>>(keyStoreFinalKey, true);
                    keys.Remove(cacheKey);
                    Cache.Store(StoreMode.Set, keyStoreFinalKey, keys.SerializeToCache());
                }
            }
        }

        public virtual object Get(string key, bool isFullKey = false)
        {
            if (string.IsNullOrEmpty(key))
            {
                return null;
            }

            var cacheKey = GetFinalKey(key, isFullKey);
            var value = Cache.Get<string>(cacheKey);
            if (value != null)
            {
                try
                {
                    return value.DeserializeFromCache();
                }
                catch
                {
                    return value;
                }
            }
            else
            {
                return null;
            }
        }


        public virtual T Get<T>(string key, bool isFullKey = false)
        {
            if (string.IsNullOrEmpty(key))
            {
                return default(T);
            }

            var cacheKey = GetFinalKey(key, isFullKey);
            var json = Cache.Get<string>(cacheKey);
            var obj = json.DeserializeFromCache<T>();
            return obj;
        }


        public virtual IDictionary<string, object> GetAll()
        {
            IDictionary<string, object> data = new Dictionary<string, object>();

            if (StoreKey)
            {
                //Get all Keys
                var keyStoreFinalKey = MemcachedObjectCacheStrategy.GetKeyStoreKey(this);
                if (CheckExisted(keyStoreFinalKey, true))
                {
                    var keys = Get<List<string>>(keyStoreFinalKey, true);
                    foreach (var key in keys)
                    {
                        data[key] = Get(key, true);
                    }
                }
            }

            return data;
        }

        public virtual bool CheckExisted(string key, bool isFullKey = false)
        {
            var cacheKey = GetFinalKey(key, isFullKey);
            object value;
            if (Cache.TryGet(cacheKey, out value))
            {
                return true;
            }
            return false;
        }

        public virtual long GetCount()
        {
            var keyStoreFinalKey = MemcachedObjectCacheStrategy.GetKeyStoreKey(this);
            if (StoreKey && CheckExisted(keyStoreFinalKey, true))
            {
                var keys = Get<List<string>>(keyStoreFinalKey, true);
                return keys.Count;
            }
            else
            {
                return 0;
            }
        }

        public virtual void Update(string key, object value, TimeSpan? expiry = null, bool isFullKey = false)
        {
            Set(key, value, expiry, isFullKey);
        }

        #endregion

        #region 异步方法

#if NET462

        //The current Memcached plugin does not provide asynchronous methods under .NET 4.5

        public async Task SetAsync(string key, object value, TimeSpan? expiry = null, bool isFullKey = false)
        {
            await Task.Factory.StartNew(() => Set(key, value, expiry, isFullKey)).ConfigureAwait(false);

        }

        public virtual async Task RemoveFromCacheAsync(string key, bool isFullKey = false)
        {
            await Task.Factory.StartNew(() => RemoveFromCache(key, isFullKey)).ConfigureAwait(false);
        }

        public virtual async Task<object> GetAsync(string key, bool isFullKey = false)
        {
            return await Task.Factory.StartNew(() => Get(key, isFullKey)).ConfigureAwait(false);
        }


        public virtual async Task<T> GetAsync<T>(string key, bool isFullKey = false)
        {
            return await Task.Factory.StartNew(() => Get<T>(key, isFullKey)).ConfigureAwait(false);
        }


        public virtual async Task<IDictionary<string, object>> GetAllAsync()
        {
            return await Task.Factory.StartNew(() => GetAll()).ConfigureAwait(false);
        }

        public virtual async Task<bool> CheckExistedAsync(string key, bool isFullKey = false)
        {
            return await Task.Factory.StartNew(() => CheckExisted(key, isFullKey)).ConfigureAwait(false);
        }

        public virtual async Task<long> GetCountAsync()
        {
            return await Task.Factory.StartNew(() => GetCount()).ConfigureAwait(false);
        }

        public virtual async Task UpdateAsync(string key, object value, TimeSpan? expiry = null, bool isFullKey = false)
        {
            await SetAsync(key, value, expiry, isFullKey).ConfigureAwait(false);
        }
#else

        public async Task SetAsync(string key, object value, TimeSpan? expiry = null, bool isFullKey = false)
        {
            if (string.IsNullOrEmpty(key) || value == null)
            {
                return;// TaskExtension.CompletedTask();
            }

            var cacheKey = GetFinalKey(key, isFullKey);

            //TODO: Adding an absolute expiration time will cause immediate invalidation (null upon retrieval), a bug in lower versions of memcache

            var newKey = StoreKey ? await CheckExistedAsync(cacheKey, true).ConfigureAwait(false) == false : false;

            var json = value.SerializeToCache();
            if (expiry.HasValue)
            {
                await Cache.StoreAsync(StoreMode.Set, cacheKey, json, expiry.Value).ConfigureAwait(false);
            }
            else
            {
                await Cache.StoreAsync(StoreMode.Set, cacheKey, json, TimeSpan.FromDays(999999)/*No expiration*/).ConfigureAwait(false);
            }


            //Since Enyim.Caching does not support traversing Keys, it needs to be stored separately
            if (newKey)
            {
                var keyStoreFinalKey = MemcachedObjectCacheStrategy.GetKeyStoreKey(this);
                List<string> keys;
                if (!await CheckExistedAsync(keyStoreFinalKey, true).ConfigureAwait(false))
                {
                    keys = new List<string>();
                }
                else
                {
                    keys = await GetAsync<List<string>>(keyStoreFinalKey, true).ConfigureAwait(false);
                }
                keys.Add(cacheKey);
                await Cache.StoreAsync(StoreMode.Set, keyStoreFinalKey, keys.SerializeToCache(), TimeSpan.FromDays(999999)/*No expiration*/).ConfigureAwait(false);
            }

        }

        public virtual async Task RemoveFromCacheAsync(string key, bool isFullKey = false)
        {
            if (string.IsNullOrEmpty(key))
            {
                return;
            }
            var cacheKey = GetFinalKey(key, isFullKey);
            await Cache.RemoveAsync(cacheKey).ConfigureAwait(false);

            if (StoreKey)
            {
                //Remove key
                var keyStoreFinalKey = MemcachedObjectCacheStrategy.GetKeyStoreKey(this);
                if (await CheckExistedAsync(keyStoreFinalKey, true).ConfigureAwait(false))
                {
                    var keys = await GetAsync<List<string>>(keyStoreFinalKey, true).ConfigureAwait(false);
                    keys.Remove(cacheKey);
                    await Cache.StoreAsync(StoreMode.Set, keyStoreFinalKey, keys.SerializeToCache(), TimeSpan.FromDays(999999)/*No expiration*/).ConfigureAwait(false);
                }
            }
        }

        public virtual async Task<object> GetAsync(string key, bool isFullKey = false)
        {
            if (string.IsNullOrEmpty(key))
            {
                return null;
            }

            var cacheKey = GetFinalKey(key, isFullKey);
            var value = await Cache.GetAsync<string>(cacheKey).ConfigureAwait(false);
            if (value != null)
            {
                try
                {
                    return value.Value.DeserializeFromCache();
                }
                catch
                {
                    return value.Value;
                }
            }
            else
            {
                return null;
            }

        }


        public virtual async Task<T> GetAsync<T>(string key, bool isFullKey = false)
        {
            if (string.IsNullOrEmpty(key))
            {
                return default(T);
            }

            var cacheKey = GetFinalKey(key, isFullKey);
            var json = await Cache.GetAsync<string>(cacheKey).ConfigureAwait(false);
            var obj = json == null ? default(T) : json.Value.DeserializeFromCache<T>();
            return obj;
        }


        public virtual async Task<IDictionary<string, object>> GetAllAsync()
        {
            IDictionary<string, object> data = new Dictionary<string, object>();

            if (StoreKey)
            {
                //Get all Keys
                var keyStoreFinalKey = MemcachedObjectCacheStrategy.GetKeyStoreKey(this);
                if (await CheckExistedAsync(keyStoreFinalKey, true).ConfigureAwait(false))
                {
                    var keys = await GetAsync<List<string>>(keyStoreFinalKey, true).ConfigureAwait(false);
                    foreach (var key in keys)
                    {
                        data[key] = Get(key, true);
                    }
                }
            }

            return data;
        }

        public virtual async Task<bool> CheckExistedAsync(string key, bool isFullKey = false)
        {
            return await Task.Factory.StartNew(() => CheckExisted(key, isFullKey)).ConfigureAwait(false);
        }

        public virtual async Task<long> GetCountAsync()
        {
            var keyStoreFinalKey = MemcachedObjectCacheStrategy.GetKeyStoreKey(this);
            if (StoreKey && CheckExisted(keyStoreFinalKey, true))
            {
                var keys = await GetAsync<List<string>>(keyStoreFinalKey, true).ConfigureAwait(false);
                return keys.Count;
            }
            else
            {
                return 0;
            }
        }

        public virtual async Task UpdateAsync(string key, object value, TimeSpan? expiry = null, bool isFullKey = false)
        {
            await SetAsync(key, value, expiry, isFullKey).ConfigureAwait(false);
        }

#endif
        #endregion

        #endregion


        public override ICacheLock BeginCacheLock(string resourceName, string key, int retryCount = 0, TimeSpan retryDelay = new TimeSpan())
        {
            return MemcachedCacheLock.CreateAndLock(this, resourceName, key, retryCount, retryDelay);
        }


        public override async Task<ICacheLock> BeginCacheLockAsync(string resourceName, string key, int retryCount = 0, TimeSpan retryDelay = new TimeSpan())
        {
            return await MemcachedCacheLock.CreateAndLockAsync(this, resourceName, key, retryCount, retryDelay).ConfigureAwait(false);
        }


        /// <summary>
        /// Cache.TryGet(key, out value);
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool TryGet(string key, out object value, bool isFullKey = false)
        {
            var cacheKey = GetFinalKey(key, isFullKey);
            object json;
            if (Cache.TryGet(key, out json))
            {
                value = (json as string).DeserializeFromCache();
                return true;
            }

            value = null;
            return false;
        }
    }
}
