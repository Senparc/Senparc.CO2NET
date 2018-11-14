using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Senparc.CO2NET.APM
{
    public class DataOperation
    {
        const string CACHE_NAMESPACE = "SENPARC_APM";

        private string _domain;

        private string _domainKey;

        //TODO：需要考虑分布式的情况，最好储存在缓存中
        private static Dictionary<string, Dictionary<string, DateTime>> KindNameStore { get; set; } = new Dictionary<string, Dictionary<string, DateTime>>();

        private string BuildFinalKey(string kindName)
        {
            return $"{_domainKey}:{kindName}";
        }

        /// <summary>
        /// 注册 Key
        /// </summary>
        /// <param name="finalKey"></param>
        private void RegisterFinalKey(string kindName)
        {
            if (KindNameStore[_domain].ContainsKey(kindName))
            {
                return;
            }

            var cacheStragety = Cache.CacheStrategyFactory.GetObjectCacheStrategyInstance();
            var kindNameKey = $"{_domainKey}:_KindNameStore";
            var keyList = cacheStragety.Get<List<string>>(kindNameKey);
            if (!keyList.Contains(kindName))
            {
                keyList.Add(kindName);
                cacheStragety.Set(kindNameKey, keyList);//永久储存
            }
            KindNameStore[_domain][kindName] = SystemTime.Now;
        }

        /// <summary>
        /// DataOperation 构造函数
        /// </summary>
        /// <param name="domain">域，统计的最小单位，可以是一个网站，也可以是一个模块</param>
        public DataOperation(string domain)
        {
            _domain = domain;
            _domainKey = $"{CACHE_NAMESPACE}:{domain}";

            if (!KindNameStore.ContainsKey(domain))
            {
                KindNameStore[domain] = new Dictionary<string, DateTime>();
            }
        }

        /// <summary>
        /// 设置数据
        /// </summary>
        /// <param name="kindName">统计类别名称</param>
        /// <param name="value">统计值</param>
        /// <param name="data">复杂类型数据</param>
        /// <returns></returns>
        public DataItem Set(string kindName, double value, object data = null, DateTime? dateTime = null)
        {
            var cacheStragety = Cache.CacheStrategyFactory.GetObjectCacheStrategyInstance();
            var finalKey = BuildFinalKey(kindName);
            //使用同步锁确定写入顺序
            using (cacheStragety.BeginCacheLock("SenparcAPM", finalKey))
            {
                var dataItem = new DataItem()
                {
                    KindName = kindName,
                    Value = value,
                    Data = data,
                    DateTime = dateTime ?? SystemTime.Now
                };

                var list = GetDataItemList(kindName);
                list.Add(dataItem);
                cacheStragety.Set(finalKey, list, Config.DataExpire, true);

                RegisterFinalKey(kindName);//注册Key

                return dataItem;
            }
        }

        /// <summary>
        /// 获取信息列表
        /// </summary>
        /// <param name="kindName"></param>
        /// <returns></returns>
        public List<DataItem> GetDataItemList(string kindName)
        {
            var cacheStragety = Cache.CacheStrategyFactory.GetObjectCacheStrategyInstance();
            var finalKey = BuildFinalKey(kindName);
            var list = cacheStragety.Get<List<DataItem>>(finalKey, true);
            return list ?? new List<DataItem>();
        }

        /// <summary>
        /// 获取并清空该 Domain 下的所有数据
        /// </summary>
        /// <returns></returns>
        public List<List<DataItem>> ReadAndCleanDataItems()
        {
            var cacheStragety = Cache.CacheStrategyFactory.GetObjectCacheStrategyInstance();
            List<List<DataItem>> result = new List<List<DataItem>>();
            foreach (var item in KindNameStore[_domain])
            {
                var kindName = item.Key;
                var finalKey = BuildFinalKey(kindName);
                using (cacheStragety.BeginCacheLock("SenparcAPM", finalKey))
                {
                    var list = GetDataItemList(item.Key);//获取列表
                    result.Add(list);//添加到列表
                    
                    cacheStragety.RemoveFromCache(finalKey);//删除
                }
            }
            return result;
        }
    }
}