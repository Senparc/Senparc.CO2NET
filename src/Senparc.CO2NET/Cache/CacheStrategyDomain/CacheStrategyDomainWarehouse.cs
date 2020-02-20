/*----------------------------------------------------------------
    Copyright (C) 2020 Senparc

    文件名：CacheStrategyDomainWarehouse.cs
    文件功能描述： 领域缓存仓库


    创建标识：Senparc - 20180609

    修改标识：Senparc - 20180707
    修改描述：添加 AutoScanDomainCacheStrategy()、ClearRegisteredDomainExtensionCacheStrategies() 方法

----------------------------------------------------------------*/

using Senparc.CO2NET.Extensions;
using Senparc.CO2NET.Helpers;
using Senparc.CO2NET.Trace;
using System;
using System.Collections.Generic;
using System.Linq;

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
        /// <param name="item"></param>
        public void AddOrUpdate(CacheStrategyDomainMappingItem item)
        {
            var cacheStrategy = item.BaseObjectCacheStrategy();
            base[cacheStrategy] = item;
        }
    }

    /// <summary>
    /// 领域缓存仓库
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
                var ex = new Exceptions.UnregisteredDomainCacheStrategyException(cacheStrategyDomain.GetType(), baseObjectCacheStrategy.GetType());
                SenparcTrace.BaseExceptionLog(ex);
                throw ex;
            }
        }

        /// <summary>
        /// 清空所有已经祖册的领域缓存对象
        /// </summary>
        public static void ClearRegisteredDomainExtensionCacheStrategies()
        {
            _extensionCacheStrategyInstance.Clear();
        }

        /// <summary>
        /// 自动注册领域缓存
        /// </summary>
        /// <param name="autoScanExtensionCacheStrategies">是否自动扫描全局的扩展缓存（会增加系统启动时间）</param>
        /// <param name="extensionCacheStrategiesFunc"><para>需要手动注册的扩展缓存策略</para>
        /// <para>（LocalContainerCacheStrategy、RedisContainerCacheStrategy、MemcacheContainerCacheStrategy已经自动注册），</para>
        /// <para>如果设置为 null（注意：不适委托返回 null，是整个委托参数为 null），则自动使用反射扫描所有可能存在的扩展缓存策略</para></param>
        ///<returns>返回所有添加的类型</returns>
        public static List<Type> AutoScanDomainCacheStrategy(bool autoScanExtensionCacheStrategies = false, Func<IList<IDomainExtensionCacheStrategy>> extensionCacheStrategiesFunc = null)
        {
            //注册扩展缓存
            var dt1 = SystemTime.Now;
            var addedTypes = new List<Type>();
            var cacheTypes = "";//所有注册的扩展缓存

            if (extensionCacheStrategiesFunc != null)
            {
                var containerCacheStrategies = extensionCacheStrategiesFunc();
                if (containerCacheStrategies != null)
                {
                    foreach (var cacheStrategy in containerCacheStrategies)
                    {
                        var exCache = cacheStrategy;//确保能运行到就行，会自动注册

                        var cacheType = exCache.GetType();
                        cacheTypes += "\r\n" + cacheType;
                        addedTypes.Add(cacheType);
                    }
                }
            }

            var scanTypesCount = 0;
            if (autoScanExtensionCacheStrategies)
            {
                //查找所有扩展缓存  TODO:扫描程序可以集中到一个 Helper 或者 Utility 中
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
                                    return new List<Type>();//不能 return null
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

                            cacheTypes += "\r\n" + type;//由于数量不多，这里使用String，不使用StringBuilder
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
