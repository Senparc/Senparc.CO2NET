/*----------------------------------------------------------------
    Copyright (C) 2026 Senparc

    FileName: MemcachedApplicationBuilderExtensions.cs
    File Function Description: Memcached dependency injection setup.


    Creation Identifier: Senparc - 20180222

----------------------------------------------------------------*/

#if !NET462
using Enyim.Caching;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.Builder
{
    public static class MemcachedApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseSenparcMemcached(this IApplicationBuilder app)
        {
            app.UseEnyimMemcached();
            return app;
        }
    }
}
#endif