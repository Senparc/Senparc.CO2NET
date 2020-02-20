/*----------------------------------------------------------------
    Copyright (C) 2020 Senparc

    文件名：MemcachedObjectCacheStrategy.cs
    文件功能描述：本地锁


    创建标识：Senparc - 20161025

    修改标识：Senparc - 20170205
    修改描述：v0.2.0 重构分布式锁

    修改标识：Senparc - 20170205
    修改描述：v1.3.0 core下，MemcachedObjectCacheStrategy.GetMemcachedClientConfiguration()方法添加注入参数

    --CO2NET--

    修改标识：Senparc - 20180714
    修改描述：v3.0.0 1、提供过期缓存策略
                     2、实现 MemcachedObjectCacheStrategy.GetAll() 和 Count() 方法

    修改标识：Senparc - 20180802
    修改描述：v3.1.0 Memcached 缓存服务连接信息实现从 Config.SenparcSetting 自动获取信息并注册）

    修改标识：Senparc - 20200220
    修改描述：v1.1.100 重构 SenparcDI

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

#if !NET45
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
        private static Dictionary<string, int> _serverlist;// = SiteConfig.MemcachedAddresss; TODO:全局注册配置

        /// <summary>
        /// <para>是否需要储存所有的缓存键</para>
        /// <para>工作原理：由于 Enyim.Caching 不支持遍历缓存键，因此当前类扩展了对所有缓存键同步储存的功能，开启后，将可以使用 GetAll() 和 Count() 方法。</para>
        /// <para>注意：1、由于储存和同步过程会产生性能消耗，在极低延时的需求下请谨慎使用！</para>
        /// <para>2、关闭期间的所有 Key 将不会同步，因此请在项目启动的第一时间决定是否启用，以免出现只记录部分 Key 的情况！</para>
        /// </summary>
        public static bool StoreKey { get; set; }

        /// 注册列表
        /// </summary>
        /// <param name="serverlist">Key：服务器地址（通常为IP），Value：端口</param>
        public static void RegisterServerList(Dictionary<string, int> serverlist)
        {
            _serverlist = serverlist;
        }

        /// <summary>
        /// 注册列表
        /// </summary>
        /// <param name="configurationString">连接字符串</param>
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

#if !NET45
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
            //自动注册连接字符串信息
            if ((_serverlist == null || _serverlist.Count == 0) &&
                !string.IsNullOrEmpty(Config.SenparcSetting.Cache_Memcached_Configuration)
                && Config.SenparcSetting.Cache_Memcached_Configuration != "Memcached配置")
            {
                RegisterServerList(Config.SenparcSetting.Cache_Memcached_Configuration);
            }

            StoreKey = false;


            // //初始化memcache服务器池
            //SockIOPool pool = SockIOPool.GetInstance();
            ////设置Memcache池连接点服务器端。
            //pool.SetServers(serverlist);
            ////其他参数根据需要进行配置

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


            //#if NET45 || NET461
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
            //                    throw new Exception("MemcachedStrategy失效，没有计入缓存！");
            //                }
            //                cache.Remove(testKey);
            //                DateTime dt2 = SystemTime.Now;

            //                SenparcTrace.Log(string.Format("MemcachedStrategy正常启用，启动及测试耗时：{0}ms", (dt2 - dt1).TotalMilliseconds));
            //            }
            //            catch (Exception ex)
            //            {
            //                //TODO:记录是同日志
            //                SenparcTrace.Log(string.Format("MemcachedStrategy静态构造函数异常：{0}", ex.Message));
            //            }

            #endregion
        }


        /// <summary>
        /// LocalCacheStrategy的构造函数
        /// </summary>
        MemcachedObjectCacheStrategy(/*ILoggerFactory loggerFactory, IOptions<MemcachedClientOptions> optionsAccessor*/)
        {
            _config = GetMemcachedClientConfiguration();
#if NET45 //|| NET461
            Cache = new MemcachedClient(_config);
#else
            var serviceProvider = SenparcDI.GlobalServiceCollection.BuildServiceProvider();
            ILoggerFactory loggerFactory = serviceProvider.GetService<ILoggerFactory>();
            Cache = new MemcachedClient(loggerFactory, _config);
#endif
        }

        //静态LocalCacheStrategy
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
            internal static readonly MemcachedObjectCacheStrategy instance = new MemcachedObjectCacheStrategy();
        }

        #endregion

        #region 配置

#if NET45
        private static MemcachedClientConfiguration GetMemcachedClientConfiguration()
#else
        private static MemcachedClientConfiguration GetMemcachedClientConfiguration(/*ILoggerFactory loggerFactory, IOptions<MemcachedClientOptions> optionsAccessor*/)
#endif
        {
            //每次都要新建

#if NET45
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
        /// 获取储存Keys信息的缓存键
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

        [Obsolete("此方法已过期，请使用 Set(TKey key, TValue value) 方法")]
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

            //TODO：加了绝对过期时间就会立即失效（再次获取后为null），memcache低版本的bug

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


            //由于 Enyim.Caching 不支持遍历Keys，所以需要单独储存
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
                //移除key
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
            var json = Cache.Get<string>(cacheKey);
            var obj = json.DeserializeFromCache();
            return obj;
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
                //获取所有Key
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
#if !NET35 && !NET40

#if NET45

        //当前使用的 Memcached 插件在 .NET 4.5 下未提供异步方法

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

            //TODO：加了绝对过期时间就会立即失效（再次获取后为null），memcache低版本的bug

            var newKey = StoreKey ? await CheckExistedAsync(cacheKey, true).ConfigureAwait(false) == false : false;

            var json = value.SerializeToCache();
            if (expiry.HasValue)
            {
                await Cache.StoreAsync(StoreMode.Set, cacheKey, json, expiry.Value).ConfigureAwait(false);
            }
            else
            {
                await Cache.StoreAsync(StoreMode.Set, cacheKey, json, TimeSpan.FromDays(999999)/*不过期*/).ConfigureAwait(false);
            }


            //由于 Enyim.Caching 不支持遍历Keys，所以需要单独储存
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
                await Cache.StoreAsync(StoreMode.Set, keyStoreFinalKey, keys.SerializeToCache(), TimeSpan.FromDays(999999)/*不过期*/).ConfigureAwait(false);
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
                //移除key
                var keyStoreFinalKey = MemcachedObjectCacheStrategy.GetKeyStoreKey(this);
                if (await CheckExistedAsync(keyStoreFinalKey, true).ConfigureAwait(false))
                {
                    var keys = await GetAsync<List<string>>(keyStoreFinalKey, true).ConfigureAwait(false);
                    keys.Remove(cacheKey);
                    await Cache.StoreAsync(StoreMode.Set, keyStoreFinalKey, keys.SerializeToCache(), TimeSpan.FromDays(999999)/*不过期*/).ConfigureAwait(false);
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
            var json = await Cache.GetAsync<string>(cacheKey).ConfigureAwait(false);
            var obj = json?.Value.DeserializeFromCache();
            return obj;
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
                //获取所有Key
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
