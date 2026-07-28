using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Senparc.CO2NET.Cache;
using Senparc.CO2NET.Cache.Memcached;
using Senparc.CO2NET.RegisterServices;
using Senparc.CO2NET.AspNet;

namespace Senparc.CO2NET.Sample.netcore3
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();

            services.AddMemoryCache();// Required when using local cache
            services.Add(ServiceDescriptor.Singleton(typeof(ILogger<>), typeof(Logger<>)));// Required when using Memcached or Logger

            // Senparc.CO2NET global registration (required)
            services.AddSenparcGlobalServices(Configuration);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IOptions<SenparcSetting> senparcSetting)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });


            // Start CO2NET global registration (required)
            app.UseSenparcGlobal(env, senparcSetting.Value, register =>
                {
                    #region CO2NET global configuration

                    #region Global cache configuration (as needed)

                    // When one distributed cache serves multiple sites (app pools), use a namespace to isolate them (optional)
                    register.ChangeDefaultCacheNamespace("CO2NETCache.netcore-3.1");

                    #region Configure and use Redis

                    // Configure global Redis cache (optional, independent)
                    var redisConfigurationStr = senparcSetting.Value.Cache_Redis_Configuration;
                    var useRedis = !string.IsNullOrEmpty(redisConfigurationStr) && redisConfigurationStr != "Redis配置";
                    if (useRedis)// For convenience across environments this is conditional; in production the if can usually be ignored
                    {
                        /* Notes:
                         * 1. Redis connection string is read from Config.SenparcSetting.Cache_Redis_Configuration automatically; skip SetConfigurationOption if unchanged
                        /* 2. To override manually, use SetConfigurationOption below (config only, does not enable immediately)
                         */
                        Senparc.CO2NET.Cache.CsRedis.Register.SetConfigurationOption(redisConfigurationStr);

                        // Immediately switch global cache to Redis
                        Senparc.CO2NET.Cache.CsRedis.Register.UseKeyValueRedisNow();// Key-value cache strategy (recommended)
                        //Senparc.CO2NET.Cache.Redis.Register.UseHashRedisNow();// HashSet storage cache strategy

                        // Or register a custom cache strategy explicitly
                        //CacheStrategyFactory.RegisterObjectCacheStrategy(() => RedisObjectCacheStrategy.Instance);// Key-value
                        //CacheStrategyFactory.RegisterObjectCacheStrategy(() => RedisHashSetObjectCacheStrategy.Instance);//HashSet
                    }
                    // If Redis is not enabled here, in-memory cache remains the default 

                    #endregion

                    #region Configure and use Memcached

                    // Configure Memcached cache (optional, independent)
                    var memcachedConfigurationStr = senparcSetting.Value.Cache_Memcached_Configuration;
                    var useMemcached = !string.IsNullOrEmpty(memcachedConfigurationStr) && memcachedConfigurationStr != "Memcached配置";

                    if (useMemcached) // For convenience across environments this is conditional; in production the if can usually be ignored
                    {
                        app.UseEnyimMemcached();

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

                    #endregion
                },

            #region Scan custom extension cache

                // Auto-scan custom extension cache (choose one)
                autoScanExtensionCacheStrategies: true // Default true; can be omitted
                                                       // Specify custom extension cache (choose one)
                                                       //autoScanExtensionCacheStrategies: false, extensionCacheStrategiesFunc: () => GetExCacheStrategies(senparcSetting.Value)

            #endregion
            );
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

            // Note: the two if blocks below are demos for adding custom extension cache strategies,

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
