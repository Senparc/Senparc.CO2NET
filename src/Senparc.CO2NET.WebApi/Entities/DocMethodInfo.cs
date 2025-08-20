using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Senparc.CO2NET.WebApi
{
    public class DocMethodInfo
    {
        /// <summary>
        /// 初始化 DocMethodInfo
        /// </summary>
        /// <param name="methodName">方法名称</param>
        /// <param name="paramsPart">参数部分的完整字符串</param>
        /// <param name="summary">方法概要说明</param>
        /// <param name="parameters">参数字典，key: 参数名，value: 参数说明</param>
        /// <param name="returns">返回值说明</param>
        public DocMethodInfo(string methodName, string paramsPart, string summary = null, Dictionary<string, string> parameters = null, string returns = null)
        {
            MethodName = methodName?.Trim();
            ParamsPart = paramsPart?.Trim();
            Summary = summary?.Trim();
            Parameters = parameters ?? new Dictionary<string, string>();
            Returns = returns?.Trim();

            // 初始化其他属性
            IsAsync = CheckIsAsyncMethod(MethodName, ParamsPart);
            HasParameters = !string.IsNullOrEmpty(ParamsPart) && ParamsPart != "()";
            HasReturnValue = !string.IsNullOrEmpty(Returns);
            ParameterCount = Parameters?.Count ?? 0;
        }

        /// <summary>
        /// 方法名称
        /// </summary>
        public string MethodName { get; }

        /// <summary>
        /// 参数部分的完整字符串
        /// </summary>
        public string ParamsPart { get; }

        /// <summary>
        /// 方法概要说明
        /// </summary>
        public string Summary { get; }

        /// <summary>
        /// 参数字典，key: 参数名，value: 参数说明
        /// </summary>
        public Dictionary<string, string> Parameters { get; }

        /// <summary>
        /// 返回值说明
        /// </summary>
        public string Returns { get; }

        /// <summary>
        /// 是否为异步方法
        /// </summary>
        public bool IsAsync { get; }

        /// <summary>
        /// 是否包含参数
        /// </summary>
        public bool HasParameters { get; }

        /// <summary>
        /// 是否有返回值说明
        /// </summary>
        public bool HasReturnValue { get; }

        /// <summary>
        /// 参数数量
        /// </summary>
        public int ParameterCount { get; }

        /// <summary>
        /// 获取格式化后的方法签名
        /// </summary>
        /// <returns></returns>
        /// <summary>
        /// 获取合并后的参数信息字符串
        /// </summary>
        /// <param name="includeParamsPart">是否包含参数类型信息</param>
        /// <param name="includeDescription">是否包含参数描述</param>
        /// <returns>格式化后的参数信息</returns>
        public string GetMergedParameters(bool includeParamsPart = true, bool includeDescription = true)
        {
            if (!HasParameters)
            {
                return "()";
            }

            var sb = new StringBuilder();

            // 解析 ParamsPart，移除开头的 ( 和结尾的 )
            var paramTypes = ParamsPart.Trim('(', ')').Split(',')
                                     .Select(p => p.Trim())
                                     .ToList();

            // 获取参数名列表
            var paramNames = Parameters.Keys.ToList();

            // 确保参数数量匹配
            if (paramTypes.Count != paramNames.Count)
            {
                return ParamsPart; // 如果不匹配，返回原始的 ParamsPart
            }

            sb.Append('(');
            for (int i = 0; i < paramNames.Count; i++)
            {
                if (i > 0)
                {
                    sb.Append(", ");
                }

                var paramName = paramNames[i];
                var paramType = paramTypes[i];

                // 添加参数类型（如果需要）
                if (includeParamsPart)
                {
                    sb.Append(paramType).Append(' ');
                }

                // 添加参数名
                sb.Append(paramName);

                // 添加参数描述（如果需要）
                if (includeDescription && Parameters.ContainsKey(paramName))
                {
                    sb.Append(" /* ").Append(Parameters[paramName]).Append(" */");
                }
            }
            sb.Append(')');

            return sb.ToString();
        }

        /// <summary>
        /// 检查方法是否为异步方法
        /// </summary>
        /// <param name="methodName">方法名</param>
        /// <param name="paramsPart">参数部分</param>
        /// <returns></returns>
        private bool CheckIsAsyncMethod(string methodName, string paramsPart)
        {
            if (string.IsNullOrEmpty(methodName))
            {
                return false;
            }

            // 1. 检查方法名是否以Async结尾
            bool isAsyncByName = methodName.EndsWith("Async", StringComparison.OrdinalIgnoreCase);

            // 2. 检查方法名是否包含泛型异步标记
            bool isAsyncByGeneric = methodName.Contains("Async``", StringComparison.OrdinalIgnoreCase);

            // 3. 检查返回值类型是否为异步类型
            bool isAsyncByReturnType = false;
            if (!string.IsNullOrEmpty(Returns))
            {
                var asyncTypes = new[]
                {
                    "Task",
                    "Task<",
                    "ValueTask",
                    "ValueTask<",
                    "IAsyncEnumerable",
                    "IAsyncEnumerable<",
                    "System.Threading.Tasks.Task",
                    "System.Threading.Tasks.Task<",
                    "System.Threading.Tasks.ValueTask",
                    "System.Threading.Tasks.ValueTask<",
                    "System.Collections.Generic.IAsyncEnumerable",
                    "System.Collections.Generic.IAsyncEnumerable<"
                };

                isAsyncByReturnType = asyncTypes.Any(t => Returns.Contains(t, StringComparison.OrdinalIgnoreCase));
            }

            // 4. 检查参数中是否包含 CancellationToken（通常异步方法会有这个参数）
            bool hasCancellationToken = false;
            if (!string.IsNullOrEmpty(paramsPart))
            {
                hasCancellationToken = paramsPart.Contains("CancellationToken", StringComparison.OrdinalIgnoreCase) ||
                                     paramsPart.Contains("System.Threading.CancellationToken", StringComparison.OrdinalIgnoreCase);
            }

            // 返回综合判断结果
            return isAsyncByName || isAsyncByGeneric || isAsyncByReturnType || hasCancellationToken;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            
            // 添加方法签名
            sb.AppendLine($"Method: {MethodName}{ParamsPart}");
            
            // 添加异步标记
            if (IsAsync)
            {
                sb.AppendLine("Type: Async");
            }

            // 添加概要信息
            if (!string.IsNullOrEmpty(Summary))
            {
                sb.AppendLine($"Summary: {Summary}");
            }

            // 添加参数信息
            if (HasParameters)
            {
                sb.AppendLine($"Parameters ({ParameterCount}):");
                foreach (var param in Parameters)
                {
                    sb.AppendLine($"  - {param.Key}: {param.Value}");
                }
            }
            else
            {
                sb.AppendLine("Parameters: None");
            }

            // 添加返回值信息
            if (HasReturnValue)
            {
                sb.AppendLine($"Returns: {Returns}");
            }

            return sb.ToString().TrimEnd();
        }
    }
}
