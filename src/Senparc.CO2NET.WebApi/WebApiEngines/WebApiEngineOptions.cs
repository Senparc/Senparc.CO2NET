using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Reflection;
using System.Text;

namespace Senparc.CO2NET.WebApi
{
    /// <summary>
    /// WebApiEngine 配置参数
    /// </summary>
    public class WebApiEngineOptions
    {
        /// <param name="defaultRequestMethod">默认请求方式（全局默认为 Post）</param>
        /// <param name="baseApiControllerType">全局 ApiController 的基类，默认为 ControllerBase</param>
        /// <param name="taskCount">同时执行线程数</param>
        /// <param name="showDetailApiLog">是否在控制台输出详细 API 创建日志</param>
        /// <param name="copyCustomAttributes">是否复制自定义特性</param>
        /// <param name="defaultAction">默认请求类型，如 Post，Get</param>
        /// <param name="additionalAttributeFunc">额外需要绑定的特性</param>
        /// <param name="forbiddenExternalAccess">是否允许外部访问，默认为 false，只允许本机访问自动生成的 WebApi</param>

        public WebApiEngineOptions(string docXmlPath = null, ApiRequestMethod defaultRequestMethod = ApiRequestMethod.Post, Type baseApiControllerType = null, bool copyCustomAttributes = true, int taskCount = 4, bool showDetailApiLog = false, Func<MethodInfo, IEnumerable<CustomAttributeBuilder>> additionalAttributeFunc = null, bool forbiddenExternalAccess = true)
        {
            DocXmlPath = docXmlPath;
            DefaultRequestMethod = defaultRequestMethod;
            BaseApiControllerType = baseApiControllerType;
            CopyCustomAttributes = copyCustomAttributes;
            TaskCount = taskCount;
            ShowDetailApiLog = showDetailApiLog;
            AdditionalAttributeFunc = additionalAttributeFunc;
            ForbiddenExternalAccess = forbiddenExternalAccess;
        }

        /// <summary>
        /// XML 文档路径
        /// </summary>
        public string DocXmlPath { get; set; }
        /// <summary>
        /// 默认请求方式（全局默认为 Post）
        /// </summary>
        public ApiRequestMethod DefaultRequestMethod { get; set; }
        /// <summary>
        /// 全局 ApiController 的基类，默认为 ControllerBase
        /// </summary>
        public Type BaseApiControllerType { get; set; }
        /// <summary>
        /// 是否复制自定义特性
        /// </summary>
        public bool CopyCustomAttributes { get; set; }
        /// <summary>
        /// 同时执行线程数
        /// </summary>
        public int TaskCount { get; set; }
        /// <summary>
        /// 是否在控制台输出详细 API 创建日志
        /// </summary>
        public bool ShowDetailApiLog { get; set; }
        /// <summary>
        /// 额外需要绑定的特性
        /// </summary>
        public Func<MethodInfo, IEnumerable<CustomAttributeBuilder>> AdditionalAttributeFunc { get; set; }
        /// <summary>
        /// 是否允许外部访问，默认为 false，只允许本机访问自动生成的 WebApi
        /// </summary>
        public bool ForbiddenExternalAccess { get; set; }
    }
}
