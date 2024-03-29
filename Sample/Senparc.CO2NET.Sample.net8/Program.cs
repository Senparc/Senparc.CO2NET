using System;
using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Senparc.CO2NET.AspNet;
using Senparc.CO2NET.Helpers;
using Senparc.CO2NET.RegisterServices;
using Senparc.CO2NET.Sample.net8.Services;
using Senparc.CO2NET.WebApi;
using Senparc.CO2NET.WebApi.WebApiEngines;
using Senparc.CO2NET;
using Microsoft.Extensions.Options;
using Senparc.CO2NET.Cache;
using Senparc.CO2NET.Cache.Memcached;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var mvpBuilder = builder.Services.AddControllersWithViews();

//使用本地缓存必须添加
builder.Services.AddMemoryCache();

#region 添加全局配置（一行代码）

//Senparc.Weixin 注册（必须）
builder.Services.AddSenparcGlobalServices(builder.Configuration);

#endregion


#region WebApiEngine（可选）

//忽略测试，注释掉以下代码后，可看到微信公众号SDK接口及注释信息
Senparc.CO2NET.WebApi.Register.OmitCategoryList.Add(Senparc.NeuChar.PlatformType.WeChat_OfficialAccount.ToString());

//额外增加测试
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

#region 启用配置（一句代码）

//手动获取配置信息可使用以下方法
var senparcSetting = app.Services.GetService<IOptions<SenparcSetting>>()!.Value;

//启用微信配置（必须）
var registerService = app.UseSenparcGlobal(app.Environment,
    senparcSetting /* 不为 null 则覆盖 appsettings  中的 SenpacSetting 配置*/,
    register =>
    {
        #region CO2NET 全局配置

        #region 全局缓存配置（按需）

        //当同一个分布式缓存同时服务于多个网站（应用程序池）时，可以使用命名空间将其隔离（非必须）
        register.ChangeDefaultCacheNamespace("CO2NETCache.net8.0");

        #region 配置和使用 Redis

        //配置全局使用Redis缓存（按需，独立）
        var redisConfigurationStr = senparcSetting.Cache_Redis_Configuration;
        var useRedis = !string.IsNullOrEmpty(redisConfigurationStr) && redisConfigurationStr != "Redis配置";
        if (useRedis)//这里为了方便不同环境的开发者进行配置，做成了判断的方式，实际开发环境一般是确定的，这里的if条件可以忽略
        {
            /* 说明：
             * 1、Redis 的连接字符串信息会从 Config.SenparcSetting.Cache_Redis_Configuration 自动获取并注册，如不需要修改，下方方法可以忽略
            /* 2、如需手动修改，可以通过下方 SetConfigurationOption 方法手动设置 Redis 链接信息（仅修改配置，不立即启用）
             */
            Senparc.CO2NET.Cache.CsRedis.Register.SetConfigurationOption(redisConfigurationStr);

            //以下会立即将全局缓存设置为 Redis
            Senparc.CO2NET.Cache.CsRedis.Register.UseKeyValueRedisNow();//键值对缓存策略（推荐）
                                                                        //Senparc.CO2NET.Cache.Redis.Register.UseHashRedisNow();//HashSet储存格式的缓存策略

            //也可以通过以下方式自定义当前需要启用的缓存策略
            //CacheStrategyFactory.RegisterObjectCacheStrategy(() => RedisObjectCacheStrategy.Instance);//键值对
            //CacheStrategyFactory.RegisterObjectCacheStrategy(() => RedisHashSetObjectCacheStrategy.Instance);//HashSet
        }
        //如果这里不进行Redis缓存启用，则目前还是默认使用内存缓存 

        #endregion

        #region 配置和使用 Memcached

        //配置Memcached缓存（按需，独立）
        var memcachedConfigurationStr = senparcSetting.Cache_Memcached_Configuration;
        var useMemcached = !string.IsNullOrEmpty(memcachedConfigurationStr) && memcachedConfigurationStr != "Memcached配置";

        if (useMemcached) //这里为了方便不同环境的开发者进行配置，做成了判断的方式，实际开发环境一般是确定的，这里的if条件可以忽略
        {
            app.UseEnyimMemcached();

            /* 说明：
            * 1、Memcached 的连接字符串信息会从 Config.SenparcSetting.Cache_Memcached_Configuration 自动获取并注册，如不需要修改，下方方法可以忽略
           /* 2、如需手动修改，可以通过下方 SetConfigurationOption 方法手动设置 Memcached 链接信息（仅修改配置，不立即启用）
            */
            Senparc.CO2NET.Cache.Memcached.Register.SetConfigurationOption(redisConfigurationStr);

            //以下会立即将全局缓存设置为 Memcached
            Senparc.CO2NET.Cache.Memcached.Register.UseMemcachedNow();

            //也可以通过以下方式自定义当前需要启用的缓存策略
            CacheStrategyFactory.RegisterObjectCacheStrategy(() => MemcachedObjectCacheStrategy.Instance);
        }

        #endregion

        #endregion

        #region 注册日志（按需，建议）

        register.RegisterTraceLog(ConfigTraceLog);//配置TraceLog

        #endregion

        #endregion
    },

#region 扫描自定义扩展缓存

    //自动扫描自定义扩展缓存（二选一）
    autoScanExtensionCacheStrategies: true //默认为 true，可以不传入
                                           //指定自定义扩展缓存（二选一）
                                           //autoScanExtensionCacheStrategies: false, extensionCacheStrategiesFunc: () => GetExCacheStrategies(senparcSetting.Value)

#endregion
            );

#endregion


/// <summary>
/// 配置全局跟踪日志
/// </summary>
void ConfigTraceLog()
{
    //这里设为Debug状态时，/App_Data/SenparcTraceLog/目录下会生成日志文件记录所有的API请求日志，正式发布版本建议关闭

    //如果全局的IsDebug（Senparc.CO2NET.Config.IsDebug）为false，此处可以单独设置true，否则自动为true
    Senparc.CO2NET.Trace.SenparcTrace.SendCustomLog("系统日志", "系统启动");//只在Senparc.CO2NET.Config.IsDebug = true的情况下生效

    //全局自定义日志记录回调
    Senparc.CO2NET.Trace.SenparcTrace.OnLogFunc = () =>
    {
        //加入每次触发Log后需要执行的代码
    };

    Senparc.CO2NET.Trace.SenparcTrace.OnBaseExceptionFunc = ex =>
    {
        //加入每次触发BaseException后需要执行的代码
    };
}
