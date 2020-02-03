#region LICENSE
/*
 *   Copyright 2014 Angelo Simone Scotto <scotto.a@gmail.com>
 * 
 *   Licensed under the Apache License, Version 2.0 (the "License");
 *   you may not use this file except in compliance with the License.
 *   You may obtain a copy of the License at
 * 
 *       http://www.apache.org/licenses/LICENSE-2.0
 * 
 *   Unless required by applicable law or agreed to in writing, software
 *   distributed under the License is distributed on an "AS IS" BASIS,
 *   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 *   See the License for the specific language governing permissions and
 *   limitations under the License.
 * 
 * */
#endregion

/*----------------------------------------------------------------
    Copyright (C) 2020 Senparc

    文件名：Redlock.cs
    文件功能描述：Redis 锁


    创建标识：Senparc - 20170402
    
    修改标识：Senparc - 20190412
    修改描述：v2.2.0 提供缓存异步接口
    
----------------------------------------------------------------*/

using CSRedis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Redlock.CSharp
{
    public class Redlock
    {
        public Redlock(int retryCount, TimeSpan retryDelay, params CSRedisClient[] list)
        {
            DefaultRetryCount = retryCount;
            DefaultRetryDelay = retryDelay;
            foreach (var item in list)
            {
                this.redisMasterDictionary.Add(item.Nodes.Keys.First(), item);
            }
        }

        public Redlock(params CSRedisClient[] list)
            : this(3, new TimeSpan(0, 0, 0, 0, 200), list)
        {

        }

        readonly int DefaultRetryCount = 3;
        readonly TimeSpan DefaultRetryDelay = new TimeSpan(0, 0, 0, 0, 200);
        const double ClockDriveFactor = 0.01;

        protected int Quorum { get { return (redisMasterDictionary.Count / 2) + 1; } }

        /// <summary>
        /// String containing the Lua unlock script.
        /// </summary>
        const String UnlockScript = @"
            if redis.call(""get"",KEYS[1]) == ARGV[1] then
                return redis.call(""del"",KEYS[1])
            else
                return 0
            end";


        protected static byte[] CreateUniqueLockId()
        {
            return Guid.NewGuid().ToByteArray();
        }


        protected Dictionary<String, CSRedisClient> redisMasterDictionary = new Dictionary<string, CSRedisClient>();

        #region 同步方法

        //TODO: Refactor passing a ConnectionMultiplexer
        protected bool LockInstance(string redisServer, string resource, byte[] val, TimeSpan ttl)
        {

            bool succeeded;
            try
            {
                var redis = this.redisMasterDictionary[redisServer];
                succeeded = redis.Set(resource, val, ttl, RedisExistence.Nx);
            }
            catch (Exception)
            {
                succeeded = false;
            }
            return succeeded;
        }

        //TODO: Refactor passing a ConnectionMultiplexer
        protected void UnlockInstance(string redisServer, string resource, byte[] val)
        {
            string key = resource;
            byte[][] values = { val };
            var redis = redisMasterDictionary[redisServer];
            redis.Eval(
                UnlockScript,
                key,
                values
                );
        }

        public bool Lock(string resource, TimeSpan ttl, out Lock lockObject)
        {
            var val = CreateUniqueLockId();
            Lock innerLock = null;
            bool successfull = retry(DefaultRetryCount, DefaultRetryDelay, () =>
            {
                try
                {
                    int n = 0;
                    var startTime = DateTime.Now;

                    // Use keys
                    for_each_redis_registered(
                        redis =>
                        {
                            if (LockInstance(redis, resource, val, ttl)) n += 1;
                        }
                    );

                    /*
                     * Add 2 milliseconds to the drift to account for Redis expires
                     * precision, which is 1 millisecond, plus 1 millisecond min drift 
                     * for small TTLs.        
                     */
                    var drift = Convert.ToInt32((ttl.TotalMilliseconds * ClockDriveFactor) + 2);
                    var validity_time = ttl - (DateTime.Now - startTime) - new TimeSpan(0, 0, 0, 0, drift);

                    if (n >= Quorum && validity_time.TotalMilliseconds > 0)
                    {
                        innerLock = new Lock(resource, val, validity_time);
                        return true;
                    }
                    else
                    {
                        for_each_redis_registered(
                            redis =>
                            {
                                UnlockInstance(redis, resource, val);
                            }
                        );
                        return false;
                    }
                }
                catch (Exception)
                { return false; }
            });

            lockObject = innerLock;
            return successfull;
        }

        protected void for_each_redis_registered(Action<CSRedisClient> action)
        {
            foreach (var item in redisMasterDictionary)
            {
                action(item.Value);
            }
        }

        protected void for_each_redis_registered(Action<String> action)
        {
            foreach (var item in redisMasterDictionary)
            {
                action(item.Key);
            }
        }

        protected bool retry(int retryCount, TimeSpan retryDelay, Func<bool> action)
        {
            int maxRetryDelay = (int)retryDelay.TotalMilliseconds;
            Random rnd = new Random();
            int currentRetry = 0;

            while (currentRetry++ < retryCount)
            {
                if (action()) return true;
                Thread.Sleep(rnd.Next(maxRetryDelay));
            }
            return false;
        }

        public void Unlock(Lock lockObject)
        {
            for_each_redis_registered(redis =>
            {
                UnlockInstance(redis, lockObject.Resource, lockObject.Value);
            });
        }

        #endregion

        #region 异步方法

        //TODO: Refactor passing a ConnectionMultiplexer
        protected async Task<bool> LockInstanceAsync(string redisServer, string resource, byte[] val, TimeSpan ttl)
        {

            bool succeeded;
            try
            {
                var redis = this.redisMasterDictionary[redisServer];
                succeeded = await redis.SetAsync(resource, val, ttl, RedisExistence.Nx).ConfigureAwait(false);
            }
            catch (Exception)
            {
                succeeded = false;
            }
            return succeeded;
        }

        //TODO: Refactor passing a ConnectionMultiplexer
        protected async Task UnlockInstanceAsync(string redisServer, string resource, byte[] val)
        {
            string key = resource;
            byte[][] values = { val };
            var redis = redisMasterDictionary[redisServer];
            await redis.EvalAsync(
                   UnlockScript,
                   key,
                   values
                   ).ConfigureAwait(false);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="resource"></param>
        /// <param name="ttl"></param>
        /// <returns>bool：successfull，Lock：lockObject</returns>
        public async Task<Tuple<bool, Lock>> LockAsync(string resource, TimeSpan ttl)
        {
            var val = CreateUniqueLockId();
            Lock innerLock = null;
            bool successfull = await retryAsync(DefaultRetryCount, DefaultRetryDelay, async () =>
            {
                try
                {
                    int n = 0;
                    var startTime = DateTime.Now;

                    // Use keys
                    for_each_redis_registered(
                          async redis =>
                          {
                              if (await LockInstanceAsync(redis, resource, val, ttl).ConfigureAwait(false)) n += 1;
                          }
                       );

                    /*
                     * Add 2 milliseconds to the drift to account for Redis expires
                     * precision, which is 1 millisecond, plus 1 millisecond min drift 
                     * for small TTLs.        
                     */
                    var drift = Convert.ToInt32((ttl.TotalMilliseconds * ClockDriveFactor) + 2);
                    var validity_time = ttl - (DateTime.Now - startTime) - new TimeSpan(0, 0, 0, 0, drift);

                    if (n >= Quorum && validity_time.TotalMilliseconds > 0)
                    {
                        innerLock = new Lock(resource, val, validity_time);
                        return true;
                    }
                    else
                    {
                        for_each_redis_registered(
                             async redis =>
                             {
                                 await UnlockInstanceAsync(redis, resource, val).ConfigureAwait(false);
                             }
                          );
                        return false;
                    }
                }
                catch (Exception)
                { return false; }
            }).ConfigureAwait(false);

            return Tuple.Create(successfull, innerLock);
        }

        protected async Task for_each_redis_registeredAsync(Action<CSRedisClient> action)
        {
            await Task.Factory.StartNew(() => for_each_redis_registered(action)).ConfigureAwait(false);
        }

        protected async Task for_each_redis_registeredAsync(Action<String> action)
        {
            await Task.Factory.StartNew(() => for_each_redis_registered(action)).ConfigureAwait(false);

        }

        protected async Task<bool> retryAsync(int retryCount, TimeSpan retryDelay, Func<Task<bool>> action)
        {
            int maxRetryDelay = (int)retryDelay.TotalMilliseconds;
            Random rnd = new Random();
            int currentRetry = 0;

            while (currentRetry++ < retryCount)
            {
                if (await action().ConfigureAwait(false)) return true;
                Thread.Sleep(rnd.Next(maxRetryDelay));
            }
            return false;
        }

        public async Task UnlockAsync(Lock lockObject)
        {
            await for_each_redis_registeredAsync(async redis =>
            {
                await UnlockInstanceAsync(redis, lockObject.Resource, lockObject.Value).ConfigureAwait(false);
            }).ConfigureAwait(false);
        }

        #endregion

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(this.GetType().FullName);

            sb.AppendLine("Registered Connections:");
            foreach (var item in redisMasterDictionary)
            {
                sb.AppendLine(item.Key.First().ToString());
            }

            return sb.ToString();
        }
    }
}
