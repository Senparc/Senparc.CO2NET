#if !NET45
using Microsoft.AspNetCore.Hosting;
using Senparc.CO2NET.RegisterServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Senparc.CO2NET.AspNet.RegisterServices
{
    public static class RegisterService
    {
        /// <summary>
        /// 开始 Senparc.CO2NET SDK 初始化参数流程（.NET Core），支持 ASP.NET Core
        /// </summary>
        /// <param name="env">IHostingEnvironment，控制台程序可以输入null，</param>
        /// <param name="senparcSetting"></param>
        /// <returns></returns>
        public static Senparc.CO2NET.RegisterServices.RegisterService Start(
#if NETSTANDARD2_0
            Microsoft.Extensions.Hosting.IHostEnvironment/*IHostingEnvironment*/ env, 
#else
            Microsoft.Extensions.Hosting.IHostEnvironment/*IWebHostEnvironment*/ env,
#endif
            SenparcSetting senparcSetting)
        {
            //提供网站根目录
            if (env != null && env.ContentRootPath != null)
            {
                Senparc.CO2NET.Config.RootDictionaryPath = env.ContentRootPath;
            }
            else
            {
                Senparc.CO2NET.Config.RootDictionaryPath = AppDomain.CurrentDomain.BaseDirectory;
            }

            Senparc.CO2NET.AspNetConfig.HostingEnvironment = env;

           
            var register = Senparc.CO2NET.RegisterServices.RegisterService.Start(senparcSetting);
            return register;
        }
    }
}
#endif
