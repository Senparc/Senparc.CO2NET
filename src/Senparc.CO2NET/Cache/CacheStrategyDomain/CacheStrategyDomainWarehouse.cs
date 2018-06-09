using Senparc.CO2NET.Trace;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Senparc.CO2NET.Cache
{
    /// <summary>
    /// 领域内不同类型缓存 -> 底层（CO2NET 级别） 的映射关系
    /// </summary>
    internal class CacheStrategyDomainMappingItem
    {
        /// <summary>
        /// 扩展缓存策略（例如RedisContainerCacheStrategy）
        /// </summary>
        public IDomainExtensionCacheStrategy DomainExtensionCacheStrategy { get; set; }

        /// <summary>
        /// 扩展缓存策略所使用的底层缓存策略（如RedisCacheStrategy）
        /// </summary>
        public Func<IBaseObjectCacheStrategy> BaseObjectCacheStrategy { get { return DomainExtensionCacheStrategy.CacheStragety; } }


        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="domainExtensionCacheStrategy">扩展缓存策略（例如RedisContainerCacheStrategy）</param>
        /// <param name="baseObjectCacheStrategy">扩展缓存策略所使用的底层缓存策略（如RedisCacheStrategy）</param>
        public CacheStrategyDomainMappingItem(IDomainExtensionCacheStrategy domainExtensionCacheStrategy)
        {
            DomainExtensionCacheStrategy = domainExtensionCacheStrategy;
        }
    }



    /// <summary>
    /// 某一个领域内的缓存策略集合
    /// </summary>
    internal class CacheStrategyDomainMappingCollection : Dictionary<IDomainExtensionCacheStrategy, CacheStrategyDomainMappingItem>
    {
        /// <summary>
        /// 添加或更新缓存策略映射
        /// </summary>
        /// <param name="domainCacheStrategy"></param>
        /// <param name="item"></param>
        public void AddOrUpdate(IDomainExtensionCacheStrategy domainCacheStrategy,
            CacheStrategyDomainMappingItem item)
        {
            var cacheStrategy = item.BaseObjectCacheStrategy();
            base[domainCacheStrategy] = item;
        }
    }


    /// <summary>
    /// 
    /// </summary>
    public class CacheStrategyDomainWarehouse
    {
        private static Dictionary<string, CacheStrategyDomainMappingCollection> _extensionCacheStrategyInstance
            = new Dictionary<string, CacheStrategyDomainMappingCollection>();


        /// <summary>
        /// 获取某个领域内的所有CacheStrategyDomainMappingCollection
        /// </summary>
        /// <param name="identityName"></param>
        /// <returns></returns>
        private static CacheStrategyDomainMappingCollection GetMappingCollection(string identityName)
        {
            if (!_extensionCacheStrategyInstance.ContainsKey(identityName))
            {
                _extensionCacheStrategyInstance[identityName] = new CacheStrategyDomainMappingCollection();
            }

            return _extensionCacheStrategyInstance[identityName];
        }


        static CacheStrategyDomainWarehouse()
        {

        }

        /// <summary>
        /// 注册领域缓存
        /// </summary>
        /// <param name="domainCacheStrategy"></param>
        public static void RegisterCacheStragetyDomain(IDomainExtensionCacheStrategy domainCacheStrategy)
        {
            var identityName = domainCacheStrategy.CacheStrategyDomain.IdentityName;
            var mappingCollection = GetMappingCollection(identityName);
            var mappingItem = new CacheStrategyDomainMappingItem(domainCacheStrategy);
        }

        public static IDomainExtensionCacheStrategy GetDomainExtensionCacheStrategy(IDomainExtensionCacheStrategy domainCacheStrategy)
        {
            var identityName = domainCacheStrategy.CacheStrategyDomain.IdentityName;
            var mappingCollection = GetMappingCollection(identityName);
            if (mappingCollection.ContainsKey(domainCacheStrategy))
            {
                var item = mappingCollection[domainCacheStrategy];
            }
            else
            {
                //未注册，默认情况下使用本地缓存策略（应急）
                SenparcTrace.BaseExceptionLog(new Exceptions.BaseException("当前扩展缓存策略没有进行注册：" + domainCacheStrategy.GetType()));

            }
        }



    }
}
