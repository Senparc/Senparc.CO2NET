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

    FileName：SenparcHttpResponse.cs
    File Function Description：Unified encapsulation of HttpResonse requests, providing debugging, tracking, and other extended capabilities during the Http request process


    Creation Identifier：Senparc - 20171104

    Modification Description：Unified encapsulation of HttpResonse requests

    Modification Identifier：Senparc - 20190429
    Modification Description：v0.7.0 Optimized HttpClient, refactored RequestUtility (including Post and Get), introduced HttpClientFactory mechanism

----------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Senparc.CO2NET.Extensions;
#if NET462
using System.Web;
#else
using System.Net.Http;
using System.Net.Http.Headers;
#endif

namespace Senparc.CO2NET.HttpUtility
{
    /// <summary>
    /// Unified encapsulation of HttpResonse requests, providing debugging, tracking, and other extended capabilities during the Http request process
    /// </summary>
    public class SenparcHttpResponse
    {
#if NET462
        public HttpWebResponse Result { get; set; }

        public SenparcHttpResponse(HttpWebResponse httpWebResponse)
        {
            Result = httpWebResponse;
        }
#else
        public HttpResponseMessage Result { get; set; }

        public SenparcHttpResponse(HttpResponseMessage httpWebResponse)
        {
            Result = httpWebResponse;
        }
#endif

//        /// <summary>
//        /// Is an Ajax request
//        /// </summary>
//        public bool IsAjax
//        {
//            get
//            {
//#if NET462
//                var values = Result.Headers.GetValues("X-Requested-With");
//                return values != null ? values.FirstOrDefault().IsNullOrEmpty() : false;
//#else
//                return !Result.RequestMessage.Headers.GetValues("X-Requested-With").FirstOrDefault().IsNullOrEmpty();
//#endif
//            }
//        }
    }
}