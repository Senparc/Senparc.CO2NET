﻿#region Apache License Version 2.0
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
    Copyright (C) 2025 Senparc

    文件名：Program.cs
    文件功能描述：Console 示例（同样适用于 WinForm 和 WPF）


    创建标识：Senparc - 20190108

    修改标识：Senparc - 20221219
    修改描述：统一修改参数 RootDictionaryPath 为 RootDirectoryPath

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

//更多绑定操作参见：https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/?view=aspnetcore-2.2
var senparcSetting = new SenparcSetting();
config.GetSection("SenparcSetting").Bind(senparcSetting);

var services = new ServiceCollection();
services.AddMemoryCache();//使用本地缓存必须添加

/*
* CO2NET 是从 Senparc.Weixin 分离的底层公共基础模块，经过了长达 6 年的迭代优化，稳定可靠。
* 关于 CO2NET 在所有项目中的通用设置可参考 CO2NET 的 Sample：
* https://github.com/Senparc/Senparc.CO2NET/blob/master/Sample/Senparc.CO2NET.Sample.netcore/Startup.cs
*/

services.AddSenparcGlobalServices(config);//Senparc.CO2NET 全局注册
Console.WriteLine("完成 AddSenparcGlobalServices 注册");

// 启动 CO2NET 全局注册，必须！
IRegisterService register = RegisterService.Start(senparcSetting)
                                            //关于 UseSenparcGlobal() 的更多用法见 CO2NET Demo：https://github.com/Senparc/Senparc.CO2NET/blob/master/Sample/Senparc.CO2NET.Sample.netcore/Startup.cs
                                            .UseSenparcGlobal();

Console.WriteLine("完成 RegisterService.Start().UseSenparcGlobal()  启动设置");
Console.WriteLine($"设定程序目录为：{Config.RootDirectoryPath}");

#region CO2NET 全局配置

#region 全局缓存配置（按需）

//当同一个分布式缓存同时服务于多个网站（应用程序池）时，可以使用命名空间将其隔离（非必须）
register.ChangeDefaultCacheNamespace("DefaultCO2NETCache");
Console.WriteLine($"默认缓存命名空间替换为：{Config.DefaultCacheNamespace}");


#region 配置和使用 Redis          -- DPBMARK Redis

//配置全局使用Redis缓存（按需，独立）
var redisConfigurationStr = senparcSetting.Cache_Redis_Configuration;
var useRedis = !string.IsNullOrEmpty(redisConfigurationStr) && redisConfigurationStr != "#{Cache_Redis_Configuration}#"/*默认值，不启用*/;
if (useRedis)//这里为了方便不同环境的开发者进行配置，做成了判断的方式，实际开发环境一般是确定的，这里的if条件可以忽略
{
    /* 说明：
     * 1、Redis 的连接字符串信息会从 Config.SenparcSetting.Cache_Redis_Configuration 自动获取并注册，如不需要修改，下方方法可以忽略
    /* 2、如需手动修改，可以通过下方 SetConfigurationOption 方法手动设置 Redis 链接信息（仅修改配置，不立即启用）
     */
    Senparc.CO2NET.Cache.CsRedis.Register.SetConfigurationOption(redisConfigurationStr);
    Console.WriteLine("完成 CsRedis 设置");


    //以下会立即将全局缓存设置为 Redis
    Senparc.CO2NET.Cache.CsRedis.Register.UseKeyValueRedisNow();//键值对缓存策略（推荐）
    Console.WriteLine("启用 CsRedis UseKeyValue 策略");

    //Senparc.CO2NET.Cache.Redis.Register.UseHashRedisNow();//HashSet储存格式的缓存策略

    //也可以通过以下方式自定义当前需要启用的缓存策略
    //CacheStrategyFactory.RegisterObjectCacheStrategy(() => RedisObjectCacheStrategy.Instance);//键值对
    //CacheStrategyFactory.RegisterObjectCacheStrategy(() => RedisHashSetObjectCacheStrategy.Instance);//HashSet
}
//如果这里不进行Redis缓存启用，则目前还是默认使用内存缓存 

#endregion                        // DPBMARK_END

#region 配置和使用 Memcached      -- DPBMARK Memcached

//配置Memcached缓存（按需，独立）
var memcachedConfigurationStr = senparcSetting.Cache_Memcached_Configuration;
var useMemcached = !string.IsNullOrEmpty(memcachedConfigurationStr) && memcachedConfigurationStr != "#{Cache_Memcached_Configuration}#";

if (useMemcached) //这里为了方便不同环境的开发者进行配置，做成了判断的方式，实际开发环境一般是确定的，这里的if条件可以忽略
{
    /* 说明：
    * 1、Memcached 的连接字符串信息会从 Config.SenparcSetting.Cache_Memcached_Configuration 自动获取并注册，如不需要修改，下方方法可以忽略
   /* 2、如需手动修改，可以通过下方 SetConfigurationOption 方法手动设置 Memcached 链接信息（仅修改配置，不立即启用）
    */
    Senparc.CO2NET.Cache.Memcached.Register.SetConfigurationOption(memcachedConfigurationStr);
    Console.WriteLine("完成 Memcached 设置");

    //以下会立即将全局缓存设置为 Memcached
    Senparc.CO2NET.Cache.Memcached.Register.UseMemcachedNow();
    Console.WriteLine("启用 Memcached UseKeyValue 策略");


    //也可以通过以下方式自定义当前需要启用的缓存策略
    CacheStrategyFactory.RegisterObjectCacheStrategy(() => MemcachedObjectCacheStrategy.Instance);
    Console.WriteLine("立即启用 Memcached 策略");
}

#endregion                        //  DPBMARK_END

#endregion

#region 注册日志（按需，建议）

register.RegisterTraceLog(ConfigTraceLog);//配置TraceLog

#endregion

#endregion

Console.WriteLine("Hello CO2NET!");
Console.WriteLine($"Total initialization time: {SystemTime.DiffTotalMS(dt1)}ms");

var cacheStrategy = CacheStrategyFactory.GetObjectCacheStrategyInstance();
Console.WriteLine($"当前缓存策略: {cacheStrategy}");
var servierProviderScope = services.BuildServiceProvider().CreateScope();
var cache = servierProviderScope.ServiceProvider.GetRequiredService<IBaseObjectCacheStrategy>();
Console.WriteLine($"依赖注入缓存策略: {cache}（{(cache == cacheStrategy ? "成功" : "失败")}）");

//存入缓存
await cache.SetAsync("Setting", Config.SenparcSetting);

//读取缓存
var settingFromCache = await cache.GetAsync<SenparcSetting>("Setting");

Console.WriteLine($"从缓读取 SenparcSetting: {settingFromCache.ToJson(true)}");


Console.ReadLine();

/// <summary>
/// 配置微信跟踪日志
/// </summary>
static void ConfigTraceLog()
{
    //这里设为Debug状态时，/App_Data/WeixinTraceLog/目录下会生成日志文件记录所有的API请求日志，正式发布版本建议关闭

    //如果全局的IsDebug（Senparc.CO2NET.Config.IsDebug）为false，此处可以单独设置true，否则自动为true
    SenparcTrace.SendCustomLog("系统日志", "系统启动");//只在Senparc.Weixin.Config.IsDebug = true的情况下生效

    //全局自定义日志记录回调
    SenparcTrace.OnLogFunc = () =>
    {
        //加入每次触发Log后需要执行的代码
    };

    Console.WriteLine("完成日志设置，已经记录 1 条系统启动日志");
}