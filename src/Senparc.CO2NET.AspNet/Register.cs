/*----------------------------------------------------------------
    Copyright (C) 2024 Senparc

    FileName: Register.cs
    File Function Description: Senparc.CO2NET.AspNet Basic Information Registration

    Creation Identifier: Senparc - 20191230

    Modification Identifier: Senparc - 20221219
    Modification Description: v1.1.3 Optimize UseSenparcGlobal method

    Modification Identifier: Senparc - 20240728
    Modification Description: v1.4.0 .NET 6.0 and .NET 8.0 assemblies no longer depend on Microsoft.AspNetCore.Hosting.Abstractions and Microsoft.AspNetCore.Http.Abstractions

----------------------------------------------------------------*/

#if !NET462
using Microsoft.AspNetCore.Builder;
using System;
using System.Collections.Generic;
using System.Text;
using Senparc.CO2NET.Cache;
using Senparc.CO2NET.RegisterServices;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;
#endif

namespace Senparc.CO2NET.AspNet
{
    /// <summary>
    /// Senparc.CO2NET.AspNet Basic Information Registration
    /// </summary>
    public static class Register
    {
#if !NET462

        /// <summary>
        /// Start Senparc.CO2NET initialization parameter process (ASP.NET Core)
        /// </summary>
        /// <param name="registerService"></param>
        /// <param name="env">IHostingEnvironment (.NET Core 2.0) or IWebHostEnvironment (.NET Core 3.0+)</param>
        /// <param name="senparcSetting">SenparcSetting object</param>
        /// <param name="registerConfigure">RegisterService settings</param>
        /// <param name="autoScanExtensionCacheStrategies">Whether to automatically scan global extension caches (will increase system startup time)</param>
        /// <param name="extensionCacheStrategiesFunc"><para>Extension cache strategies that need to be manually registered</para>
        /// <para>(LocalContainerCacheStrategy, RedisContainerCacheStrategy, MemcacheContainerCacheStrategy are already automatically registered),</para>
        /// <para>If set to null (note: not delegate returns null, but the entire delegate parameter is null), it will automatically use reflection to scan all possible extension cache strategies</para></param>
        /// <returns></returns>
        public static IRegisterService UseSenparcGlobal(this IApplicationBuilder registerService,
            Microsoft.Extensions.Hosting.IHostEnvironment/*IHostingEnvironment*/ env,
            SenparcSetting senparcSetting = null,
            Action<RegisterService> registerConfigure = null,
            bool autoScanExtensionCacheStrategies = false,
            Func<IList<IDomainExtensionCacheStrategy>> extensionCacheStrategiesFunc = null)
        {
            senparcSetting = senparcSetting ?? registerService.ApplicationServices.GetService<IOptions<SenparcSetting>>().Value;

            //Initialize the global RegisterService object and store SenparcSetting information
            var register = Senparc.CO2NET.AspNet.RegisterServices.
                            RegisterService.Start(env, senparcSetting);

            return Senparc.CO2NET.Register.UseSenparcGlobal(senparcSetting, registerConfigure, autoScanExtensionCacheStrategies, extensionCacheStrategiesFunc);
        }
#endif
    }
}
