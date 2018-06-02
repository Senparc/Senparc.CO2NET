using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Senparc.CO2NET.Cache
{
    public static class CacheLockWrapperExtension
    {
        public static CacheLockWrapper InstanceCacheLockWrapper(this IBaseCacheStrategy stragegy, string resourceName, string key, int retryCount, TimeSpan retryDelay)
        {
            return new CacheLockWrapper(stragegy, resourceName, key, retryCount, retryDelay);
        }

        public static CacheLockWrapper InstanceCacheLockWrapper(this IBaseCacheStrategy stragegy, string resourceName, string key)
        {
            return InstanceCacheLockWrapper(stragegy, resourceName, key, 0, new TimeSpan());
        }
    }
}
