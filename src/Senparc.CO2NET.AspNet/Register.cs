#if !NET45
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Builder;
#endif

using System;
using System.Collections.Generic;
using System.Text;
using Senparc.CO2NET.Cache;
using Senparc.CO2NET.RegisterServices;

namespace Senparc.CO2NET.AspNet
{
    /// <summary>
    /// Senparc.CO2NET.AspNet 基础信息注册
    /// </summary>
    public static class Register
    {
#if !NET45

        /// <summary>
        /// 开始 Senparc.CO2NET 初始化参数流程（ASP.NET Core)
        /// </summary>
        /// <param name="registerService"></param>
        /// <param name="env">IHostingEnvironment（.NET Core 2.0） 或 IWebHostEnvironment（.NET Core 3.0）</param>
        /// <param name="senparcSetting">SenparcSetting 对象</param>
        /// <param name="registerConfigure">RegisterService 设置</param>
        /// <param name="autoScanExtensionCacheStrategies">是否自动扫描全局的扩展缓存（会增加系统启动时间）</param>
        /// <param name="extensionCacheStrategiesFunc"><para>需要手动注册的扩展缓存策略</para>
        /// <para>（LocalContainerCacheStrategy、RedisContainerCacheStrategy、MemcacheContainerCacheStrategy已经自动注册），</para>
        /// <para>如果设置为 null（注意：不适委托返回 null，是整个委托参数为 null），则自动使用反射扫描所有可能存在的扩展缓存策略</para></param>
        /// <returns></returns>
        public static IRegisterService UseSenparcGlobal(this IApplicationBuilder registerService,
#if NETSTANDARD2_0
            Microsoft.Extensions.Hosting.IHostEnvironment/*IHostingEnvironment*/ env,
#else
            Microsoft.Extensions.Hosting.IHostEnvironment/*IWebHostEnvironment*/ env,
#endif
            SenparcSetting senparcSetting,
            Action<RegisterService> registerConfigure,
            bool autoScanExtensionCacheStrategies = false,
            Func<IList<IDomainExtensionCacheStrategy>> extensionCacheStrategiesFunc = null)
        {
            //初始化全局 RegisterService 对象，并储存 SenparcSetting 信息
            var register = Senparc.CO2NET.AspNet.RegisterServices.
                            RegisterService.Start(env, senparcSetting);

            return Senparc.CO2NET.Register.UseSenparcGlobal(senparcSetting, registerConfigure, autoScanExtensionCacheStrategies, extensionCacheStrategiesFunc);
        }
#endif
    }
}
