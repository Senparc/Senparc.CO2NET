﻿/*----------------------------------------------------------------
    Copyright (C) 2025 Senparc

    文件名：MemcachedServiceCollectionExtensions.cs
    文件功能描述：Memcached 依赖注入设置。


    创建标识：Senparc - 20180222

----------------------------------------------------------------*/


#if !NET462
using Enyim.Caching.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Senparc.CO2NET.Cache.Memcached;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class MemcachedServiceCollectionExtensions
    {
        /// <summary>
        /// .NET Core下设置依赖注入
        /// </summary>
        /// <param name="services"></param>
        /// <param name="setupAction"></param>
        /// <returns></returns>
        public static IServiceCollection AddSenparcMemcached(this IServiceCollection services, Action<MemcachedClientOptions> setupAction)
        {
            services.AddSingleton<MemcachedObjectCacheStrategy, MemcachedObjectCacheStrategy>();
            //services.AddSingleton<MemcachedContainerStrategy, MemcachedContainerStrategy>();
            return services.AddEnyimMemcached(setupAction);
        }
    }
}

#endif