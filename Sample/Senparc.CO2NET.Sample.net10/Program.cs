/*----------------------------------------------------------------
    Copyright (C) 2026 Senparc

    File name: Program.cs
    File description: Configure and start the CO2NET net10 sample application


    Create identity: Senparc - 20251123

    Modify identity: Senparc - 20260721
    Modify description: v1.0.1 Fix Chinese comment encoding and adapt to CO2NET 4.0.0

----------------------------------------------------------------*/

using System;
using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Senparc.CO2NET.AspNet;
using Senparc.CO2NET.Helpers;
using Senparc.CO2NET.RegisterServices;
using Senparc.CO2NET.Sample.net10.Services;
using Senparc.CO2NET.WebApi;
using Senparc.CO2NET.WebApi.WebApiEngines;
using Senparc.CO2NET;
using Microsoft.Extensions.Options;
using Senparc.CO2NET.Cache;
using Senparc.CO2NET.Cache.Memcached;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var mvpBuilder = builder.Services.AddControllersWithViews();

// Required when using local cache
builder.Services.AddMemoryCache();

#region Add global configuration (one line)

// Senparc.Weixin registration (required)
builder.Services.AddSenparcGlobalServices(builder.Configuration);

#endregion


#region WebApiEngine (optional)

// Ignore for testing; comment out the code below to see WeChat Official Account SDK APIs and comments
Senparc.CO2NET.WebApi.Register.OmitCategoryList.Add(Senparc.NeuChar.PlatformType.WeChat_OfficialAccount.ToString());

// Additional test entries
Senparc.CO2NET.WebApi.Register.AdditionalClasses.Add(typeof(AdditionalType), "Additional");
Senparc.CO2NET.WebApi.Register.AdditionalMethods.Add(typeof(AdditionalMethod).GetMethod("TestApi"), "Additional");
Senparc.CO2NET.WebApi.Register.AdditionalMethods.Add(typeof(EncryptHelper).GetMethod("GetMD5", new[] { typeof(string), typeof(string) }), "Additional");

var docXmlPath = Path.Combine(builder.Environment.ContentRootPath, "App_Data", "ApiDocXml");
builder.Services.AddAndInitDynamicApi(mvpBuilder, options =>
{
    options.DocXmlPath = docXmlPath;
    options.DefaultRequestMethod = ApiRequestMethod.Get;
    options.BaseApiControllerType = null;
    options.CopyCustomAttributes = true;
    options.TaskCount = Environment.ProcessorCount * 4;
    options.ShowDetailApiLog = true;
    options.AdditionalAttributeFunc = null;
    options.ForbiddenExternalAccess = true;
});

#endregion

var app = builder.Build();

#region Enable configuration (one line)

// Use the following to manually obtain configuration
var senparcSetting = app.Services.GetService<IOptions<SenparcSetting>>()!.Value;

// Enable WeChat configuration (required)
var registerService = app.UseSenparcGlobal(app.Environment,
    senparcSetting /* When not null, overrides SenpacSetting in appsettings */,
    register =>
    {
        #region CO2NET global configuration

        #region Global cache configuration (as needed)

        // When one distributed cache serves multiple sites (app pools), use a namespace to isolate them (optional)
        register.ChangeDefaultCacheNamespace("CO2NETCache.net10.0");

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
            Senparc.CO2NET.Cache.CsRedis.Register.SetConfigurationOption(redisConfigurationStr);

            // Immediately switch global cache to Redis
            Senparc.CO2NET.Cache.CsRedis.Register.UseKeyValueRedisNow();// Key-value cache strategy (recommended)
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

#endregion


/// <summary>
/// Configure global trace log
/// </summary>
void ConfigTraceLog()
{
    // When Debug is enabled, logs are written under /App_Data/SenparcTraceLog/; disable in production

    // If global IsDebug (Senparc.CO2NET.Config.IsDebug) is false, set true here; otherwise it stays true
    Senparc.CO2NET.Trace.SenparcTrace.SendCustomLog("系统日志", "系统启动");// Only effective when Senparc.CO2NET.Config.IsDebug = true

    // Global custom log callback
    Senparc.CO2NET.Trace.SenparcTrace.OnLogFunc = () =>
    {
        // Code to run after each log event
    };

    Senparc.CO2NET.Trace.SenparcTrace.OnBaseExceptionFunc = ex =>
    {
        // Code to run after each BaseException
    };
}
