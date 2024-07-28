/*----------------------------------------------------------------
    Copyright (C) 2024 Senparc

    文件名：RegisterService.cs
    文件功能描述： Senparc.CO2NET 快捷注册流程


    创建标识：Senparc - 20191230

    修改标识：Senparc - 20221219
    修改描述：v1.1.3 优化 RegisterService.StartStart() 方法，根据 env 
                     参数自动判断是否为网站项目，并获取运行根目录

    修改标识：Senparc - 20240728
    修改描述：v1.4.0 .NET 6.0 和 .NET 8.0 程序集不再依赖 Microsoft.AspNetCore.Hosting.Abstractions 和 Microsoft.AspNetCore.Http.Abstractions

----------------------------------------------------------------*/


#if !NET462

using Microsoft.AspNetCore.Hosting;
using Senparc.CO2NET.RegisterServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Senparc.CO2NET.AspNet.RegisterServices
{
    /// <summary>
    /// Senparc.CO2NET 快捷注册流程
    /// </summary>
    public static class RegisterService
    {
        /// <summary>
        /// 开始 Senparc.CO2NET SDK 初始化参数流程（.NET Core），支持 ASP.NET Core
        /// </summary>
        /// <param name="env">IHostingEnvironment，控制台程序可以输入null，</param>
        /// <param name="senparcSetting"></param>
        /// <returns></returns>
        public static Senparc.CO2NET.RegisterServices.RegisterService Start(
            Microsoft.Extensions.Hosting.IHostEnvironment/*IHostingEnvironment*/ env,
            SenparcSetting senparcSetting)
        {
            //提供网站根目录
            if (env != null && env.ContentRootPath != null)
            {
#if NETSTANDARD2_0 || NETSTANDARD2_1
                if (env is Microsoft.AspNetCore.Hosting.IHostingEnvironment webHostingEnv)
#else
                if (env is IWebHostEnvironment webHostingEnv)
#endif
                {
                    Senparc.CO2NET.Config.RootDirectoryPath = webHostingEnv.ContentRootPath;
                }
                else
                {
                    Senparc.CO2NET.Config.RootDirectoryPath = env.ContentRootPath;
                }
            }
            else
            {
                Senparc.CO2NET.Config.RootDirectoryPath = AppDomain.CurrentDomain.BaseDirectory;
            }

            Senparc.CO2NET.AspNetConfig.HostEnvironment = env;


            var register = Senparc.CO2NET.RegisterServices.RegisterService.Start(senparcSetting);
            return register;
        }
    }
}
#endif
