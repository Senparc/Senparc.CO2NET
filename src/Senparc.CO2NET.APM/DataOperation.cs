#region Apache License Version 2.0
/*----------------------------------------------------------------

Copyright 2025 Suzhou Senparc Network Technology Co.,Ltd.

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
    Copyright (C) 2025 Senparc

    FileName: DataOperation.cs
    File Function Description: Object information for each tracking log


    Creation Identifier: Senparc - 20180602

    Modification Identifier: Senparc - 20181226
    Modification Description: Support NeuChar v0.4.3, change DateTime to DateTimeOffset

    Modification Identifier: Senparc - 20181116
    Modification Description: v0.2.5 Add keepTodayData property to ReadAndCleanDataItems, keep today's data

    Modification Identifier: Senparc - 20190523
    Modification Description: v0.4 Use asynchronous methods to improve concurrency efficiency

    Modification Identifier: Senparc - 20190523
    Modification Description: v0.4.1.1 Initialize KindNameStore in static constructor

    Modification Identifier: Senparc - 20190523
    Modification Description: v0.6.102 1. Use queue to handle DataOperation.SetAsync()
                       2. DataOperation.KindNameStore uses ConcurrentDictionary type

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

        //TODO: Consider distributed scenarios, preferably stored in cache
        private static ConcurrentDictionary<string, ConcurrentDictionary<string, DateTimeOffset>> KindNameStore { get; set; } //= new Dictionary<string, Dictionary<string, DateTimeOffset>>();

        private string BuildFinalKey(string kindName)
        {
            return $"{_domainKey}:{kindName}";
        }

        /// <summary>
        /// Register Key
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
                await cacheStragety.SetAsync(kindNameKey, keyList, isFullKey: true).ConfigureAwait(false);//Permanent storage
            }

            KindNameStore[_domain][kindName] = SystemTime.Now;
        }

        /// <summary>
        /// DataOperation constructor
        /// </summary>
        /// <param name="domain">Domain, the smallest unit of statistics, can be a website or a module</param>
        public DataOperation(string domain)
        {
            _domain = domain ?? "GLOBAL";//If not provided, it defaults to GLOBAL, shared globally
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
        /// Set data
        /// </summary>
        /// <param name="kindName">Category name of the statistics</param>
        /// <param name="value">Statistical value</param>
        /// <param name="data">Complex type data</param>
        /// <param name="tempStorage">Temporary storage information</param>
        /// <param name="dateTime">Occurrence time, default is the current system time</param>
        /// <returns></returns>
        public async Task<DataItem> SetAsync(string kindName, double value, object data = null, object tempStorage = null, DateTimeOffset? dateTime = null)
        {
            if (!Config.EnableAPM)
            {
                return null;//Not enabled, no record
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
                    //Use sync lock to determine write order
                    using (await cacheStragety.BeginCacheLockAsync("SenparcAPM", finalKey).ConfigureAwait(false))
                    {


                        var list = await GetDataItemListAsync(kindName).ConfigureAwait(false);
                        list.Add(dataItem);
                        await cacheStragety.SetAsync(finalKey, list, Config.DataExpire, true).ConfigureAwait(false);

                        await RegisterFinalKeyAsync(kindName).ConfigureAwait(false);//Register Key

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
        /// Get information list
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
        /// Get and clear all data under this Domain
        /// </summary>
        /// <returns></returns>
        /// <param name="removeReadItems">Whether to remove read items, default is true</param>
        /// <param name="keepTodayData">Effective when removeReadItems = true, whether to keep today's data when cleaning</param>
        public async Task<List<MinuteDataPack>> ReadAndCleanDataItemsAsync(bool removeReadItems = true, bool keepTodayData = true)
        {
            try
            {
                var dt1 = SystemTime.Now;

                var cacheStragety = Cache.CacheStrategyFactory.GetObjectCacheStrategyInstance();
                Dictionary<string, List<DataItem>> tempDataItems = new Dictionary<string, List<DataItem>>();

                var systemNow = SystemTime.Now.UtcDateTime;//Unified UTC time
                var nowMinuteTime = SystemTime.Now.AddSeconds(-SystemTime.Now.Second).AddMilliseconds(-SystemTime.Now.Millisecond);// new DateTimeOffset(systemNow.Year, systemNow.Month, systemNow.Day, systemNow.Hour, systemNow.Minute, 0, TimeSpan.Zero);

                //Quickly get and clean data
                foreach (var item in KindNameStore[_domain])
                {
                    var kindName = item.Key;
                    var finalKey = BuildFinalKey(kindName);
                    using (await cacheStragety.BeginCacheLockAsync("SenparcAPM", finalKey).ConfigureAwait(false))
                    {
                        var list = await GetDataItemListAsync(item.Key).ConfigureAwait(false);//Get list
                        var completedStatData = list.Where(z => z.DateTime < nowMinuteTime).ToList();//All data within the statistical range

                        tempDataItems[kindName] = completedStatData;//Add to list

                        if (removeReadItems)
                        {
                            //Filter data to be deleted
                            var tobeRemove = completedStatData.Where(z => keepTodayData ? z.DateTime < SystemTime.Today : true);

                            //Remove read items
                            if (tobeRemove.Count() == list.Count())
                            {
                                //All deleted
                                await cacheStragety.RemoveFromCacheAsync(finalKey, true).ConfigureAwait(false);//Delete
                            }
                            else
                            {
                                //Partially deleted
                                var newList = list.Except(tobeRemove).ToList();
                                await cacheStragety.SetAsync(finalKey, newList, Config.DataExpire, true).ConfigureAwait(false);
                            }
                        }
                    }
                }


                //Start processing data (in two steps to reduce sync lock time)
                var result = new List<MinuteDataPack>();
                foreach (var kv in tempDataItems)
                {
                    var kindName = kv.Key;
                    var domainData = kv.Value;

                    var lastDataItemTime = DateTimeOffset.MinValue;

                    MinuteDataPack minuteDataPack = new MinuteDataPack();
                    minuteDataPack.KindName = kindName;
                    result.Add(minuteDataPack);//Add a metric

                    MinuteData minuteData = null;//A metric for a certain minute
                    foreach (var dataItem in domainData)
                    {
                        if (DataHelper.IsLaterMinute(lastDataItemTime, dataItem.DateTime))
                        {
                            //New minute
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