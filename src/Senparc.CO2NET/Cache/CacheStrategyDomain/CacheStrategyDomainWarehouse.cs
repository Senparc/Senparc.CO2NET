/*----------------------------------------------------------------
    Copyright (C) 2024 Senparc

    FileName：CacheStrategyDomainWarehouse.cs
    File Function Description： Domain Cache Warehouse


    Creation Identifier：Senparc - 20180609

    Modification Identifier：Senparc - 20180707
    Modification Description：Added AutoScanDomainCacheStrategy() and ClearRegisteredDomainExtensionCacheStrategies() methods

    Modification Identifier：Senparc - 20221219
    Modification Description：v2.1.4 _extensionCacheStrategyInstance parameter changed to ConcurrentDictionary type

----------------------------------------------------------------*/

using Senparc.CO2NET.Extensions;
using Senparc.CO2NET.Helpers;
using Senparc.CO2NET.Trace;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Senparc.CO2NET.Cache
{
    /// <summary>
    /// Mapping relationship between different types of caches in the domain -> underlying (CO2NET level)
    /// </summary>
    internal class CacheStrategyDomainMappingItem
    {
        /// <summary>
        /// Extension cache strategy (e.g., RedisContainerCacheStrategy)
        /// </summary>
        public IDomainExtensionCacheStrategy DomainExtensionCacheStrategy { get; set; }

        /// <summary>
        /// Underlying cache strategy used by the extension cache strategy (e.g., RedisCacheStrategy)
        /// </summary>
        public Func<IBaseObjectCacheStrategy> BaseObjectCacheStrategy { get { return DomainExtensionCacheStrategy.BaseCacheStrategy; } }


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="domainExtensionCacheStrategy">Extension cache strategy (e.g., RedisContainerCacheStrategy)</param>
        public CacheStrategyDomainMappingItem(IDomainExtensionCacheStrategy domainExtensionCacheStrategy)
        {
            DomainExtensionCacheStrategy = domainExtensionCacheStrategy;
        }
    }

    /// <summary>
    /// Cache strategy collection within a domain
    /// </summary>
    internal class CacheStrategyDomainMappingCollection : Dictionary<IBaseObjectCacheStrategy, CacheStrategyDomainMappingItem>
    {
        /// <summary>
        /// Add or update cache strategy mapping
        /// </summary>
        /// <param name="item"></param>
        public void AddOrUpdate(CacheStrategyDomainMappingItem item)
        {
            var cacheStrategy = item.BaseObjectCacheStrategy();
            base[cacheStrategy] = item;
        }
    }

    /// <summary>
    /// Domain Cache Warehouse
    /// </summary>
    public class CacheStrategyDomainWarehouse
    {
        private static ConcurrentDictionary<string, CacheStrategyDomainMappingCollection> _extensionCacheStrategyInstance
            = new ConcurrentDictionary<string, CacheStrategyDomainMappingCollection>();


        /// <summary>
        /// Get all CacheStrategyDomainMappingCollection within a domain
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
        /// Register domain cache
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
        /// Get domain cache (specify a specific IBaseObjectCacheStrategy cache strategy object)
        /// </summary>
        /// <param name="baseObjectCacheStrategy">IBaseObjectCacheStrategy cache strategy object</param>
        /// <param name="cacheStrategyDomain">Cache domain</param>
        /// <returns></returns>
        public static IDomainExtensionCacheStrategy GetDomainExtensionCacheStrategy(IBaseObjectCacheStrategy baseObjectCacheStrategy,
            ICacheStrategyDomain cacheStrategyDomain)
        {
            var identityName = cacheStrategyDomain.IdentityName;
            var mappingCollection = GetMappingCollection(identityName);//All base caches that the current extension cache may have registered

            if (mappingCollection.ContainsKey(baseObjectCacheStrategy))
            {
                var item = mappingCollection[baseObjectCacheStrategy];
                return item.DomainExtensionCacheStrategy;
            }
            else
            {
                //Not registered, use local cache strategy by default (emergency)
                var ex = new Exceptions.UnregisteredDomainCacheStrategyException(cacheStrategyDomain.GetType(), baseObjectCacheStrategy.GetType());
                SenparcTrace.BaseExceptionLog(ex);
                throw ex;
            }
        }

        /// <summary>
        /// Clear all registered domain cache objects
        /// </summary>
        public static void ClearRegisteredDomainExtensionCacheStrategies()
        {
            _extensionCacheStrategyInstance.Clear();
        }

        /// <summary>
        /// Automatically register domain cache
        /// </summary>
        /// <param name="autoScanExtensionCacheStrategies">Whether to automatically scan global extension caches (will increase system startup time)</param>
        /// <param name="extensionCacheStrategiesFunc"><para>Extension cache strategies that need to be manually registered</para>
        /// <para>(LocalContainerCacheStrategy, RedisContainerCacheStrategy, MemcacheContainerCacheStrategy are already automatically registered),</para>
        /// <para>If set to null (note: not delegate return null, but the entire delegate parameter is null), it will automatically use reflection to scan all possible extension cache strategies</para></param>
        ///<returns>Returns all added types</returns>
        public static List<Type> AutoScanDomainCacheStrategy(bool autoScanExtensionCacheStrategies = false, Func<IList<IDomainExtensionCacheStrategy>> extensionCacheStrategiesFunc = null)
        {
            //Register extension cache
            var dt1 = SystemTime.Now;
            var addedTypes = new List<Type>();
            var cacheTypes = "";//All registered extension caches

            if (extensionCacheStrategiesFunc != null)
            {
                var containerCacheStrategies = extensionCacheStrategiesFunc();
                if (containerCacheStrategies != null)
                {
                    foreach (var cacheStrategy in containerCacheStrategies)
                    {
                        var exCache = cacheStrategy;//Ensure it can run, will automatically register

                        var cacheType = exCache.GetType();
                        cacheTypes += "\r\n" + cacheType;
                        addedTypes.Add(cacheType);
                    }
                }
            }

            var scanTypesCount = 0;
            if (autoScanExtensionCacheStrategies)
            {
                //Find all extension caches  TODO: The scanning program can be centralized in a Helper or Utility
                var types = AppDomain.CurrentDomain.GetAssemblies()
                            .SelectMany(a =>
                            {
                                try
                                {
                                    scanTypesCount++;
                                    var aTypes = a.GetTypes();
                                    return aTypes.Where(t => !t.IsAbstract &&/* !officialTypes.Contains(t) &&*/ t.GetInterfaces().Contains(typeof(IDomainExtensionCacheStrategy)));
                                }
                                catch (Exception ex)
                                {
                                    Trace.SenparcTrace.SendCustomLog("UseSenparcGlobal() 自动扫描程序集异常：" + a.FullName, ex.ToString());
                                    return new List<Type>();//Cannot return null
                                }
                            });

                if (types != null)
                {
                    foreach (var type in types)
                    {
                        if (type == null)
                        {
                            continue;
                        }
                        try
                        {
                            var exCache = ReflectionHelper.GetStaticMember(type, "Instance");

                            cacheTypes += "\r\n" + type;//Since the number is small, use String instead of StringBuilder
                            addedTypes.Add(type);
                        }
                        catch (Exception ex)
                        {
                            Trace.SenparcTrace.BaseExceptionLog(new Exceptions.BaseException(ex.Message, ex));
                        }
                    }
                }
            }

            var dt2 = SystemTime.Now;
            var exCacheLog = "注册总用时：{0}ms\r\n自动扫描程序集：{1}个\r\n扩展缓存：{2}".FormatWith((dt2 - dt1).TotalMilliseconds, scanTypesCount, cacheTypes);
            Trace.SenparcTrace.SendCustomLog("自动注册扩展缓存完成", exCacheLog);

            return addedTypes;
        }
    }
}
