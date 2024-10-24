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

Detail: https://github.com/JeffreySu/WeiXinMPSDK/blob/master/license.md

----------------------------------------------------------------*/
#endregion Apache License Version 2.0

/*----------------------------------------------------------------
    Copyright (C) 2024 Senparc
    
    FileName：ApiBindInfoCollection.cs
    File Function Description：API binding information collection
    
    
    Creation Identifier：Senparc - 20180901
    
    Modification Identifier：Senparc - 20190513
    Modification Description：v0.6.7 Added ApiBindInfoCollection.GetGroupedCollection() method

    ---------- 2021.6.27 Migrated from Senparc.NeuChar

----------------------------------------------------------------*/

using Senparc.CO2NET.WebApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Senparc.CO2NET.ApiBind
{
    /// <summary>
    /// Globally unique collection of ApiBind binding information
    /// </summary>
    public class ApiBindInfoCollection : Dictionary<string, ApiBindInfo>
    {
        #region 单例

        //Static SearchCache
        public static ApiBindInfoCollection Instance
        {
            get
            {
                return Nested.instance;//Returns the static member instance in the Nested class
            }
        }

        class Nested
        {
            static Nested()
            {
            }
            //Set instance to a new initialized BaseCacheStrategy instance
            internal static readonly ApiBindInfoCollection instance = new ApiBindInfoCollection();
        }

        #endregion

        /// <summary>
        /// Get globally unique name
        /// </summary>
        /// <param name="category">Category (platform type), used for grouping when outputting API Url</param>
        /// <param name="apiBindAttrName">Common name across assemblies (e.g., CustomApi.SendText)</param>
        /// <returns></returns>
        private string GetGlobalName(string category, string apiBindAttrName)
        {
            return $"{category}:{apiBindAttrName}";//TODO: Generate globally unique name
        }

        /// <summary>
        /// ApiBindCollection constructor
        /// </summary>
        public ApiBindInfoCollection() : base(StringComparer.OrdinalIgnoreCase)
        {

        }

        /// <summary>
        /// Add ApiBindInfo object
        /// </summary>
        /// <param name="method"></param>
        /// <param name="apiBindAttr"></param>
        public void Add(ApiBindOn apiBindOn, string cagtegory, MethodInfo method, ApiBindAttribute apiBindAttr)
        {
            var category = apiBindAttr.GetCategoryName(method);
            var name = apiBindAttr.GetName(method);
            var globalName = GetGlobalName(category, name);

            var finalGlobalName = globalName;
            var suffix = 0;
            //Ensure the name is not duplicated
            while (base.ContainsKey(finalGlobalName))
            {
                suffix++;
                finalGlobalName = globalName + suffix.ToString("00");
            }

            base.Add(finalGlobalName, new ApiBindInfo(apiBindOn, cagtegory, finalGlobalName, name, apiBindAttr.BaseApiControllerType, apiBindAttr.BaseApiControllerOrder, apiBindAttr, method));
        }

        /// <summary>
        /// Get ApiBindInfo
        /// </summary>
        /// <param name="category">Category (platform type), used for grouping when outputting API Url</param>
        /// <param name="apiBindAttrName">Common name across assemblies (e.g., CustomApi.SendText)</param>
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
        /// Get grouped API binding information for different modules (Note: Each retrieval will re-execute the grouping process)
        /// </summary>
        /// <returns></returns>
        public IEnumerable<IGrouping<string, KeyValuePair<string, ApiBindInfo>>> GetGroupedCollection()
        {
            var apiGroups = ApiBindInfoCollection.Instance.GroupBy(z => z.Value.Category);
            return apiGroups;
        }
    }
}
