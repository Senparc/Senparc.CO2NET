using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Senparc.CO2NET.Cache;
using Senparc.CO2NET.Cache.Redis;
using Senparc.CO2NET.Cache.Memcached;
using Senparc.CO2NET.RegisterServices;

namespace Senparc.CO2NET.Sample.netcore
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
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });


            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            services.AddMemoryCache();//使用本地缓存必须添加

            services.AddSenparcGlobalServices(Configuration);//Senparc.CO2NET 全局注册

            #region Senparc.CO2NET Memcached 配置（按需）

            //添加Memcached配置（按需）
            services.AddSenparcMemcached(options =>
            {
                options.AddServer("memcached", 11211);
                //options.AddPlainTextAuthenticator("", "usename", "password");
            });

            #endregion

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IOptions<SenparcSetting> senparcSetting)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });


            // 启动 CO2NET 全局注册，必须！
            IRegisterService register = RegisterService.Start(env, senparcSetting.Value)
                                                        .UseSenparcGlobal(false, () => GetExCacheStrategies(senparcSetting.Value));

            //如果需要自动扫描自定义扩展缓存，可以这样使用：
            //register.UseSenparcWeixin(true);
            //如果需要指定自定义扩展缓存，可以这样用：
            //register.UseSenparcWeixin(false, GetExCacheStrategies);

            #region CO2NET 全局配置

            #region 注册线程，在 RegisterService.Start() 中已经自动注册，此处也可以省略，仅作演示

            register.RegisterThreads();  //启动线程，RegisterThreads()也可以省略，在RegisterService.Start()中已经自动注册

            #endregion

            #region 缓存配置（按需）

            //当同一个分布式缓存同时服务于多个网站（应用程序池）时，可以使用命名空间将其隔离（非必须）
            register.ChangeDefaultCacheNamespace("CO2NETCache.netcore");

            //配置全局使用Redis缓存（按需，独立）
            var redisConfigurationStr = senparcSetting.Value.Cache_Redis_Configuration;
            var useRedis = !string.IsNullOrEmpty(redisConfigurationStr) && redisConfigurationStr != "Redis配置";
            if (useRedis)//这里为了方便不同环境的开发者进行配置，做成了判断的方式，实际开发环境一般是确定的
            {
                //设置Redis链接信息，并在全局立即启用Redis缓存。
                register.RegisterCacheRedis(redisConfigurationStr, redisConfiguration => RedisObjectCacheStrategy.Instance);

                //此外还可以通过这种方式修改 Redis 链接信息（不立即启用）：
                //RedisManager.ConfigurationOption = redisConfigurationStr;

                //以下会立即将全局缓存设置为Redis（不修改配置）。
                //如果要使用其他缓存，同样可以在任意地方使用这个方法，修改 RedisObjectCacheStrategy 为其他缓存策略
                //CacheStrategyFactory.RegisterObjectCacheStrategy(() => RedisObjectCacheStrategy.Instance);
            }
            //如果这里不进行Redis缓存启用，则目前还是默认使用内存缓存 


            //配置Memcached缓存（按需，独立）
            //这里配置的是 CO2NET 的 Memcached 缓存（如果执行了下面的 app.UseSenparcWeixinCacheMemcached()，
            //会自动包含本步骤，这一步注册可以忽略）
            var useMemcached = false;
            app.UseWhen(h => useMemcached, a =>
            {
                a.UseEnyimMemcached();
                //确保Memcached连接可用后，启用下面的做法：
                //var memcachedConnStr = senparcSetting.Value.Cache_Memcached_Configuration;
                //var memcachedConnDic = new Dictionary<string, int>() {/*进行配置 { "localhost", 9101 }*/ };//可以由 memcachedConnStr 分割得到，或直接填写
                //register.RegisterCacheMemcached(memcachedConnDic, memcachedConfig => MemcachedObjectCacheStrategy.Instance);
            });


            #endregion

            #region 注册日志（按需，建议）

            register.RegisterTraceLog(ConfigTraceLog);//配置TraceLog

            #endregion

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
