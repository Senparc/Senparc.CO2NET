/*----------------------------------------------------------------
    Copyright (C) 2018 Senparc

    文件名：RegisterService.cs
    文件功能描述：Senparc.Weixin SDK 快捷注册流程


    创建标识：Senparc - 20180222

    修改标识：Senparc - 20180531
    修改描述：v4.22.2 修改 AddSenparcWeixinGlobalServices() 方法命名
    
    ----  CO2NET   ----

    修改标识：Senparc - 20180704
    修改描述：v0.1.5 RegisterServiceExtension.AddSenparcGlobalServices() 方法可自动获取 SenparcSetting 全局设置

----------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#if NETCOREAPP2_0 || NETCOREAPP2_1
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
#endif

namespace Senparc.CO2NET.RegisterServices
{
    /// <summary>
    /// 快捷注册类，RegisterService 扩展类
    /// </summary>
    public static class RegisterServiceExtension
    {
#if NETCOREAPP2_0 || NETCOREAPP2_1
        /// <summary>
        /// 注册 IServiceCollection，并返回 RegisterService，开始注册流程
        /// </summary>
        /// <param name="serviceCollection">IServiceCollection</param>
        /// <param name="configuration">IConfiguration</param>
        /// <returns></returns>
        public static IServiceCollection AddSenparcGlobalServices(this IServiceCollection serviceCollection, IConfiguration configuration)
        {
            RegisterService.GlobalServiceCollection = serviceCollection;
            serviceCollection.Configure<SenparcSetting>(configuration.GetSection("SenparcSetting"));

            /*
             * appsettings.json 中添加节点：
 //CO2NET 设置
  "SenparcSetting": {
    "IsDebug": true,
    "DefaultCacheNamespace": "DefaultCache"
  },
             */

            return serviceCollection;
        }
#endif
    }
}
