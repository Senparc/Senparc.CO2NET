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
    
    FileName：ApiBindInfo.cs
    File Function Description：Method information bound by the ApiBind attribute (Mapping)
    
    
    Creation Identifier：Senparc - 20210627
    
    Modification Identifier：Senparc - 20241119
    Modification Description：v3.0.0-beta3 reconstruction

----------------------------------------------------------------*/


using Senparc.CO2NET.Exceptions;
using Senparc.CO2NET.WebApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace Senparc.CO2NET
{
    /// <summary>
    /// Method information bound by the ApiBind attribute (Mapping)
    /// </summary>
    public class ApiBindInfo
    {
        /// <summary>
        /// ApiBindAttribute
        /// </summary>
        public ApiBindAttribute ApiBindAttribute { get; set; }

        /// <summary>
        /// Group directory
        /// </summary>
        public string Category { get; private set; }
        /// <summary>
        /// Unique name under a single group. Default: [ClassName].[MethodName]
        /// </summary>
        public string GlobalName { get; private set; }
        /// <summary>
        /// Unique name: $"{methodInfo.DeclaringType.Name}.{methodInfo.Name}"
        /// </summary>
        public string ApiBindAttrName { get; private set; }

        /// <summary>
        /// Base class of ApiController, default is ControllerBase
        /// </summary>
        public Type BaseApiControllerType { get; set; }
        /// <summary>
        /// Base class order of ApiController, the one with the largest number will be used (supports negative numbers)
        /// </summary>
        public short BaseApiControllerOrder { get; set; }

        ///// <summary>
        ///// Bind API method object information
        ///// </summary>
        //public MethodInfo MethodInfo { get; set; }

        /// <summary>
        /// Bind the entire API method
        /// </summary>
        public Type ClassType { get; set; }

        /// <summary>
        /// Bind API method object information
        /// </summary>
        public MethodInfo MethodInfo { get; set; }

        /// <summary>
        /// Scope of ApiBind attribute: class or method
        /// </summary>
        public ApiBindOn ApiBindOn { get; set; }

        //TODO: Add ignore attribute
        //TODO: Can be ignored or enabled based on the module

        #region ApiPath parameters

        public string MethodName { get; set; }
        /// <summary>
        /// {methodInfo.DeclaringType.Name} e.g. TestAppService
        /// </summary>
        public string ApiBindName { get; set; }
        /// <summary>
        /// {methodInfo.Name} e.g. MyFunction
        /// </summary>
        public string ApiName { get; set; }
        /// <summary>
        /// e.g. TestAppService
        /// </summary>
        public string ControllerKeyName { get; set; }
        #endregion

        /// <summary>
        /// Match keywords to be replaced from apiBindInfo.Value.GlobalName
        /// </summary>
        private static readonly Regex regexForMethodName = new Regex(@"[\.\-/:]", RegexOptions.Compiled);


        public ApiBindInfo(ApiBindOn apiBindOn, string category, string globalName, string apiBindAttrName, Type baseApiControllerType, short baseApiControllerOrder, ApiBindAttribute apiBindAttribute, MethodInfo methodInfo)
        {
            ApiBindOn = apiBindOn;
            Category = category;
            GlobalName = globalName;
            ApiBindAttrName = apiBindAttrName;
            BaseApiControllerType = baseApiControllerType;
            BaseApiControllerOrder = baseApiControllerOrder;
            ApiBindAttribute = apiBindAttribute;
            MethodInfo = methodInfo;
            ClassType = methodInfo.DeclaringType;

            #region ApiPath parameters

            MethodName = regexForMethodName.Replace(GlobalName, "_");
            ApiBindName = ApiBindAttrName.Split('.')[0];
            var indexOfApiGroupDot = GlobalName.IndexOf(".");
            ApiName = GlobalName.Substring(indexOfApiGroupDot + 1, GlobalName.Length - indexOfApiGroupDot - 1);
            ControllerKeyName = GetControllerKeyName(category);

            #endregion
        }

        public static string GetControllerKeyName(string category)
        {
            return category.Replace(":", "_");//Do not change the rules arbitrarily, global consistency is required
        }


        //public string GetApiPath(string showStaticApiState)
        //{
        //    var apiBindGroupNamePath = ApiBindName.Replace(":", "_");
        //    var apiNamePath = ApiName.Replace(":", "_");
        //    var apiPath = $"/api/{ControllerKeyName}/{apiBindGroupNamePath}/{apiNamePath}{showStaticApiState}";
        //    return apiPath;
        //}
    }
}
