/*----------------------------------------------------------------
    Copyright (C) 2024 Senparc

    FileName：RegisterService.cs
    File Function Description： Senparc.CO2NET Quick Registration Process


    Creation Identifier：Senparc - 20191230

    Modification Identifier：Senparc - 20221219
    Modification Description：v1.1.3 Optimized RegisterService.StartStart() method, automatically determine whether it is a website project based on the env parameter and get the running root directory

    Modification Identifier：Senparc - 20240728
    Modification Description：v1.4.0 .NET 6.0 and .NET 8.0 assemblies no longer depend on Microsoft.AspNetCore.Hosting.Abstractions and Microsoft.AspNetCore.Http.Abstractions

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
    /// Senparc.CO2NET Quick Registration Process
    /// </summary>
    public static class RegisterService
    {
        /// <summary>
        /// Start Senparc.CO2NET SDK initialization parameter process (.NET Core), supports ASP.NET Core
        /// </summary>
        /// <param name="env">IHostingEnvironment, console programs can input null,</param>
        /// <returns></returns>
        public static Senparc.CO2NET.RegisterServices.RegisterService Start(
            Microsoft.Extensions.Hosting.IHostEnvironment/*IHostingEnvironment*/ env
            , SenparcSetting senparcSetting = null)
        {
            //Provide website root directory
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
