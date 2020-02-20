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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Senparc.CO2NET.Cache
{
    /// <summary>
    /// 缓存策略工厂。
    /// <para>缓存策略的注册（立即启用）和当前缓存策略获取</para>
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
        /// 注册当前全局环境下的缓存策略，并立即启用。
        /// </summary>
        /// <param name="func">如果为 null，将使用默认的本地缓存策略（LocalObjectCacheStrategy.Instance）</param>
        public static void RegisterObjectCacheStrategy(Func<IBaseObjectCacheStrategy> func)
        {
            ObjectCacheStrateFunc = func;

            if (func!=null)
            {
                ObjectCacheStrategy = func();//提前运行一次，否则第一次运行开销比较大（400毫秒以上）
            }
        }

        public static IBaseObjectCacheStrategy GetObjectCacheStrategyInstance(this IServiceProvider serviceProvider)
        {
            return GetObjectCacheStrategyInstance();
        }

        /// <summary>
        /// 获取全局缓存策略
        /// </summary>
        /// <returns></returns>
        public static IBaseObjectCacheStrategy GetObjectCacheStrategyInstance()
        {
            if (ObjectCacheStrateFunc == null)
            {
                //默认状态
                return LocalObjectCacheStrategy.Instance;
            }
            else
            {
                //自定义类型
                var instance = ObjectCacheStrateFunc();// ?? LocalObjectCacheStrategy.Instance;

                //if (instance == null)
                //{
                //    return LocalObjectCacheStrategy.Instance;//确保有值，防止委托内结果仍然为null
                //}
                return instance;
            }
        }

        /// <summary>
        /// 获取指定领域缓存的换存策略
        /// </summary>
        /// <param name="cacheStrategyDomain">领域缓存信息（需要为单例）CacheStrategyDomain</param>
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
        //        //默认状态
        //        return LocalContainerCacheStrategy.Instance;
        //    }
        //    else
        //    {
        //        //自定义类型
        //        var instance = ContainerCacheStrateFunc();
        //        return instance;
        //    }
        //}
    }
}
