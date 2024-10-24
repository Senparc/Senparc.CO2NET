#region Apache License Version 2.0
/*----------------------------------------------------------------

Copyright 2024 Suzhou Senparc Network Technology Co.,Ltd.

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
    Copyright (C) 2024 Senparc

    FileName：RegisterService.cs
    File Function Description：Senparc.CO2NET quick registration process


    Creation Identifier：Senparc - 20180222

    Modification Identifier：Senparc - 20180517
    Modification Description：Improve .net core registration process

    Modification Identifier：Senparc - 20180517
    Modification Description：v0.1.1 Fix the issue where isDebug in RegisterService.Start() is always set to true

    Modification Identifier：Senparc - 20180517
    Modification Description：v0.1.9 1. Remove public constructor from RegisterService, use RegisterService.Start() for initialization
                     2. Unified requirement for SenparcSetting parameter in constructors for both .net framework and .net core versions
  
    Modification Identifier：Senparc - 20190108
    Modification Description：v0.5.0 Add Start() override method to provide full support for .NET Core Console

    Modification Identifier：Senparc - 20180911
    Modification Description：v0.8.10 RegisterService.Start() method starts recording evn parameter to Config.HostingEnvironment property 
   
----------------------------------------------------------------*/


#if !NET462
using Microsoft.Extensions.DependencyInjection;
#endif

using System;

namespace Senparc.CO2NET.RegisterServices
{
    /// <summary>
    /// Quick registration interface
    /// </summary>
    public interface IRegisterService
    {

    }

    /// <summary>
    /// Quick registration class, default implementation of IRegisterService
    /// </summary>
    public class RegisterService : IRegisterService
    {
        public static RegisterService Object { get; internal set; }

        private RegisterService() : this(null) { }

        private RegisterService(SenparcSetting senparcSetting)
        {
            //Senparc.CO2NET SDK configuration
            Senparc.CO2NET.Config.SenparcSetting = senparcSetting ?? new SenparcSetting();
        }

#if !NET462

        /// <summary>
        /// Single instance referencing global ServiceCollection
        /// </summary>
        public IServiceCollection ServiceCollection => SenparcDI.GlobalServiceCollection;

        /// <summary>
        /// Start Senparc.CO2NET SDK initialization parameter process (.NET Core)
        /// </summary>
        /// <param name="senparcSetting"></param>
        /// <returns></returns>
        public static RegisterService Start(SenparcSetting senparcSetting)
        {
            var register = new RegisterService(senparcSetting);

            //If this thread is not registered, AccessToken, JsTicket, etc. cannot use the SDK for automatic storage and management.
            register.RegisterThreads();//Register the thread by default

            return register;
        }

#else
        /// <summary>
        /// Start Senparc.CO2NET SDK initialization parameter process
        /// </summary>
        /// <returns></returns>
        public static RegisterService Start(SenparcSetting senparcSetting)
        {
            var register = new RegisterService(senparcSetting);

            //Provide website root directory
            Senparc.CO2NET.Config.RootDirectoryPath = AppDomain.CurrentDomain.BaseDirectory;

            //If this thread is not registered, AccessToken, JsTicket, etc. cannot use the SDK for automatic storage and management.
            register.RegisterThreads();//Register the thread by default
            
            return register;
        }
#endif
    }
}
