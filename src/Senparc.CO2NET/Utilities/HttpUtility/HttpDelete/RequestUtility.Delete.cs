#region Apache License Version 2.0
/*----------------------------------------------------------------

Copyright 2023 Suzhou Senparc Network Technology Co.,Ltd.

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

    文件名：RequestUtility.Delete.cs
    文件功能描述：获取请求结果（Delete）


    创建标识：Senparc - 20230625

    修改标识：Senparc - 20230711
    修改描述：v2.2.1 优化 Http 请求，及时关闭资源

----------------------------------------------------------------*/

using System;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Specialized;
#if NET462
using System.Web;
#else
using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.Extensions.DependencyInjection;
#endif
#if !NET462
using Senparc.CO2NET.WebProxy;
#endif

namespace Senparc.CO2NET.HttpUtility
{
    public partial class RequestUtility
    {
        /// <summary>
        /// 使用 Delete 方法获取请求结果（没有加入Cookie）
        /// </summary>
        /// <param name="serviceProvider">.NetCore 下的服务器提供程序，如果 .NET Framework 则保留 null</param>
        /// <param name="url"></param>
        /// <returns></returns>
        public static async Task<string> HttpDeleteAsync(
            IServiceProvider serviceProvider,
            string url, Encoding encoding = null)
        {
#if NET462
            using (var wc = new WebClient())
            {
                wc.Proxy = _webproxy;
                wc.Encoding = encoding ?? Encoding.UTF8;
                var result =  wc.UploadValues(url, "DELETE", new NameValueCollection());
                var response = Encoding.ASCII.GetString(result);
                return response;
            }
#else
            var handler = new HttpClientHandler
            {
                UseProxy = SenparcHttpClientWebProxy != null,
                Proxy = SenparcHttpClientWebProxy,
            };

            HttpClient httpClient = serviceProvider.GetRequiredService<SenparcHttpClient>().Client;

            using (httpClient)
            {
                var response = await httpClient.DeleteAsync(url).ConfigureAwait(false);
                using (response)
                {
                    return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                }
            }
#endif

        }
    }
}
