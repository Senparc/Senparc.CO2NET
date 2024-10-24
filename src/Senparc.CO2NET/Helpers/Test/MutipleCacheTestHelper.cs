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
    
    FileName：MutipleCacheTestHelper.cs
    File Function Description：Multiple cache test helper class
    
    
    Creation Identifier：Senparc - 20170702

----------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Senparc.CO2NET.Helpers;
using Senparc.CO2NET.Cache;

namespace Senparc.CO2NET.Helpers
{
    /// <summary>
    /// Multiple cache test helper class
    /// </summary>
    public class MutipleCacheTestHelper
    {
        /// <summary>
        /// Test multiple caches
        /// </summary>
        public static void RunMutipleCache(Action action)
        {
            RunMutipleCache(action, CacheType.Local);
        }

        /// <summary>
        /// Iterate using multiple caches to test the same process (delegate) to ensure consistent behavior across different caching strategies
        /// </summary>
        public static void RunMutipleCache(Action action, params CacheType[] cacheTypes)
        {
            List<IBaseObjectCacheStrategy> cacheStrategies = new List<IBaseObjectCacheStrategy>();

            foreach (var cacheType in cacheTypes)
            {
                var assabmleName = cacheType == CacheType.Local
                    ? "Senparc.CO2NET"
                    : "Senparc.CO2NET.Cache." + cacheType.ToString();

                var nameSpace = cacheType == CacheType.Local
                                    ? "Senparc.CO2NET.Cache"
                                    : "Senparc.CO2NET.Cache." + cacheType.ToString();

                var className = cacheType.ToString() + "ObjectCacheStrategy";


                var cacheInstance = ReflectionHelper.GetStaticMember(assabmleName, nameSpace,
                    className, "Instance"/*Get singleton property*/) as IBaseObjectCacheStrategy;

                cacheStrategies.Add(cacheInstance);

                //switch (cacheType)
                //{
                //    case CacheType.Local:
                //        cacheStrategies.Add(LocalObjectCacheStrategy.Instance);
                //        break;
                //    case CacheType.Redis:
                //        cacheStrategies.Add(RedisObjectCacheStrategy.Instance);
                //        break;
                //    case CacheType.Memcached:
                //        cacheStrategies.Add(MemcachedObjectCacheStrategy.Instance);
                //        break;
                //}
            }

            foreach (var objectCacheStrategy in cacheStrategies)
            {
                //Original cache strategy
                var originalCache = CacheStrategyFactory.GetObjectCacheStrategyInstance();

                Console.WriteLine("== 使用缓存策略：" + objectCacheStrategy.GetType().Name + " 开始 == ");

                //Use current cache strategy
                CacheStrategyFactory.RegisterObjectCacheStrategy(() => objectCacheStrategy);

                try
                {
                    action();//Execute
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }

                Console.WriteLine("== 使用缓存策略：" + objectCacheStrategy.GetType().Name + " 结束 == \r\n");

                //Restore cache strategy
                CacheStrategyFactory.RegisterObjectCacheStrategy(() => originalCache);
            }
        }
    }
}
