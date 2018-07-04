/*----------------------------------------------------------------
    Copyright (C) 2018 Senparc

    文件名：Register.cs
    文件功能描述：Senparc.Weixin 快捷注册流程（包括Thread、TraceLog等）


    创建标识：Senparc - 20180222

    修改标识：Senparc - 20180516
    修改描述：优化 RegisterService

    修改标识：Senparc - 20180704
    修改描述：v0.1.6.1 添加 Register.UseSenparcGlobal() 方法

----------------------------------------------------------------*/

using System;
using Senparc.CO2NET.Threads;
using Senparc.CO2NET.RegisterServices;

namespace Senparc.CO2NET
{
    /// <summary>
    /// Senparc.CO2NET 基础信息注册
    /// </summary>
    public static class Register
    {
        /// <summary>
        /// 修改默认缓存命名空间
        /// </summary>
        /// <param name="registerService">RegisterService</param>
        /// <param name="customNamespace">自定义命名空间名称</param>
        /// <returns></returns>
        public static IRegisterService ChangeDefaultCacheNamespace(this IRegisterService registerService, string customNamespace)
        {
            Config.DefaultCacheNamespace = customNamespace;
            return registerService;
        }


        /// <summary>
        /// 注册 Threads 的方法（如果不注册此线程，则AccessToken、JsTicket等都无法使用SDK自动储存和管理）
        /// </summary>
        /// <param name="registerService">RegisterService</param>
        /// <returns></returns>
        public static IRegisterService RegisterThreads(this IRegisterService registerService)
        {
            ThreadUtility.Register();//如果不注册此线程，则AccessToken、JsTicket等都无法使用SDK自动储存和管理。
            return registerService;
        }

        /// <summary>
        /// 注册 TraceLog 的方法
        /// </summary>
        /// <param name="registerService">RegisterService</param>
        /// <param name="action"></param>
        /// <returns></returns>
        public static IRegisterService RegisterTraceLog(this IRegisterService registerService, Action action)
        {
            action();
            return registerService;
        }

        /// <summary>
        /// 开始 Senparc.Weixin SDK 初始化参数流程
        /// </summary>
        /// <param name="registerService"></param>
        /// <param name="senparcSetting"></param>
        /// <returns></returns>
        public static IRegisterService UseSenparcGlobal(this IRegisterService registerService, SenparcSetting senparcSetting)
        {
            //Senparc.CO2NET 配置
            Senparc.CO2NET.Config.SenparcSetting = senparcSetting;
            return registerService;
        }
    }
}
