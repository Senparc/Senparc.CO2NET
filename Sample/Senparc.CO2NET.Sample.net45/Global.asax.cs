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


            //设置全局 Debug 状态
            var isGLobalDebug = true;
            //全局设置参数，将被储存到 Senparc.CO2NET.Config.SenparcSetting
            var senparcSetting = SenparcSetting.BuildFromWebConfig(isGLobalDebug);
            //也可以通过这种方法在程序任意位置设置全局 Debug 状态：
            //Senparc.CO2NET.Config.IsDebug = isGLobalDebug;


            //CO2NET 全局注册，必须！！
            IRegisterService register = RegisterService.Start(senparcSetting)
                                          .UseSenparcGlobal(false, () => GetExCacheStrategies(senparcSetting)) //这里没有 ; 下面接着写

            #region 注册分自定义（分布式）缓存策略（按需，如果需要，必须放在第一个）

                 // 当同一个分布式缓存同时服务于多个网站（应用程序池）时，可以使用命名空间将其隔离（非必须）
                 // 也可以在 senparcSetting.DefaultCacheNamespace 属性上进行设置
                 .ChangeDefaultCacheNamespace("CO2NETCache.net45")

                 //配置Redis缓存
                 .RegisterCacheRedis(
                     senparcSetting.Cache_Redis_Configuration,
                     redisConfiguration => (!string.IsNullOrEmpty(redisConfiguration) && redisConfiguration != "Redis配置")
                                          ? RedisObjectCacheStrategy.Instance
                                          : null)

                 //配置Memcached缓存
                 .RegisterCacheMemcached(
                     new Dictionary<string, int>() {/* { "localhost", 9101 }*/ },
                     memcachedConfig => (memcachedConfig != null && memcachedConfig.Count > 0)
                                         ? MemcachedObjectCacheStrategy.Instance
                                         : null)

            #endregion

            #region 注册日志（按需，建议）

                 .RegisterTraceLog(ConfigTraceLog);//配置TraceLog

            #endregion

        }


        /// <summary>
        /// 配置微信跟踪日志
        /// </summary>
        private void ConfigTraceLog()
        {
            //这里设为Debug状态时，/App_Data/WeixinTraceLog/目录下会生成日志文件记录所有的API请求日志，正式发布版本建议关闭

            //如果全局的IsDebug（Senparc.CO2NET.Config.IsDebug）为false，此处可以单独设置true，否则自动为true
            CO2NET.Trace.SenparcTrace.SendCustomLog("系统日志", "系统启动");//只在Senparc.Weixin.Config.IsDebug = true的情况下生效

            //全局自定义日志记录回调
            CO2NET.Trace.SenparcTrace.OnLogFunc = () =>
            {
                //加入每次触发Log后需要执行的代码
            };

            CO2NET.Trace.SenparcTrace.OnBaseExceptionFunc = ex =>
            {
                //加入每次触发BaseException后需要执行的代码
            };
        }

        /// <summary>
        /// 获取扩展缓存策略
        /// </summary>
        /// <returns></returns>
        private IList<IDomainExtensionCacheStrategy> GetExCacheStrategies(SenparcSetting senparcSetting)
        {
            var exContainerCacheStrategies = new List<IDomainExtensionCacheStrategy>();
            senparcSetting = senparcSetting ?? new SenparcSetting();

            //注意：以下两个 if 判断仅作为演示，方便大家添加自定义的扩展缓存策略，
            //      只要进行了 register.UseSenparcWeixin() 操作，Container 的缓存策略下的 Local、Redis 和 Memcached 系统已经默认自动注册，无需操作！

            #region 演示扩展缓存注册方法

            /*

            //判断Redis是否可用
            var redisConfiguration = senparcSetting.Cache_Redis_Configuration;
            if ((!string.IsNullOrEmpty(redisConfiguration) && redisConfiguration != "Redis配置"))
            {
                exContainerCacheStrategies.Add(RedisContainerCacheStrategy.Instance);//自定义的扩展缓存
            }

            //判断Memcached是否可用
            var memcachedConfiguration = senparcSetting.Cache_Memcached_Configuration;
            if ((!string.IsNullOrEmpty(memcachedConfiguration) && memcachedConfiguration != "Memcached配置"))
            {
                exContainerCacheStrategies.Add(MemcachedContainerCacheStrategy.Instance);//TODO:如果没有进行配置会产生异常
            }
            */

            #endregion

            //扩展自定义的缓存策略

            return exContainerCacheStrategies;
        }
    }
}
