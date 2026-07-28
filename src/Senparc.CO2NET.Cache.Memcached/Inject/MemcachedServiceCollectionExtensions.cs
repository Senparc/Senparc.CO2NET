/*----------------------------------------------------------------
    Copyright (C) 2026 Senparc

    FileName: MemcachedServiceCollectionExtensions.cs
    File Function Description: Memcached dependency injection setup.


    Creation Identifier: Senparc - 20180222

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
        /// Configure dependency injection under .NET Core
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