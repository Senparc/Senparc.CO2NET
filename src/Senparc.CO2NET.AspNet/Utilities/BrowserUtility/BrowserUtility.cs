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
    Copyright (C) 2025 Senparc

    FileName：BrowserUtility.cs
    File Function Description：Browser utility class


    Creation Identifier：Senparc - 20150419

    Modification Identifier：Senparc - 20161219
    Modification Description：v4.9.6 Corrected typo: Browser->Browser

    Modification Identifier：Senparc - 20161219
    Modification Description：v4.11.2 Modified SideInWeixinBrowser logic

    Modification Identifier：Senparc - 20180513
    Modification Description：v4.11.2 1. Added method to determine mini program requests SideInWeixinMiniProgram()
                      2. Added GetUserAgent() method

    ----  CO2NET   ----
    ----  split from Senparc.Weixin/Utilities/BrowserUtility.cs  ----

    Modification Identifier：Senparc - 20180531
    Modification Description：v0.1.0 Ported BrowserUtility

    -- Ported from CO2NET to CO2NET.AspNet --
    
    Modification Identifier：Senparc - 20180721
    Modification Description：v0.1.0  Ported from CO2NET to CO2NET.AspNet

----------------------------------------------------------------*/

using System.Web;

#if !NET462
using Microsoft.AspNetCore.Http;
#endif


namespace Senparc.CO2NET.Utilities
{
    /// <summary>
    /// Browser utility class
    /// </summary>
    public class BrowserUtility
    {
        /// <summary>
        /// Get the User-Agent string from Headers
        /// </summary>
        /// <param name="httpRequest"></param>
        /// <returns></returns>
#if NET462
        public static string GetUserAgent(HttpRequestBase httpRequest)
#else
        public static string GetUserAgent(HttpRequest httpRequest)
#endif
        {
#if !NET462

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
