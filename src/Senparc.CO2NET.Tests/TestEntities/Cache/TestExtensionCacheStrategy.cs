using Senparc.CO2NET.Cache;
using System;
using System.Collections.Generic;
using System.Text;

namespace Senparc.CO2NET.Tests.TestEntities
{
    /// <summary>
    /// 测试用的扩展缓存策略（默认使用本地缓存，可以修改）
    /// </summary>
    public class TestExtensionCacheStrategy : IDomainExtensionCacheStrategy
    {
        #region 单例

        ///<summary>
        /// LocalCacheStrategy的构造函数
        ///</summary>
        TestExtensionCacheStrategy()
        {
            //向底层缓存注册当前缓存策略
            CacheStrategyDomainWarehouse.RegisterCacheStrategyDomain(this);
        }

        //静态LocalCacheStrategy
        public static TestExtensionCacheStrategy Instance
        {
            get
            {
                return Nested.instance;//返回Nested类中的静态成员instance
            }
        }


        class Nested
        {
            static Nested()
            {
            }
            //将instance设为一个初始化的LocalCacheStrategy新实例
            internal static readonly TestExtensionCacheStrategy instance = new TestExtensionCacheStrategy();
        }


        #endregion


        //设置此属性用于指定当前扩展缓存所属领域
        public ICacheStrategyDomain CacheStrategyDomain => new TestCacheDomain();

        //所使用的基础缓存策略（也可以在委托内动态调整，但是不建议。）
        public Func<IBaseObjectCacheStrategy> BaseCacheStrategy =>
            () => LocalObjectCacheStrategy.Instance;//默认使用本地缓存

        /// <summary>
        /// 扩展一个方法
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string GetTestCache(string key)
        {
            return BaseCacheStrategy().Get(key).ToString();
        }
    }
}
