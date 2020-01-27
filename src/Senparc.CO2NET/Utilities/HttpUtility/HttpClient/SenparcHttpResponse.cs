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

    文件名：SenparcHttpResponse.cs
    文件功能描述：统一封装HttpResonse请求，提供Http请求过程中的调试、跟踪等扩展能力


    创建标识：Senparc - 20171104

    修改描述：统一封装HttpResonse请求

    修改标识：Senparc - 20190429
    修改描述：v0.7.0 优化 HttpClient，重构 RequestUtility（包括 Post 和 Get），引入 HttpClientFactory 机制

----------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Senparc.CO2NET.Extensions;
#if NET45
using System.Web;
#else
using System.Net.Http;
using System.Net.Http.Headers;
#endif

namespace Senparc.CO2NET.HttpUtility
{
    /// <summary>
    /// 统一封装HttpResonse请求，提供Http请求过程中的调试、跟踪等扩展能力
    /// </summary>
    public class SenparcHttpResponse
    {
#if NET45
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
//        /// 是Ajax请求
//        /// </summary>
//        public bool IsAjax
//        {
//            get
//            {
//#if NET45
//                var values = Result.Headers.GetValues("X-Requested-With");
//                return values != null ? values.FirstOrDefault().IsNullOrEmpty() : false;
//#else
//                return !Result.RequestMessage.Headers.GetValues("X-Requested-With").FirstOrDefault().IsNullOrEmpty();
//#endif
//            }
//        }
    }
}