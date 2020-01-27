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

/*----------------------------------------------------------------
    Copyright (C) 2020 Senparc

    文件名：DataOperation.cs
    文件功能描述：每一次跟踪日志的对象信息


    创建标识：Senparc - 20180602

    修改标识：Senparc - 20181226
    修改描述：支持 NeuChar v0.4.3 修改 DateTime 为 DateTimeOffset

    修改标识：Senparc - 20181116
    修改描述：v0.2.5 添加 ReadAndCleanDataItems 的 keepTodayData 属性，保留当天数据

    修改标识：Senparc - 20190523
    修改描述：v0.4 使用异步方法提升并发效率

    修改标识：Senparc - 20190523
    修改描述：v0.4.1.1 在静态构造函数中初始化 KindNameStore

    修改标识：Senparc - 20190523
    修改描述：v0.6.102 1、使用队列处理 DataOperation.SetAsync()
                       2、DataOperation.KindNameStore 使用 ConcurrentDictionary 类型

----------------------------------------------------------------*/


using Senparc.CO2NET.APM.Exceptions;
using Senparc.CO2NET.Trace;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Senparc.CO2NET.APM
{
    /// <summary>
    /// DataOperation
    /// </summary>
    public class DataOperation
    {
        const string CACHE_NAMESPACE = "SENPARC_APM";

        private string _domain;

        private string _domainKey;

        //TODO：需要考虑分布式的情况，最好储存在缓存中
        private static ConcurrentDictionary<string, ConcurrentDictionary<string, DateTimeOffset>> KindNameStore { get; set; } //= new Dictionary<string, Dictionary<string, DateTimeOffset>>();

        private string BuildFinalKey(string kindName)
        {
            return $"{_domainKey}:{kindName}";
        }

        /// <summary>
        /// 注册 Key
        /// </summary>
        /// <param name="kindName"></param>
        private async Task RegisterFinalKeyAsync(string kindName)
        {
            if (KindNameStore[_domain].ContainsKey(kindName))
            {
                return;
            }

            var cacheStragety = Cache.CacheStrategyFactory.GetObjectCacheStrategyInstance();
            var kindNameKey = $"{_domainKey}:_KindNameStore";
            var keyList = await cacheStragety.GetAsync<List<string>>(kindNameKey, true).ConfigureAwait(false) ?? new List<string>();
            if (!keyList.Contains(kindName))
            {
                keyList.Add(kindName);
                await cacheStragety.SetAsync(kindNameKey, keyList, isFullKey: true).ConfigureAwait(false); ;//永久储存
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
            _domainKey = $"{CACHE_NAMESPACE}:{_domain}";

            if (!KindNameStore.ContainsKey(_domain))
            {
                KindNameStore[_domain] = new ConcurrentDictionary<string, DateTimeOffset>();
            }
        }

        //static DataOperation()
        //{
        //    KindNameStore = new Dictionary<string, Dictionary<string, DateTimeOffset>>();

        //}

        static DataOperation()
        {
            KindNameStore = new ConcurrentDictionary<string, ConcurrentDictionary<string, DateTimeOffset>>();
        }

        /// <summary>
        /// 设置数据
        /// </summary>
        /// <param name="kindName">统计类别名称</param>
        /// <param name="value">统计值</param>
        /// <param name="data">复杂类型数据</param>
        /// <param name="tempStorage">临时储存信息</param>
        /// <param name="dateTime">发生时间，默认为当前系统时间</param>
        /// <returns></returns>
        public async Task<DataItem> SetAsync(string kindName, double value, object data = null, object tempStorage = null, DateTimeOffset? dateTime = null)
        {
            if (!Config.EnableAPM)
            {
                return null;//不启用，不进行记录
            }

            var dataItem = new DataItem()
            {
                KindName = kindName,
                Value = value,
                Data = data,
                TempStorage = tempStorage,
                DateTime = dateTime ?? SystemTime.Now
            };

            MessageQueue.SenparcMessageQueue queue = new MessageQueue.SenparcMessageQueue();
            queue.Add($"SenparcAPM-{kindName}-{DateTime.Now.Ticks}", async () =>
            {
                try
                {
                    var dt1 = SystemTime.Now;
                    var cacheStragety = Cache.CacheStrategyFactory.GetObjectCacheStrategyInstance();
                    var finalKey = BuildFinalKey(kindName);
                    //使用同步锁确定写入顺序
                    using (await cacheStragety.BeginCacheLockAsync("SenparcAPM", finalKey).ConfigureAwait(false))
                    {


                        var list = await GetDataItemListAsync(kindName).ConfigureAwait(false);
                        list.Add(dataItem);
                        await cacheStragety.SetAsync(finalKey, list, Config.DataExpire, true).ConfigureAwait(false);

                        await RegisterFinalKeyAsync(kindName).ConfigureAwait(false);//注册Key

                        if (SenparcTrace.RecordAPMLog)
                        {
                            SenparcTrace.SendCustomLog($"APM 性能记录 - DataOperation.Set - {_domain}:{kindName}", SystemTime.DiffTotalMS(dt1) + " ms");
                        }

                        return;// dataItem;
                    }
                }
                catch (Exception e)
                {
                    new APMException(e.Message, _domain, kindName, $"DataOperation.Set -  {_domain}:{kindName}");
                    return;// null;
                }
            });

            return dataItem;
        }

        /// <summary>
        /// 获取信息列表
        /// </summary>
        /// <param name="kindName"></param>
        /// <returns></returns>
        public async Task<List<DataItem>> GetDataItemListAsync(string kindName)
        {
            try
            {
                var cacheStragety = Cache.CacheStrategyFactory.GetObjectCacheStrategyInstance();
                var finalKey = BuildFinalKey(kindName);
                var list = await cacheStragety.GetAsync<List<DataItem>>(finalKey, true).ConfigureAwait(false);

                if (list != null)
                {
                    list = list.OrderBy(z => z.DateTime).ToList();
                }

                return list ?? new List<DataItem>();
            }
            catch (Exception e)
            {
                new APMException(e.Message, _domain, kindName, "DataOperation.GetDataItemList");
                return null;
            }
        }

        /// <summary>
        /// 获取并清空该 Domain 下的所有数据
        /// </summary>
        /// <returns></returns>
        /// <param name="removeReadItems">是否移除已读取的项目，默认为 true</param>
        /// <param name="keepTodayData">当 removeReadItems = true 时有效，在清理的时候是否保留当天的数据</param>
        public async Task<List<MinuteDataPack>> ReadAndCleanDataItemsAsync(bool removeReadItems = true, bool keepTodayData = true)
        {
            try
            {
                var dt1 = SystemTime.Now;

                var cacheStragety = Cache.CacheStrategyFactory.GetObjectCacheStrategyInstance();
                Dictionary<string, List<DataItem>> tempDataItems = new Dictionary<string, List<DataItem>>();

                var systemNow = SystemTime.Now.UtcDateTime;//统一UTC时间
                var nowMinuteTime = SystemTime.Now.AddSeconds(-SystemTime.Now.Second).AddMilliseconds(-SystemTime.Now.Millisecond);// new DateTimeOffset(systemNow.Year, systemNow.Month, systemNow.Day, systemNow.Hour, systemNow.Minute, 0, TimeSpan.Zero);

                //快速获取并清理数据
                foreach (var item in KindNameStore[_domain])
                {
                    var kindName = item.Key;
                    var finalKey = BuildFinalKey(kindName);
                    using (await cacheStragety.BeginCacheLockAsync("SenparcAPM", finalKey).ConfigureAwait(false))
                    {
                        var list = await GetDataItemListAsync(item.Key).ConfigureAwait(false);//获取列表
                        var completedStatData = list.Where(z => z.DateTime < nowMinuteTime).ToList();//统计范围内的所有数据

                        tempDataItems[kindName] = completedStatData;//添加到列表

                        if (removeReadItems)
                        {
                            //筛选需要删除的数据
                            var tobeRemove = completedStatData.Where(z => keepTodayData ? z.DateTime < SystemTime.Today : true);

                            //移除已读取的项目
                            if (tobeRemove.Count() == list.Count())
                            {
                                //已经全部删除
                                await cacheStragety.RemoveFromCacheAsync(finalKey, true).ConfigureAwait(false);//删除
                            }
                            else
                            {
                                //部分删除
                                var newList = list.Except(tobeRemove).ToList();
                                await cacheStragety.SetAsync(finalKey, newList, Config.DataExpire, true).ConfigureAwait(false);
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

                    var lastDataItemTime = DateTimeOffset.MinValue;

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
                            minuteData.Time = new DateTimeOffset(dataItem.DateTime.Year, dataItem.DateTime.Month, dataItem.DateTime.Day, dataItem.DateTime.Hour, dataItem.DateTime.Minute, 0, TimeSpan.Zero);
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

                //if (SenparcTrace.RecordAPMLog)
                {
                    SenparcTrace.SendCustomLog("APM 记录 - DataOperation.ReadAndCleanDataItems", SystemTime.DiffTotalMS(dt1) + " ms");
                }

                return result;
            }
            catch (Exception e)
            {
                new APMException(e.Message, _domain, "", "DataOperation.ReadAndCleanDataItems");
                return null;
            }
        }
    }
}