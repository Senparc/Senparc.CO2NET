#region Apache License Version 2.0
/*----------------------------------------------------------------

Copyright 2019 Suzhou Senparc Network Technology Co.,Ltd.

Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file
except in compliance with the License. You may obtain a copy of the License at

http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software distributed under the
License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND,
either express or implied. See the License for the specific language governing permissions
and limitations under the License.

Detail: https://github.com/Senparc/Senparc.CO2NET/blob/master/LICENSE

----------------------------------------------------------------*/
#endregion Apache License Version 2.0

/*----------------------------------------------------------------
    Copyright (C) 2026 Senparc

    File name: Program.cs
    File description: Console sample (also applies to WinForm and WPF)


    Create identity: Senparc - 20190108

    Modify identity: Senparc - 20221219
    Modify description: Rename parameter RootDictionaryPath to RootDirectoryPath

----------------------------------------------------------------*/

#region usings
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Senparc.CO2NET;
using Senparc.CO2NET.Cache;
using Senparc.CO2NET.Cache.Memcached;
using Senparc.CO2NET.Extensions;
using Senparc.CO2NET.RegisterServices;
using Senparc.CO2NET.Trace;
using System;
#endregion

var dt1 = SystemTime.Now;

var configBuilder = new ConfigurationBuilder();
configBuilder.AddJsonFile("appsettings.json", false, false);
Console.WriteLine("完成 appsettings.json 添加");

var config = configBuilder.Build();
Console.WriteLine("完成 ServiceCollection 和 ConfigurationBuilder 初始化");

// More binding options: https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/?view=aspnetcore-2.2
var senparcSetting = new SenparcSetting();
config.GetSection("SenparcSetting").Bind(senparcSetting);

var services = new ServiceCollection();
services.AddMemoryCache();// Required when using local cache

/*
* CO2NET is the foundational module split from Senparc.Weixin, refined over six years and stable in production.
* For common CO2NET setup across projects, see the CO2NET Sample:
* https://github.com/Senparc/Senparc.CO2NET/blob/master/Sample/Senparc.CO2NET.Sample.netcore/Startup.cs
*/

services.AddSenparcGlobalServices(config);// Senparc.CO2NET global registration
Console.WriteLine("完成 AddSenparcGlobalServices 注册");

// Start CO2NET global registration (required)
IRegisterService register = RegisterService.Start(senparcSetting)
                                            // More UseSenparcGlobal() examples: https://github.com/Senparc/Senparc.CO2NET/blob/master/Sample/Senparc.CO2NET.Sample.netcore/Startup.cs
                                            .UseSenparcGlobal();

Console.WriteLine("完成 RegisterService.Start().UseSenparcGlobal()  启动设置");
Console.WriteLine($"设定程序目录为：{Config.RootDirectoryPath}");

#region CO2NET global configuration

#region Global cache configuration (as needed)

// When one distributed cache serves multiple sites (app pools), use a namespace to isolate them (optional)
register.ChangeDefaultCacheNamespace("DefaultCO2NETCache");
Console.WriteLine($"默认缓存命名空间替换为：{Config.DefaultCacheNamespace}");


#region Configure and use Redis          -- DPBMARK Redis

// Configure global Redis cache (optional, independent)
var redisConfigurationStr = senparcSetting.Cache_Redis_Configuration;
var useRedis = !string.IsNullOrEmpty(redisConfigurationStr) && redisConfigurationStr != "#{Cache_Redis_Configuration}#"/* default placeholder, disabled */;
if (useRedis)// For convenience across environments this is conditional; in production the if can usually be ignored
{
    /* Notes:
     * 1. Redis connection string is read from Config.SenparcSetting.Cache_Redis_Configuration automatically; skip SetConfigurationOption if unchanged
    /* 2. To override manually, use SetConfigurationOption below (config only, does not enable immediately)
     */
    Senparc.CO2NET.Cache.CsRedis.Register.SetConfigurationOption(redisConfigurationStr);
    Console.WriteLine("完成 CsRedis 设置");


    // Immediately switch global cache to Redis
    Senparc.CO2NET.Cache.CsRedis.Register.UseKeyValueRedisNow();// Key-value cache strategy (recommended)
    Console.WriteLine("启用 CsRedis UseKeyValue 策略");

    //Senparc.CO2NET.Cache.Redis.Register.UseHashRedisNow();// HashSet storage cache strategy

    // Or register a custom cache strategy explicitly
    //CacheStrategyFactory.RegisterObjectCacheStrategy(() => RedisObjectCacheStrategy.Instance);// Key-value
    //CacheStrategyFactory.RegisterObjectCacheStrategy(() => RedisHashSetObjectCacheStrategy.Instance);// HashSet
}
// If Redis is not enabled here, in-memory cache remains the default

#endregion                        // DPBMARK_END

#region Configure and use Memcached      -- DPBMARK Memcached

// Configure Memcached cache (optional, independent)
var memcachedConfigurationStr = senparcSetting.Cache_Memcached_Configuration;
var useMemcached = !string.IsNullOrEmpty(memcachedConfigurationStr) && memcachedConfigurationStr != "#{Cache_Memcached_Configuration}#";

if (useMemcached) // For convenience across environments this is conditional; in production the if can usually be ignored
{
    /* Notes:
    * 1. Memcached connection string is read from Config.SenparcSetting.Cache_Memcached_Configuration automatically; skip SetConfigurationOption if unchanged
   /* 2. To override manually, use SetConfigurationOption below (config only, does not enable immediately)
    */
    Senparc.CO2NET.Cache.Memcached.Register.SetConfigurationOption(memcachedConfigurationStr);
    Console.WriteLine("完成 Memcached 设置");

    // Immediately switch global cache to Memcached
    Senparc.CO2NET.Cache.Memcached.Register.UseMemcachedNow();
    Console.WriteLine("启用 Memcached UseKeyValue 策略");


    // Or register a custom cache strategy explicitly
    CacheStrategyFactory.RegisterObjectCacheStrategy(() => MemcachedObjectCacheStrategy.Instance);
    Console.WriteLine("立即启用 Memcached 策略");
}

#endregion                        //  DPBMARK_END

#endregion

#region Register trace log (optional, recommended)

register.RegisterTraceLog(ConfigTraceLog);// Configure TraceLog

#endregion

#endregion

Console.WriteLine("Hello CO2NET!");
Console.WriteLine($"Total initialization time: {SystemTime.DiffTotalMS(dt1)}ms");

var cacheStrategy = CacheStrategyFactory.GetObjectCacheStrategyInstance();
Console.WriteLine($"当前缓存策略: {cacheStrategy}");
var servierProviderScope = services.BuildServiceProvider().CreateScope();
var cache = servierProviderScope.ServiceProvider.GetRequiredService<IBaseObjectCacheStrategy>();
Console.WriteLine($"依赖注入缓存策略: {cache}（{(cache == cacheStrategy ? "成功" : "失败")}）");

// Write to cache
await cache.SetAsync("Setting", Config.SenparcSetting);

// Read from cache
var settingFromCache = await cache.GetAsync<SenparcSetting>("Setting");

Console.WriteLine($"从缓读取 SenparcSetting: {settingFromCache.ToJson(true)}");


Console.ReadLine();

/// <summary>
/// Configure WeChat trace log
/// </summary>
static void ConfigTraceLog()
{
    // When Debug is enabled, logs are written under /App_Data/WeixinTraceLog/; disable in production

    // If global IsDebug (Senparc.CO2NET.Config.IsDebug) is false, set true here; otherwise it stays true
    SenparcTrace.SendCustomLog("系统日志", "系统启动");// Only effective when Senparc.Weixin.Config.IsDebug = true

    // Global custom log callback
    SenparcTrace.OnLogFunc = () =>
    {
        // Code to run after each log event
    };

    Console.WriteLine("完成日志设置，已经记录 1 条系统启动日志");
}