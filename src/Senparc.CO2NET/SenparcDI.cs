
#if NETCOREAPP2_0 || NETCOREAPP2_1
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Senparc.CO2NET
{
    /// <summary>
    /// 针对 .NET Core 的依赖注入扩赞类
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

        /// <summary>
        /// 获取 ServiceProvider
        /// </summary>
        /// <returns></returns>
        public static ServiceProvider GetServiceProvider()
        {
            return GetServiceCollection().BuildServiceProvider();
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
