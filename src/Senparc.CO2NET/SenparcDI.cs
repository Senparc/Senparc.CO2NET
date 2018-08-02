
/*----------------------------------------------------------------
    Copyright (C) 2018 Senparc

    文件名：SenparcDI.cs
    文件功能描述：针对 .NET Core 的依赖注入扩展类

    创建标识：Senparc - 20180714


    修改标识：Senparc - 20180802
    修改描述：v3.1.0 提供当前类中的方法对 netstandard2.0 的完整支持

----------------------------------------------------------------*/

#if NETSTANDARD2_0 || NETCOREAPP2_0 || NETCOREAPP2_1
using Microsoft.Extensions.DependencyInjection;

namespace Senparc.CO2NET
{
    /// <summary>
    /// 针对 .NET Core 的依赖注入扩展类
    /// </summary>
    public static class SenparcDI
    {
        /// <summary>
        /// 全局 ServiceCollection
        /// </summary>
        public static IServiceCollection GlobalServiceCollection { get; set; }

        /// <summary>
        /// 创建一个新的 ServiceCollection 对象
        /// </summary>
        /// <returns></returns>
        public static IServiceCollection GetServiceCollection()
        {
            return GlobalServiceCollection;
        }

        public static ServiceProvider GlobalServiceProvider { get; set; }

        /// <summary>
        /// 获取 ServiceProvider
        /// </summary>
        /// <returns></returns>
        public static ServiceProvider GetServiceProvider()
        {
            if (GlobalServiceProvider == null)
            {
                //注意：BuildServiceProvider() 方法每次会生成不同的 ServiceProvider 对象！
                GlobalServiceProvider = GetServiceCollection().BuildServiceProvider();
            }
            return GlobalServiceProvider;
        }

        /// <summary>
        /// 使用 .net core 默认的 DI 方法获得实例
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T GetService<T>()
        {
            return GetServiceProvider().GetService<T>();
        }

    }
}
#endif
