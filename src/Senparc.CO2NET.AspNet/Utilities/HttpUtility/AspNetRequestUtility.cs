#if !NET45
using Microsoft.AspNetCore.Http;
#endif
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Senparc.CO2NET.AspNet.HttpUtility
{
    public static partial class RequestUtility
    {
#if !NET45
        /// <summary>
        /// 从 Request.Body 中读取流，并复制到一个独立的 MemoryStream 对象中
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static Stream GetRequestMemoryStream(this HttpRequest request)
        {
#if NETCOREAPP3_0
            var syncIOFeature = request.HttpContext.Features.Get<IHttpBodyControlFeature>();

            if (syncIOFeature != null)
            {
                syncIOFeature.AllowSynchronousIO = true;
            }
#endif
            string body = new StreamReader(request.Body).ReadToEnd();
            byte[] requestData = Encoding.UTF8.GetBytes(body);
            Stream inputStream = new MemoryStream(requestData);
            return inputStream;
        }
    }
#endif
}
