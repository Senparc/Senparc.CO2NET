using Senparc.CO2NET.Extensions;
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
        public Func<IBaseObjectCacheStrategy> BaseObjectCacheStrategy { get { return DomainExtensionCacheStrategy.BaseCacheStrategy; } }


        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="domainExtensionCacheStrategy">扩展缓存策略（例如RedisContainerCacheStrategy）</param>
        public CacheStrategyDomainMappingItem(IDomainExtensionCacheStrategy domainExtensionCacheStrategy)
        {
            DomainExtensionCacheStrategy = domainExtensionCacheStrategy;
        }
    }



    /// <summary>
    /// 某一个领域内的缓存策略集合
    /// </summary>
    internal class CacheStrategyDomainMappingCollection : Dictionary<IBaseObjectCacheStrategy, CacheStrategyDomainMappingItem>
    {
        /// <summary>
        /// 添加或更新缓存策略映射
        /// </summary>
        /// <param name="domainCacheStrategy"></param>
        /// <param name="item"></param>
        public void AddOrUpdate(CacheStrategyDomainMappingItem item)
        {
            var cacheStrategy = item.BaseObjectCacheStrategy();
            base[cacheStrategy] = item;
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
        public static void RegisterCacheStrategyDomain(IDomainExtensionCacheStrategy domainCacheStrategy)
        {
            var identityName = domainCacheStrategy.CacheStrategyDomain.IdentityName;
            var cacheStrategy = domainCacheStrategy.BaseCacheStrategy();
            var mappingCollection = GetMappingCollection(identityName);
            var mappingItem = new CacheStrategyDomainMappingItem(domainCacheStrategy);
            mappingCollection[cacheStrategy] = mappingItem;
        }

        /// <summary>
        /// 获取领域缓存（指定特定 的IBaseObjectCacheStrategy 缓存策略对象）
        /// </summary>
        /// <param name="baseObjectCacheStrategy">IBaseObjectCacheStrategy 缓存策略对象</param>
        /// <param name="cacheStrategyDomain">缓存领域</param>
        /// <returns></returns>
        public static IDomainExtensionCacheStrategy GetDomainExtensionCacheStrategy(IBaseObjectCacheStrategy baseObjectCacheStrategy,
            ICacheStrategyDomain cacheStrategyDomain)
        {
            var identityName = cacheStrategyDomain.IdentityName;
            var mappingCollection = GetMappingCollection(identityName);//当前扩展缓存可能已经注册的所有基础缓存

            if (mappingCollection.ContainsKey(baseObjectCacheStrategy))
            {
                var item = mappingCollection[baseObjectCacheStrategy];
                return item.DomainExtensionCacheStrategy;
            }
            else
            {
                //未注册，默认情况下使用本地缓存策略（应急）
                var ex = new Exceptions.BaseException("当前扩展缓存策略没有进行注册，CacheStrategyDomain：{0}，IBaseObjectCacheStrategy：{1}".FormatWith(cacheStrategyDomain.GetType(), baseObjectCacheStrategy.GetType()));
                SenparcTrace.BaseExceptionLog(ex);
                throw ex;
            }
        }
    }
}
