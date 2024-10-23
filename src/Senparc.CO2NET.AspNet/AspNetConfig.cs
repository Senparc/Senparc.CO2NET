#if !NET462
using Microsoft.AspNetCore.Hosting;
#endif 
using System;
using System.Collections.Generic;
using System.Text;

namespace Senparc.CO2NET
{
    public static class AspNetConfig
    {

#if !NET462
        /// <summary>
        /// Web hosting environment
        /// </summary>
        [Obsolete("请使用 HostEnvironment", true)]
        public static Microsoft.Extensions.Hosting.IHostEnvironment HostingEnvironment { get => HostEnvironment; set => HostEnvironment = value; }


        public static Microsoft.Extensions.Hosting.IHostEnvironment HostEnvironment { get; set; }
        //#elseif NETSTANDARD2_1
        //        /// <summary>
        //        /// Web hosting environment
        //        /// </summary>
        //        public static Microsoft.Extensions.Hosting.IHostEnvironment/*IWebHostEnvironment*/ HostingEnvironment { get; set; }
#endif
    }
}
