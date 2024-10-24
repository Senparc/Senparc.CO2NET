#region Apache License Version 2.0
/*----------------------------------------------------------------

Copyright 2024 Suzhou Senparc Network Technology Co.,Ltd.

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
    Copyright (C) 2024 Senparc

    FileName：SenparcMessageQueue.cs
    File Function Description：SenparcMessageQueue message queue


    Creation Identifier：Senparc - 20151226

    Modification Identifier：Senparc - 20160210
    Modification Description：v4.5.10 Removed MessageQueueList, used MessageQueueDictionary.Keys to record identifiers
              (Using MessageQueueDictionary.Keys may cause unordered execution of stored items)


    ----  CO2NET   ----
    ----  split from Senparc.Weixin/MessageQueue/SenparcMessageQueue.cs  ----

    Modification Identifier：Senparc - 20180601
    Modification Description：v0.1.0 Ported SenparcMessageQueue

    Modification Identifier：Senparc - 20190812
    Modification Description：v0.8.8 Switched to using MessageQueueDictionary inherited from ConcurrentDictionary, adjusted methods

    Modification Identifier：Senparc - 20191009
    Modification Description：v1.0.102 Modified SenparcMessageQueue.GetCurrentKey() method

----------------------------------------------------------------*/



using System;
using System.Collections.Generic;
using System.Linq;

namespace Senparc.CO2NET.MessageQueue
{
    /// <summary>
    /// Message queue
    /// </summary>
    public class SenparcMessageQueue
    {
        /// <summary>
        /// Queue data collection
        /// </summary>
        public static MessageQueueDictionary MessageQueueDictionary = new MessageQueueDictionary();

        /// <summary>
        /// Synchronous execution lock (Note: Do not use concurrent locks here, nor use local locks in caching strategies, otherwise logging within external locks may cause a dead loop)
        /// </summary>
        private static object MessageQueueSyncLock = new object();
        /// <summary>
        /// Immediately synchronize all cache execution locks (used by OperateQueue())
        /// </summary>
        private static object FlushCacheLock = new object();

        /// <summary>
        /// Generate Key
        /// </summary>
        /// <param name="name">Queue application name, such as "ContainerBag"</param>
        /// <param name="senderType">Type of the operation object</param>
        /// <param name="identityKey">Unique identifier Key of the object</param>
        /// <param name="actionName">Operation name, such as "UpdateContainerBag"</param>
        /// <returns></returns>
        public static string GenerateKey(string name, Type senderType, string identityKey, string actionName)
        {
            var key = string.Format("Name@{0}||Type@{1}||Key@{2}||ActionName@{3}",
                name, senderType, identityKey, actionName);
            return key;
        }

        /// <summary>
        /// Operate queue
        /// </summary>
        public static void OperateQueue()
        {
            lock (FlushCacheLock)
            {
                var mq = new SenparcMessageQueue();
                var key = mq.GetCurrentKey(); //Get the latest Key
                while (!string.IsNullOrEmpty(key))
                {
                    var mqItem = mq.GetItem(key); //Get task item
                    mqItem.Action(); //Execute
                    mq.Remove(key, out SenparcMessageQueueItem value); //Clear
                    key = mq.GetCurrentKey(); //Get the latest Key
                }
            }
        }

        /// <summary>
        /// Get the current Key waiting to be executed
        /// </summary>
        /// <returns></returns>
        public string GetCurrentKey()
        {
            lock (MessageQueueSyncLock)
            {
                //Do not use Key directly because the order of Keys is uncertain
                var value = MessageQueueDictionary.Values.OrderBy(z=>z.AddTime).FirstOrDefault();
                if (value==null)
                {
                    return null;
                }
                return value.Key;
            }
        }

        /// <summary>
        /// Get SenparcMessageQueueItem
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public SenparcMessageQueueItem GetItem(string key)
        {
            lock (MessageQueueSyncLock)
            {
                if (MessageQueueDictionary.ContainsKey(key))
                {
                    return MessageQueueDictionary[key];
                }
                return null;
            }
        }

        /// <summary>
        /// Add queue member
        /// </summary>
        /// <param name="key"></param>
        /// <param name="action"></param>
        public SenparcMessageQueueItem Add(string key, Action action)
        {
            lock (MessageQueueSyncLock)
            {
                //if (!MessageQueueDictionary.ContainsKey(key))
                //{
                //    MessageQueueList.Add(key);
                //}
                //else
                //{
                //    MessageQueueList.Remove(key);
                //    MessageQueueList.Add(key);//Move to the end
                //}

                var mqItem = new SenparcMessageQueueItem(key, action);
                MessageQueueDictionary.TryAdd(key, mqItem);
                return mqItem;
            }
        }

        /// <summary>
        /// Remove queue member
        /// </summary>
        /// <param name="key">Cache key</param>
        /// <param name="value">Value object of the removed information</param>
        /// <returns>Returns true if removal is successful or key does not exist, otherwise returns false</returns>
        public bool Remove(string key, out SenparcMessageQueueItem value)
        {
            lock (MessageQueueSyncLock)
            {
                if (MessageQueueDictionary.ContainsKey(key))
                {
                    return MessageQueueDictionary.TryRemove(key, out value);
                    //MessageQueueList.Remove(key);
                }
                else
                {
                    value = null;
                    return true;
                }
            }
        }

        /// <summary>
        /// Get the current queue count
        /// </summary>
        /// <returns></returns>
        public int GetCount()
        {
            lock (MessageQueueSyncLock)
            {
                return MessageQueueDictionary.Count;
            }
        }

    }
}
