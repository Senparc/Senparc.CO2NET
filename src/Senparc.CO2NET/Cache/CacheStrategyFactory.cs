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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Senparc.CO2NET.Cache
{
    /// <summary>
    /// Cache strategy factory.
    /// <para>Registration of cache strategy (immediate activation) and retrieval of current cache strategy</para>
    /// </summary>
    public static class CacheStrategyFactory
    {
        internal static Func<IBaseObjectCacheStrategy> ObjectCacheStrateFunc { get; set; }
        internal static IBaseObjectCacheStrategy ObjectCacheStrategy { get; set; }
        //internal static IBaseCacheStrategy<TKey, TValue> GetContainerCacheStrategy<TKey, TValue>()
        //    where TKey : class
        //    where TValue : class
        //{
        //    return;
        //}

        /// <summary>
        /// Register the cache strategy for the current global environment and activate it immediately.
        /// </summary>
        /// <param name="func">If null, the default local cache strategy (LocalObjectCacheStrategy.Instance) will be used</param>
        public static void RegisterObjectCacheStrategy(Func<IBaseObjectCacheStrategy> func)
        {
            ObjectCacheStrateFunc = func;

            if (func!=null)
            {
                ObjectCacheStrategy = func();//Run once in advance, otherwise the first run will have a large overhead (more than 400 milliseconds)
            }
        }

        /// <summary>
        /// Get the global cache strategy
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <returns></returns>
        public static IBaseObjectCacheStrategy GetObjectCacheStrategyInstance(this IServiceProvider serviceProvider)
        {
            return GetObjectCacheStrategyInstance();
        }

        /// <summary>
        /// Get the global cache strategy
        /// </summary>
        /// <returns></returns>
        public static IBaseObjectCacheStrategy GetObjectCacheStrategyInstance()
        {
            if (ObjectCacheStrateFunc == null)
            {
                //Default state
                return LocalObjectCacheStrategy.Instance;
            }
            else
            {
                //Custom type
                var instance = ObjectCacheStrateFunc();// ?? LocalObjectCacheStrategy.Instance;

                //if (instance == null)
                //{
                //    return LocalObjectCacheStrategy.Instance;//Ensure there is a value to prevent the delegate result from still being null
                //}
                return instance;
            }
        }

        /// <summary>
        /// Get the cache strategy for the specified domain cache
        /// </summary>
        /// <param name="cacheStrategyDomain">Domain cache information (must be singleton) CacheStrategyDomain</param>
        /// <returns></returns>
        public static IDomainExtensionCacheStrategy GetExtensionCacheStrategyInstance(ICacheStrategyDomain cacheStrategyDomain)
        {
            var currentObjectCacheStrategy = GetObjectCacheStrategyInstance();
            var domianExtensionCacheStrategy = CacheStrategyDomainWarehouse.GetDomainExtensionCacheStrategy(currentObjectCacheStrategy, cacheStrategyDomain);
            return domianExtensionCacheStrategy;
        }

        //public static void RegisterContainerCacheStrategy(Func<IContainerCacheStrategy> func)
        //{
        //    ContainerCacheStrateFunc = func;
        //}

        //public static IContainerCacheStrategy GetContainerCacheStrategyInstance()
        //{
        //    if (ContainerCacheStrateFunc == null)
        //    {
        //        //Default state
        //        return LocalContainerCacheStrategy.Instance;
        //    }
        //    else
        //    {
        //        //Custom type
        //        var instance = ContainerCacheStrateFunc();
        //        return instance;
        //    }
        //}
    }
}
