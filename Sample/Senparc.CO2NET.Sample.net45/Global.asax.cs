using Senparc.CO2NET.Cache;
using Senparc.CO2NET.Cache.Memcached;
using Senparc.CO2NET.Cache.Redis;
using Senparc.CO2NET.RegisterServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace Senparc.CO2NET.Sample.net45
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);


            // Set global Debug state
            var isGLobalDebug = true;
            // Global settings stored in Senparc.CO2NET.Config.SenparcSetting
            var senparcSetting = SenparcSetting.BuildFromWebConfig(isGLobalDebug);
            // Or set global Debug anywhere in the app:
            //Senparc.CO2NET.Config.IsDebug = isGLobalDebug;


            // CO2NET global registration (required)
            IRegisterService register = RegisterService.Start(senparcSetting)
                                          .UseSenparcGlobal(false, () => GetExCacheStrategies(senparcSetting));

            #region Global cache configuration (as needed)

            // When one distributed cache serves multiple sites (app pools), use a namespace to isolate them (optional)
            register.ChangeDefaultCacheNamespace("CO2NETCache.net45");

            #region Configure and use Redis

            // Configure global Redis cache (optional, independent)
            var redisConfigurationStr = senparcSetting.Cache_Redis_Configuration;
            var useRedis = !string.IsNullOrEmpty(redisConfigurationStr) && redisConfigurationStr != "Redis配置";
            if (useRedis)// For convenience across environments this is conditional; in production the if can usually be ignored
            {
                /* Notes:
                 * 1. Redis connection string is read from Config.SenparcSetting.Cache_Redis_Configuration automatically; skip SetConfigurationOption if unchanged
                /* 2. To override manually, use SetConfigurationOption below (config only, does not enable immediately)
                 */
                Senparc.CO2NET.Cache.Redis.Register.SetConfigurationOption(redisConfigurationStr);

                // Immediately switch global cache to Redis
                Senparc.CO2NET.Cache.Redis.Register.UseKeyValueRedisNow();// Key-value cache strategy (recommended)
                //Senparc.CO2NET.Cache.Redis.Register.UseHashRedisNow();// HashSet storage cache strategy

                // Or register a custom cache strategy explicitly
                //CacheStrategyFactory.RegisterObjectCacheStrategy(() => RedisObjectCacheStrategy.Instance);// Key-value
                //CacheStrategyFactory.RegisterObjectCacheStrategy(() => RedisHashSetObjectCacheStrategy.Instance);// HashSet
            }
            // If Redis is not enabled here, in-memory cache remains the default

            #endregion

            #region Configure and use Memcached

            // Configure Memcached cache (optional, independent)
            var memcachedConfigurationStr = senparcSetting.Cache_Memcached_Configuration;
            var useMemcached = !string.IsNullOrEmpty(memcachedConfigurationStr) && memcachedConfigurationStr != "Memcached配置";

            if (useMemcached) // For convenience across environments this is conditional; in production the if can usually be ignored
            {
                /* Notes:
                * 1. Memcached connection string is read from Config.SenparcSetting.Cache_Memcached_Configuration automatically; skip SetConfigurationOption if unchanged
               /* 2. To override manually, use SetConfigurationOption below (config only, does not enable immediately)
                */
                Senparc.CO2NET.Cache.Memcached.Register.SetConfigurationOption(redisConfigurationStr);

                // Immediately switch global cache to Memcached
                Senparc.CO2NET.Cache.Memcached.Register.UseMemcachedNow();

                // Or register a custom cache strategy explicitly
                CacheStrategyFactory.RegisterObjectCacheStrategy(() => MemcachedObjectCacheStrategy.Instance);
            }

            #endregion


            #endregion

            #region Register trace log (optional, recommended)

            register.RegisterTraceLog(ConfigTraceLog);// Configure TraceLog

            #endregion

        }


        /// <summary>
        /// Configure global trace log
        /// </summary>
        private void ConfigTraceLog()
        {
            // When Debug is enabled, logs are written under /App_Data/SenparcTraceLog/; disable in production

            // If global IsDebug (Senparc.CO2NET.Config.IsDebug) is false, set true here; otherwise it stays true
            CO2NET.Trace.SenparcTrace.SendCustomLog("系统日志", "系统启动");// Only effective when Senparc.CO2NET.Config.IsDebug = true

            // Global custom log callback
            CO2NET.Trace.SenparcTrace.OnLogFunc = () =>
            {
                // Code to run after each log event
            };

            CO2NET.Trace.SenparcTrace.OnBaseExceptionFunc = ex =>
            {
                // Code to run after each BaseException
            };
        }

        /// <summary>
        /// Get extension cache strategies
        /// </summary>
        /// <returns></returns>
        private IList<IDomainExtensionCacheStrategy> GetExCacheStrategies(SenparcSetting senparcSetting)
        {
            var exContainerCacheStrategies = new List<IDomainExtensionCacheStrategy>();
            senparcSetting = senparcSetting ?? new SenparcSetting();

            // Note: the two if blocks below are demos for adding custom extension cache strategies

            #region Demo extension cache registration

            /*

            // Check whether Redis is available
            var redisConfiguration = senparcSetting.Cache_Redis_Configuration;
            if ((!string.IsNullOrEmpty(redisConfiguration) && redisConfiguration != "Redis configuration"))
            {
                exContainerCacheStrategies.Add(RedisContainerCacheStrategy.Instance);// Custom extension cache
            }

            // Check whether Memcached is available
            var memcachedConfiguration = senparcSetting.Cache_Memcached_Configuration;
            if ((!string.IsNullOrEmpty(memcachedConfiguration) && memcachedConfiguration != "Memcached configuration"))
            {
                exContainerCacheStrategies.Add(MemcachedContainerCacheStrategy.Instance);// TODO: throws if not configured
            }
            */

            #endregion

            // Extend with custom cache strategies

            return exContainerCacheStrategies;
        }
    }
}
