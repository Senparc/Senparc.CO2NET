#region Apache License Version 2.0
/*----------------------------------------------------------------

Copyright 2025 Suzhou Senparc Network Technology Co.,Ltd.

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

    文件名：ApiClient.cs
    文件功能描述：ApiClient, used to provide APiClient Container, work for such as Aspire.


    创建标识：Senparc - 20241118

----------------------------------------------------------------*/


#if !NET462
using Microsoft.Extensions.DependencyInjection;
using Senparc.CO2NET.Extensions;
using Senparc.CO2NET.HttpUtility;
using System;
using System.Collections.Generic;
using System.Text;

namespace Senparc.CO2NET
{
    /// <summary>
    /// ApiClient, used to provide APiClient Container, work for such as Aspire.
    /// </summary>
    public sealed class ApiClient
    {
        public SenparcHttpClient SenparcHttpClient { get; private set; }

        internal ApiClient(IServiceProvider serviceProvider,string httpClientName, string apiClientName)
        {
            SenparcHttpClient = SenparcHttpClient.GetInstanceByName(serviceProvider, httpClientName);

            if (!apiClientName.IsNullOrEmpty())
            {
                SetBaseAddress(apiClientName);
            }
        }

        internal ApiClient(IServiceProvider serviceProvider, string apiClientName)
        {
            SenparcHttpClient = serviceProvider.GetService<SenparcHttpClient>();
            SetBaseAddress(apiClientName);
        }

        private void SetBaseAddress(string apiClientName)
        { 
            SenparcHttpClient.Client.BaseAddress = new($"https+http://{apiClientName}");
        }
    }

    /// <summary>
    /// ApiClientHelper, set ApiClient parameters and get ApiClient object
    /// </summary>
    public sealed class ApiClientHelper
    {
        private readonly IServiceProvider _serviceProvider;

        public ApiClientHelper(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Use an apiClientName to get the ApiClient object
        /// </summary>
        /// <param name="apiClientName"></param>
        /// <returns></returns>
        public ApiClient ConnectApiClient(string apiClientName)
        {
            var apiClient = new ApiClient(_serviceProvider, apiClientName);
            return apiClient;
        }

        /// <summary>
        /// Use an httpClientName and an apiClientName(optional) to get the ApiClient object
        /// </summary>
        /// <param name="httpClientName"></param>
        /// <param name="apiClientName"></param>
        /// <returns></returns>
        public ApiClient ConnectHttpClient(string httpClientName, string apiClientName = null)
        {
            var apiClient = new ApiClient(_serviceProvider, httpClientName, apiClientName);
            return apiClient;
        }
    }
}
#endif