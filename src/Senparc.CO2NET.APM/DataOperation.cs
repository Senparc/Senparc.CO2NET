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
            var keyList = cacheStragety.Get<List<string>>(kindNameKey) ?? new List<string>();
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
            _domain = domain ?? "GLOBAL";//如果未提供，则统一为 GLOBAL，全局共享
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
        /// <param name="removeReadItems">是否移除已读取的项目，默认为 true</param>
        public List<MinuteDataPack> ReadAndCleanDataItems(bool removeReadItems = true)
        {
            var cacheStragety = Cache.CacheStrategyFactory.GetObjectCacheStrategyInstance();
            Dictionary<string, List<DataItem>> tempDataItems = new Dictionary<string, List<DataItem>>();

            var systemNow = SystemTime.Now;
            var nowMinuteTime = new DateTime(systemNow.Year, systemNow.Month, systemNow.Day, systemNow.Hour, systemNow.Minute, 0);

            //快速获取并清理数据
            foreach (var item in KindNameStore[_domain])
            {
                var kindName = item.Key;
                var finalKey = BuildFinalKey(kindName);
                using (cacheStragety.BeginCacheLock("SenparcAPM", finalKey))
                {
                    var list = GetDataItemList(item.Key);//获取列表
                    var toveRemove = list.Where(z => z.DateTime < nowMinuteTime);

                    tempDataItems[kindName] = toveRemove.ToList();//添加到列表

                    if (removeReadItems)
                    {
                        //移除已读取的项目
                        if (toveRemove.Count() == list.Count())
                        {
                            //已经全部删除
                            cacheStragety.RemoveFromCache(finalKey, true);//删除
                        }
                        else
                        {
                            //部分删除
                            var newList = list.Except(toveRemove).ToList();
                            cacheStragety.Set(finalKey, newList, Config.DataExpire, true);
                        }
                    }
                }
            }


            //开始处理数据（分两步是为了减少同步锁的时间）
            var result = new List<MinuteDataPack>();
            foreach (var kv in tempDataItems)
            {
                var kindName = kv.Key;
                var domainData = kv.Value;

                DateTime lastDataItemTime = DateTime.MinValue;

                MinuteDataPack minuteDataPack = new MinuteDataPack();
                minuteDataPack.KindName = kindName;
                result.Add(minuteDataPack);//添加一个指标

                MinuteData minuteData = null;//某一分钟的指标
                foreach (var dataItem in domainData)
                {
                    if (DataHelper.IsLaterMinute(lastDataItemTime, dataItem.DateTime))
                    {
                        //新的一分钟
                        minuteData = new MinuteData();
                        minuteDataPack.MinuteDataList.Add(minuteData);

                        minuteData.KindName = dataItem.KindName;
                        minuteData.Time = new DateTime(dataItem.DateTime.Year, dataItem.DateTime.Month, dataItem.DateTime.Day, dataItem.DateTime.Hour, dataItem.DateTime.Minute, 0);
                        minuteData.StartValue = dataItem.Value;
                        minuteData.HighestValue = dataItem.Value;
                        minuteData.LowestValue = dataItem.Value;
                    }

                    minuteData.EndValue = dataItem.Value;
                    minuteData.SumValue += dataItem.Value;

                    if (dataItem.Value > minuteData.HighestValue)
                    {
                        minuteData.HighestValue = dataItem.Value;
                    }

                    if (dataItem.Value < minuteData.LowestValue)
                    {
                        minuteData.LowestValue = dataItem.Value;
                    }


                    minuteData.SampleSize++;

                    lastDataItemTime = dataItem.DateTime;
                }
            }

            return result;
        }
    }
}