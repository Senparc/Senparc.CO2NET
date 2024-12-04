#region Apache License Version 2.0
/*----------------------------------------------------------------

Copyright 2024 Suzhou Senparc Network Technology Co.,Ltd.

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
    Copyright (C) 2024 Senparc

    FileName：RegisterService.cs
    File Function Description：Quick registration class, RegisterService extension class


    Creation Identifier：Senparc - 20180222

    Modification Identifier：Senparc - 20180531
    Modification Description：v4.22.2 Modified AddSenparcWeixinGlobalServices() method name
    
    ----  CO2NET   ----

    Modification Identifier：Senparc - 20180704
    Modification Description：v0.1.5 RegisterServiceExtension.AddSenparcGlobalServices() method can automatically obtain SenparcSetting global settings

    Modification Identifier：Senparc - 20190429
    Modification Description：v0.7.0 Optimized HttpClient, refactored RequestUtility (including Post and Get), introduced HttpClientFactory mechanism

    Modification Identifier：Senparc - 20190521
    Modification Description：v0.7.3 .NET Core provides multi-certificate registration function

    Modification Identifier：Senparc - 20200220
    Modification Description：v1.1.100 Refactored SenparcDI

    Modification Identifier：Senparc - 20241119
    Modification Description：v1.1.100 Refactored SenparcDI

----------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Senparc.CO2NET.HttpUtility;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using System.IO;
using Senparc.CO2NET.Cache;



#if !NET462
using System.Net.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
#endif

namespace Senparc.CO2NET.RegisterServices
{
    /// <summary>
    /// Quick registration class, RegisterService extension class
    /// </summary>
    public static class RegisterServiceExtension
    {
#if !NET462 

        /// <summary>
        /// Whether global registration has been performed
        /// </summary>
        public static bool SenparcGlobalServicesRegistered { get; set; }

        /// <summary>0781-B2EB0781-B2EB0781-B2EB0781-B2EB0781-B2EB0781-B2EB
        /// Register IServiceCollection and return RegisterService to start the registration process (required)
        /// </summary>
        /// <param name="serviceCollection">IServiceCollection</param>
        /// <param name="configuration">IConfiguration</param>
        /// <returns></returns>
        public static IServiceCollection AddSenparcGlobalServices(this IServiceCollection serviceCollection,
            IConfiguration configuration)
        {
            SenparcDI.GlobalServiceCollection = serviceCollection;
            serviceCollection.Configure<SenparcSetting>(configuration.GetSection("SenparcSetting"));

            //Senparc.CO2NET SDK configuration
            var senparcSetting = configuration.GetSection("SenparcSetting").Get<SenparcSetting>();
            if (senparcSetting != null)
            {
                Senparc.CO2NET.Config.SenparcSetting = senparcSetting;
            }

            serviceCollection.AddTransient<IBaseObjectCacheStrategy>(s => CacheStrategyFactory.GetObjectCacheStrategyInstance());

            serviceCollection.AddScoped<ApiClientHelper>();

            // .net core 8.0 HttpClient documentation reference: https://learn.microsoft.com/zh-cn/aspnet/core/fundamentals/http-requests?view=aspnetcore-8.0
            //Configure HttpClient, can use Head to customize Cookie
            serviceCollection.AddHttpClient<SenparcHttpClient>()
            //.ConfigureHttpMessageHandlerBuilder((c) =>
            .ConfigurePrimaryHttpMessageHandler((c) =>
            {
                var httpClientHandler = HttpClientHelper.GetHttpClientHandler(null, RequestUtility.SenparcHttpClientWebProxy, System.Net.DecompressionMethods.GZip);
                return httpClientHandler;
            });

            /*
             * Add node in appsettings.json:
 //CO2NET settings
  "SenparcSetting": {
    "IsDebug": true,
    "DefaultCacheNamespace": "DefaultCache"
  },
             */

            SenparcGlobalServicesRegistered = true;

            //var serviceProvider  = serviceCollection.BuildServiceProvider();
            //SenparcDI.GlobalServiceProvider = serviceProvider;
            //return serviceProvider;

            return serviceCollection;
        }

        /// <summary>
        /// Register IServiceCollection and return RegisterService to start the registration process (required)
        /// </summary>
        /// <param name="serviceCollection">IServiceCollection</param>
        /// <param name="certName">Certificate name, must be globally unique and ensure uniqueness within the global HttpClientFactory</param>
        /// <param name="certSecret">Certificate password</param>
        /// <param name="certPath">Certificate path (physical path)</param>
        /// <param name="checkValidationResult">Settings</param>
        /// <returns></returns>
        public static IServiceCollection AddSenparcHttpClientWithCertificate(this IServiceCollection serviceCollection,
            string certName, string certSecret, string certPath, bool checkValidationResult = false)
        {
            //Add registration
            if (!string.IsNullOrEmpty(certPath))
            {
                if (File.Exists(certPath))
                {
                    try
                    {
                        var cert = new X509Certificate2(certPath, certSecret, X509KeyStorageFlags.PersistKeySet | X509KeyStorageFlags.MachineKeySet);
                        return AddSenparcHttpClientWithCertificate(serviceCollection, certName, cert, checkValidationResult);
                    }
                    catch (Exception ex)
                    {
                        Senparc.CO2NET.Trace.SenparcTrace.SendCustomLog($"添加微信支付证书发生异常", $"certName:{certName},certPath:{certPath}");
                        Senparc.CO2NET.Trace.SenparcTrace.BaseExceptionLog(ex);
                        return serviceCollection;
                    }
                }
                else
                {
                    Senparc.CO2NET.Trace.SenparcTrace.SendCustomLog($"已设置微信支付证书，但无法找到文件", $"certName:{certName},certPath:{certPath}");
                    return serviceCollection;
                }
            }
            return serviceCollection;
        }

        /// <summary>
        /// Register IServiceCollection and return RegisterService to start the registration process (required)
        /// </summary>
        /// <param name="serviceCollection">IServiceCollection</param>
        /// <param name="certName">Certificate name, must be globally unique and ensure uniqueness within the global HttpClientFactory</param>
        /// <param name="cert">Certificate object, can also be X509Certificate2</param>
        /// <param name="checkValidationResult">Settings</param>
        /// <returns></returns>
        public static IServiceCollection AddSenparcHttpClientWithCertificate(this IServiceCollection serviceCollection,
            string certName, X509Certificate cert, bool checkValidationResult = false)
        {
            serviceCollection.AddHttpClient<SenparcHttpClient>(certName)
                         .ConfigurePrimaryHttpMessageHandler(() =>
                         {
                             var httpClientHandler = HttpClientHelper.GetHttpClientHandler(null, RequestUtility.SenparcHttpClientWebProxy, System.Net.DecompressionMethods.GZip);

                             httpClientHandler.ClientCertificates.Add(cert);

                             if (checkValidationResult)
                             {
                                 httpClientHandler.ServerCertificateCustomValidationCallback = new Func<HttpRequestMessage, X509Certificate2, X509Chain, SslPolicyErrors, bool>(RequestUtility.CheckValidationResult);
                             }

                             return httpClientHandler;
                         });

            //serviceCollection.ResetGlobalIServiceProvider();//Reset GlobalIServiceProvider
            return serviceCollection;
        }

        /// <summary>
        /// Add WebProxy for SenparcHttpClient (needs to be defined before AddSenparcGlobalServices)
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
