#region Apache License Version 2.0
/*----------------------------------------------------------------

Copyright 2023 Suzhou Senparc Network Technology Co.,Ltd.

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
  
    FileName: Enums.cs
    File Function Description: Enum configuration file
    
    
    Creation Identifier: Senparc - 20180602
 
    Modification Identifier: Senparc - 20180704
    Modification Description: v0.5.1.1 Added Sex.NotSet enum value

    Modification Identifier: Senparc - 20190507
    Modification Description: v0.7.1 Added DayOfWeekString, DILifecycleType configuration and enums


----------------------------------------------------------------*/

namespace Senparc.CO2NET
{
    /// <summary>
    /// Enum
    /// </summary>
    public static class Enums
    {
        /// <summary>
        /// Weekday
        /// </summary>
        public static readonly string[] DayOfWeekString = new[]
        {
            "星期日",
            "星期一",
            "星期二",
            "星期三",
            "星期四",
            "星期五",
            "星期六"
        };
    }

    /// <summary>
    /// Cache type
    /// </summary>
    public enum CacheType
    {
        /// <summary>
        /// Local runtime cache (single machine)
        /// </summary>
        Local,
        /// <summary>
        /// Redis cache (supports distributed)
        /// </summary>
        Redis,
        /// <summary>
        /// Memcached (supports distributed)
        /// </summary>
        Memcached
    }

    /// <summary>
    /// Gender in user information (sex)
    /// </summary>
    public enum Sex
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member   
        未知 = 0,
        未设置 = 0,
        男 = 1,
        女 = 2,
        其他 = 3
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    }

    /// <summary>
    /// Lifecycle of dependency injection
    /// </summary>
    public enum DILifecycleType
    {
        Scoped,
        Singleton,
        Transient
    }

}
