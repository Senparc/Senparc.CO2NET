using Senparc.CO2NET.Exceptions;
using Senparc.CO2NET.WebApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

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

        public string Name { get; private set; }

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

        public ApiBindInfo(ApiBindOn apiBindOn, string category, string globalName, string name, Type baseApiControllerType, short baseApiControllerOrder, ApiBindAttribute apiBindAttribute, MethodInfo methodInfo)
        {
            ApiBindOn = apiBindOn;
            Category = category;
            GlobalName = globalName;
            Name = name;
            BaseApiControllerType = baseApiControllerType;
            BaseApiControllerOrder = baseApiControllerOrder;
            ApiBindAttribute = apiBindAttribute;
            MethodInfo = methodInfo;
            ClassType = methodInfo.DeclaringType;
        }
    }
}
