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

        private static Dictionary<string, DateTime> KeyStore { get; set; } = new Dictionary<string, DateTime>();

        private string BuildFinalKey(string kindName)
        {
            return $"{_domainKey}:{kindName}";
        }

        /// <summary>
        /// 注册 Key
        /// </summary>
        /// <param name="finalKey"></param>
        private void RegisterFinalKey(string finalKey)
        {
            if (KeyStore.ContainsKey(finalKey))
            {
                return;
            }

            var cacheStragety = Cache.CacheStrategyFactory.GetObjectCacheStrategyInstance();
            var keyForKeys = $"{_domainKey}:_keyStore";
            var keyList = cacheStragety.Get<List<string>>(keyForKeys);
            if (!keyList.Contains(finalKey))
            {
                keyList.Add(finalKey);
                cacheStragety.Set(keyForKeys, keyList);//永久储存
            }
            KeyStore[finalKey] = SystemTime.Now;
        }

        /// <summary>
        /// DataOperation 构造函数
        /// </summary>
        /// <param name="domain">域，统计的最小单位，可以是一个网站，也可以是一个模块</param>
        public DataOperation(string domain)
        {
            _domain = domain;
            _domainKey = $"{CACHE_NAMESPACE}:{domain}";
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

                RegisterFinalKey(finalKey);//注册Key

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
    }
}