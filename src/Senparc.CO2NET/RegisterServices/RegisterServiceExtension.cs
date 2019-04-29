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
    Copyright (C) 2018 Senparc

    文件名：RegisterService.cs
    文件功能描述：Senparc.Weixin SDK 快捷注册流程


    创建标识：Senparc - 20180222

    修改标识：Senparc - 20180531
    修改描述：v4.22.2 修改 AddSenparcWeixinGlobalServices() 方法命名
    
    ----  CO2NET   ----

    修改标识：Senparc - 20180704
    修改描述：v0.1.5 RegisterServiceExtension.AddSenparcGlobalServices() 方法可自动获取 SenparcSetting 全局设置

    修改标识：Senparc - 201929
    修改描述：v0.7.0 优化 HttpClient，重构 RequestUtility（包括 Post 和 Get），引入 HttpClientFactory 机制

----------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Senparc.CO2NET.HttpUtility;

#if NETSTANDARD2_0
using System.Net.Http;
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
#if NETSTANDARD2_0
        /// <summary>
        /// 注册 IServiceCollection，并返回 RegisterService，开始注册流程（必须）
        /// </summary>
        /// <param name="serviceCollection">IServiceCollection</param>
        /// <param name="configuration">IConfiguration</param>
        /// <returns></returns>
        public static IServiceCollection AddSenparcGlobalServices(this IServiceCollection serviceCollection,
            IConfiguration configuration)
        {
            SenparcDI.GlobalServiceCollection = serviceCollection;
            serviceCollection.Configure<SenparcSetting>(configuration.GetSection("SenparcSetting"));

            //配置 HttpClient，课使用 Head 自定义 Cookie
            serviceCollection.AddHttpClient<SenparcHttpClient>()
            //.ConfigureHttpMessageHandlerBuilder((c) =>
            .ConfigurePrimaryHttpMessageHandler((c) =>
            {
                var httpClientHandler = HttpClientHelper.GetHttpClientHandler(null, RequestUtility.SenparcHttpClientWebProxy, System.Net.DecompressionMethods.GZip);
                return httpClientHandler;
            });

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

        /// <summary>
        /// 添加作用于 SenparcHttpClient 的 WebProxy（需要在 AddSenparcGlobalServices 之前定义）
        /// </summary>
        /// <param name="serviceCollection"></param>
        /// <param name="host"></param>
        /// <param name="port"></param>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public static IServiceCollection AddSenparcHttpClientProxy(this IServiceCollection serviceCollection, string host, string port, string username, string password)
        {
            RequestUtility.SetHttpProxy(host, port, username, password);

            return serviceCollection;
        }
#endif
    }
}
