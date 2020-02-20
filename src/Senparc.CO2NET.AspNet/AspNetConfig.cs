#if !NET45
using Microsoft.AspNetCore.Hosting;
#endif 
using System;
using System.Collections.Generic;
using System.Text;

namespace Senparc.CO2NET
{
    public static class AspNetConfig
    {

#if NETSTANDARD2_0
        /// <summary>
        /// Web hosting environment
        /// </summary>
        public static Microsoft.Extensions.Hosting.IHostEnvironment HostingEnvironment { get; set; }
#elif NETSTANDARD2_1
        /// <summary>
        /// Web hosting environment
        /// </summary>
        public static Microsoft.Extensions.Hosting.IHostEnvironment/*IWebHostEnvironment*/ HostingEnvironment { get; set; }
#endif
    }
}
