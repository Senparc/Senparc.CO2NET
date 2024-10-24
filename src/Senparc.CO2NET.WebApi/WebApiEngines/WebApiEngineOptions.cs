using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Reflection;
using System.Text;

namespace Senparc.CO2NET.WebApi
{
    /// <summary>
    /// WebApiEngine configuration parameters
    /// </summary>
    public class WebApiEngineOptions
    {
        /// <param name="defaultRequestMethod">Default request method (global default is Post)</param>
        /// <param name="baseApiControllerType">Global ApiController base class, default is ControllerBase</param>
        /// <param name="taskCount">Number of concurrent threads</param>
        /// <param name="showDetailApiLog">Whether to output detailed API creation logs in the console</param>
        /// <param name="copyCustomAttributes">Whether to copy custom attributes</param>
        /// <param name="defaultAction">Default request type, such as Post, Get</param>
        /// <param name="additionalAttributeFunc">Additional attributes to bind</param>
        /// <param name="forbiddenExternalAccess">Whether to allow external access, default is false, only local access to auto-generated WebApi is allowed</param>
        /// <param name="addApiControllerAttribute">Whether to automatically add [ApiController] tag to auto-generated interface classes (Controller)</param>
        public WebApiEngineOptions(string docXmlPath = null, ApiRequestMethod defaultRequestMethod = ApiRequestMethod.Post, Type baseApiControllerType = null, bool copyCustomAttributes = true, int taskCount = 4, bool showDetailApiLog = false, Func<MethodInfo, IEnumerable<CustomAttributeBuilder>> additionalAttributeFunc = null, bool forbiddenExternalAccess = true, bool addApiControllerAttribute = true)
        {
            DocXmlPath = docXmlPath;
            DefaultRequestMethod = defaultRequestMethod;
            BaseApiControllerType = baseApiControllerType;
            CopyCustomAttributes = copyCustomAttributes;
            TaskCount = taskCount;
            ShowDetailApiLog = showDetailApiLog;
            AdditionalAttributeFunc = additionalAttributeFunc;
            ForbiddenExternalAccess = forbiddenExternalAccess;
            AddApiControllerAttribute = addApiControllerAttribute;
        }

        /// <summary>
        /// XML document path
        /// </summary>
        public string DocXmlPath { get; set; }
        /// <summary>
        /// Default request method (global default is Post)
        /// </summary>
        public ApiRequestMethod DefaultRequestMethod { get; set; }
        /// <summary>
        /// Global ApiController base class, default is ControllerBase
        /// </summary>
        public Type BaseApiControllerType { get; set; }
        /// <summary>
        /// Whether to copy custom attributes
        /// </summary>
        public bool CopyCustomAttributes { get; set; }
        /// <summary>
        /// Number of concurrent threads
        /// </summary>
        public int TaskCount { get; set; }
        /// <summary>
        /// Whether to output detailed API creation logs in the console
        /// </summary>
        public bool ShowDetailApiLog { get; set; }
        /// <summary>
        /// Additional attributes to bind
        /// </summary>
        public Func<MethodInfo, IEnumerable<CustomAttributeBuilder>> AdditionalAttributeFunc { get; set; }
        /// <summary>
        /// Whether to allow external access, default is false, only local access to auto-generated WebApi is allowed
        /// </summary>
        public bool ForbiddenExternalAccess { get; set; }
        public bool AddApiControllerAttribute { get; }
    }
}
