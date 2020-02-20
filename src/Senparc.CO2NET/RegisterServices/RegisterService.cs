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

    文件名：RegisterService.cs
    文件功能描述：Senparc.CO2NET SDK 快捷注册流程


    创建标识：Senparc - 20180222

    修改标识：Senparc - 20180517
    修改描述：完善 .net core 注册流程

    修改标识：Senparc - 20180517
    修改描述：v0.1.1 修复 RegisterService.Start() 的 isDebug 设置始终为 true 的问题

    修改标识：Senparc - 20180517
    修改描述：v0.1.9 1、RegisterService 取消 public 的构造函数，统一使用 RegisterService.Start() 初始化
                     2、.net framework 和 .net core 版本统一强制在构造函数中要求提供 SenparcSetting 参数
  
    修改标识：Senparc - 20190108
    修改描述：v0.5.0 添加 Start() 重写方法，提供 .NET Core Console 的全面支持

    修改标识：Senparc - 20180911
    修改描述：v0.8.10 RegisterService.Start() 方法开始记录 evn 参数到 Config.HostingEnvironment 属性 
   
----------------------------------------------------------------*/


#if !NET45
using Microsoft.Extensions.DependencyInjection;
#endif

using System;

namespace Senparc.CO2NET.RegisterServices
{
    /// <summary>
    /// 快捷注册接口
    /// </summary>
    public interface IRegisterService
    {

    }

    /// <summary>
    /// 快捷注册类，IRegisterService的默认实现
    /// </summary>
    public class RegisterService : IRegisterService
    {
        public static RegisterService Object { get; internal set; }

        private RegisterService() : this(null) { }

        private RegisterService(SenparcSetting senparcSetting)
        {
            //Senparc.CO2NET SDK 配置
            Senparc.CO2NET.Config.SenparcSetting = senparcSetting ?? new SenparcSetting();
        }

#if !NET45

        /// <summary>
        /// 单个实例引用全局的 ServiceCollection
        /// </summary>
        public IServiceCollection ServiceCollection => SenparcDI.GlobalServiceCollection;

        /// <summary>
        /// 开始 Senparc.CO2NET SDK 初始化参数流程（.NET Core）
        /// </summary>
        /// <param name="senparcSetting"></param>
        /// <returns></returns>
        public static RegisterService Start(SenparcSetting senparcSetting)
        {
            var register = new RegisterService(senparcSetting);

            //如果不注册此线程，则AccessToken、JsTicket等都无法使用SDK自动储存和管理。
            register.RegisterThreads();//默认把线程注册好

            return register;
        }

#else
        /// <summary>
        /// 开始 Senparc.CO2NET SDK 初始化参数流程
        /// </summary>
        /// <returns></returns>
        public static RegisterService Start(SenparcSetting senparcSetting)
        {
            var register = new RegisterService(senparcSetting);

            //提供网站根目录
            Senparc.CO2NET.Config.RootDictionaryPath = AppDomain.CurrentDomain.BaseDirectory;

            //如果不注册此线程，则AccessToken、JsTicket等都无法使用SDK自动储存和管理。
            register.RegisterThreads();//默认把线程注册好
            
            return register;
        }
#endif
    }
}
