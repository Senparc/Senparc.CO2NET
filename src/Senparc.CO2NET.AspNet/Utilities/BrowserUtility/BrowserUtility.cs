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
    Copyright (C) 2020 Senparc

    文件名：BrowserUtility.cs
    文件功能描述：浏览器公共类


    创建标识：Senparc - 20150419

    修改标识：Senparc - 20161219
    修改描述：v4.9.6 修改错别字：Browser->Browser

    修改标识：Senparc - 20161219
    修改描述：v4.11.2 修改SideInWeixinBrowser判断逻辑

    修改标识：Senparc - 20180513
    修改描述：v4.11.2 1、增加对小程序请求的判断方法 SideInWeixinMiniProgram()
                      2、添加 GetUserAgent() 方法

    ----  CO2NET   ----
    ----  split from Senparc.Weixin/Utilities/BrowserUtility.cs  ----

    修改标识：Senparc - 20180531
    修改描述：v0.1.0 移植 BrowserUtility

    -- 从 CO2NET 移植到 CO2NET.AspNet --
    
    修改标识：Senparc - 20180721
    修改描述：v0.1.0  从 CO2NET 移植到 CO2NET.AspNet

----------------------------------------------------------------*/

using System.Web;

#if !NET45
using Microsoft.AspNetCore.Http;
#endif


namespace Senparc.CO2NET.Utilities
{
    /// <summary>
    /// 浏览器公共类
    /// </summary>
    public class BrowserUtility
    {
        /// <summary>
        /// 获取 Headers 中的 User-Agent 字符串
        /// </summary>
        /// <param name="httpRequest"></param>
        /// <returns></returns>
#if NET40 || NET45
        public static string GetUserAgent(HttpRequestBase httpRequest)
#else
        public static string GetUserAgent(HttpRequest httpRequest)
#endif
        {
#if !NET45

            string userAgent = null;
            var userAgentHeader = httpRequest.Headers["User-Agent"];
            if (userAgentHeader.Count > 0)
            {
                userAgent = userAgentHeader[0];//.ToUpper();
            }
#else
            string userAgent = httpRequest.UserAgent != null
                                ? httpRequest.UserAgent//.ToUpper()
                                : null;
#endif
            return userAgent;
        }
    }
}
