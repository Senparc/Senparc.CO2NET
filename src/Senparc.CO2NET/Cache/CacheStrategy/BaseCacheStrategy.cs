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

    文件名：BaseCacheStrategy.cs
    文件功能描述：泛型缓存策略基类。


    创建标识：Senparc - 20160813 v4.7.7 


    --CO2NET--

    修改标识：Senparc - 20180606
    修改描述：GetFinalKey() 方法添加虚方法关键字

 ----------------------------------------------------------------*/


using System;
using System.Threading.Tasks;

namespace Senparc.CO2NET.Cache
{
    /// <summary>
    /// 泛型缓存策略基类
    /// </summary>
    public abstract class BaseCacheStrategy : IBaseCacheStrategy
    {
        ///// <summary>
        ///// 默认下级命名空间
        ///// </summary>
        //public virtual string ChildNamespace { get; set; }

        /// <summary>
        /// 获取拼装后的FinalKey
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="isFullKey">是否已经是经过拼接的FullKey</param>
        /// <returns></returns>
        public virtual string GetFinalKey(string key, bool isFullKey = false)
        {
            return isFullKey ? key : $"Senparc:{Config.DefaultCacheNamespace}:{key}";
        }

        /// <summary>
        /// 获取一个同步锁
        /// </summary>
        /// <param name="resourceName"></param>
        /// <param name="key"></param>
        /// <param name="retryCount"></param>
        /// <param name="retryDelay"></param>
        /// <returns></returns>
        public abstract ICacheLock BeginCacheLock(string resourceName, string key, int retryCount = 0, TimeSpan retryDelay = new TimeSpan());


#if !NET35 && !NET40
        /// <summary>
        /// 【异步方法】创建一个（分布）锁
        /// </summary>
        /// <param name="resourceName">资源名称</param>
        /// <param name="key">Key标识</param>
        /// <param name="retryCount">重试次数</param>
        /// <param name="retryDelay">重试延时</param>
        /// <returns></returns>
        public abstract Task<ICacheLock> BeginCacheLockAsync(string resourceName, string key, int retryCount = 0, TimeSpan retryDelay = new TimeSpan());
#endif
    }
}
