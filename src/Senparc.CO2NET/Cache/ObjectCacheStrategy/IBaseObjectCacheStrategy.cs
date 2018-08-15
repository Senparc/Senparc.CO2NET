#region Apache License Version 2.0
/*----------------------------------------------------------------

Copyright 2018 Jeffrey Su & Suzhou Senparc Network Technology Co.,Ltd.

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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Senparc.CO2NET.Cache
{
    /// <summary>
    /// 所有以 string 类型为 key ，object 为 value 的缓存策略接口
    /// </summary>
    public interface IBaseObjectCacheStrategy : IBaseCacheStrategy<string, object>, IBaseCacheStrategy
    {
        //IContainerCacheStrategy ContainerCacheStrategy { get; }

        /// <summary>
        /// 注册的扩展缓存策略
        /// </summary>
        //Dictionary<IExtensionCacheStrategy, IBaseObjectCacheStrategy> ExtensionCacheStratety { get; set; }
    }
}
