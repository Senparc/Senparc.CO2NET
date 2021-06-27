#region Apache License Version 2.0
/*----------------------------------------------------------------

Copyright 2021 Suzhou Senparc Network Technology Co.,Ltd.

Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file
except in compliance with the License. You may obtain a copy of the License at

http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software distributed under the
License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND,
either express or implied. See the License for the specific language governing permissions
and limitations under the License.

Detail: https://github.com/JeffreySu/WeiXinMPSDK/blob/master/license.md

----------------------------------------------------------------*/
#endregion Apache License Version 2.0

/*----------------------------------------------------------------
    Copyright (C) 2021 Senparc
    
    文件名：ApiBindInfoCollection.cs
    文件功能描述：API 绑定信息集合
    
    
    创建标识：Senparc - 20180901
    
    修改标识：Senparc - 20190513
    修改描述：v0.6.7 添加 ApiBindInfoCollection.GetGroupedCollection() 方法

    ---------- 2021.6.27 从 Senparc.NeuChar 移植

----------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Senparc.CO2NET.ApiBind
{
    /// <summary>
    /// ApiBind 绑定信息的全局唯一集合
    /// </summary>
    public class ApiBindInfoCollection : Dictionary<string, ApiBindInfo>
    {
        #region 单例

        //静态SearchCache
        public static ApiBindInfoCollection Instance
        {
            get
            {
                return Nested.instance;//返回Nested类中的静态成员instance
            }
        }

        class Nested
        {
            static Nested()
            {
            }
            //将instance设为一个初始化的BaseCacheStrategy新实例
            internal static readonly ApiBindInfoCollection instance = new ApiBindInfoCollection();
        }

        #endregion

        /// <summary>
        /// 获取全局唯一名称
        /// </summary>
        /// <param name="category">目录（平台类型），用于输出 API 的 Url 时分组</param>
        /// <param name="apiBindAttrName">跨程序集的通用名称（如：CustomApi.SendText）</param>
        /// <returns></returns>
        private string GetGlobalName(string category, string apiBindAttrName)
        {
            return $"{category}:{apiBindAttrName}";//TODO：生成全局唯一名称
        }

        /// <summary>
        /// ApiBindCollection 构造函数
        /// </summary>
        public ApiBindInfoCollection() : base(StringComparer.OrdinalIgnoreCase)
        {

        }

        /// <summary>
        /// 添加 ApiBindInfo 对象
        /// </summary>
        /// <param name="method"></param>
        /// <param name="apiBindAttr"></param>
        public void Add(MethodInfo method, ApiBindAttribute apiBindAttr)
        {
            var name = GetGlobalName(apiBindAttr.Category, apiBindAttr.Name);

            var finalName = name;
            var suffix = 0;
            //确保名称不会重复
            while (base.ContainsKey(finalName))
            {
                suffix++;
                finalName = name + suffix.ToString("00");
            }

            base.Add(finalName, new ApiBindInfo(apiBindAttr, method));
        }

        /// <summary>
        /// 获取 ApiBindInfo
        /// </summary>
        /// <param name="category">目录（平台类型），用于输出 API 的 Url 时分组</param>
        /// <param name="apiBindAttrName">跨程序集的通用名称（如：CustomApi.SendText）</param>
        public ApiBindInfo Get(string category, string apiBindAttrName)
        {
            var name = GetGlobalName(category, apiBindAttrName);
            if (ApiBindInfoCollection.Instance.ContainsKey(name))
            {
                return ApiBindInfoCollection.Instance[name];
            }
            return null;
        }

        /// <summary>
        /// 获取不同模块的分组 API 绑定信息（注意：每次获取都会重新执行分组过程）
        /// </summary>
        /// <returns></returns>
        public IEnumerable<IGrouping<string, KeyValuePair<string, ApiBindInfo>>> GetGroupedCollection()
        {
            var apiGroups = ApiBindInfoCollection.Instance.GroupBy(z => z.Value.ApiBindAttribute.Category);
            return apiGroups;
        }
    }
}
