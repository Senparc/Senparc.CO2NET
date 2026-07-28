using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Senparc.CO2NET.WebApi
{
    public class DocMethodInfo
    {
        /// <summary>
        /// Initializes DocMethodInfo
        /// </summary>
        /// <param name="methodName">Method name</param>
        /// <param name="paramsPart">Complete string of the parameter section</param>
        /// <param name="summary">Method summary description</param>
        /// <param name="parameters">Parameter dictionary, key: parameter name, value: parameter description</param>
        /// <param name="returns">Return value description</param>
        public DocMethodInfo(string methodName, string paramsPart, string summary = null, Dictionary<string, string> parameters = null, string returns = null)
        {
            MethodName = methodName?.Trim();
            ParamsPart = paramsPart?.Trim();
            Summary = summary?.Trim();
            Parameters = parameters ?? new Dictionary<string, string>();
            Returns = returns?.Trim();

            // Initialize other properties
            IsAsync = CheckIsAsyncMethod(MethodName, ParamsPart);
            HasParameters = !string.IsNullOrEmpty(ParamsPart) && ParamsPart != "()";
            HasReturnValue = !string.IsNullOrEmpty(Returns);
            ParameterCount = Parameters?.Count ?? 0;
        }

        /// <summary>
        /// Method name
        /// </summary>
        public string MethodName { get; }

        /// <summary>
        /// Complete string of the parameter section
        /// </summary>
        public string ParamsPart { get; }

        /// <summary>
        /// Method summary description
        /// </summary>
        public string Summary { get; }

        /// <summary>
        /// Parameter dictionary, key: parameter name, value: parameter description
        /// </summary>
        public Dictionary<string, string> Parameters { get; }

        /// <summary>
        /// Return value description
        /// </summary>
        public string Returns { get; }

        /// <summary>
        /// Whether the method is asynchronous
        /// </summary>
        public bool IsAsync { get; }

        /// <summary>
        /// Whether parameters are included
        /// </summary>
        public bool HasParameters { get; }

        /// <summary>
        /// Whether a return value description exists
        /// </summary>
        public bool HasReturnValue { get; }

        /// <summary>
        /// Parameter count
        /// </summary>
        public int ParameterCount { get; }

        /// <summary>
        /// Gets the formatted method signature
        /// </summary>
        /// <returns></returns>
        /// <summary>
        /// Gets the merged parameter information string
        /// </summary>
        /// <param name="includeParamsPart">Whether to include parameter type information</param>
        /// <param name="includeDescription">Whether to include parameter descriptions</param>
        /// <returns>Formatted parameter information</returns>
        public string GetMergedParameters(bool includeParamsPart = true, bool includeDescription = true)
        {
            if (!HasParameters)
            {
                return "()";
            }

            var sb = new StringBuilder();

            // Parse ParamsPart, remove leading ( and trailing )
            var paramTypes = ParamsPart.Trim('(', ')').Split(',')
                                     .Select(p => p.Trim())
                                     .ToList();

            // Get parameter name list
            var paramNames = Parameters.Keys.ToList();

            // Ensure parameter counts match
            if (paramTypes.Count != paramNames.Count)
            {
                return ParamsPart; // If they do not match, return the original ParamsPart
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

                // Add parameter type (if needed)
                if (includeParamsPart)
                {
                    sb.Append(paramType).Append(' ');
                }

                // Add parameter name
                sb.Append(paramName);

                // Add parameter description (if needed)
                if (includeDescription && Parameters.ContainsKey(paramName))
                {
                    sb.Append(" /* ").Append(Parameters[paramName]).Append(" */");
                }
            }
            sb.Append(')');

            return sb.ToString();
        }

        /// <summary>
        /// Checks whether the method is asynchronous
        /// </summary>
        /// <param name="methodName">Method name</param>
        /// <param name="paramsPart">Parameter section</param>
        /// <returns></returns>
        private bool CheckIsAsyncMethod(string methodName, string paramsPart)
        {
            if (string.IsNullOrEmpty(methodName))
            {
                return false;
            }

            // 1. Check whether the method name ends with Async
            bool isAsyncByName = methodName.EndsWith("Async", StringComparison.OrdinalIgnoreCase);

            // 2. Check whether the method name contains a generic async marker
            bool isAsyncByGeneric = methodName.Contains("Async``", StringComparison.OrdinalIgnoreCase);

            // 3. Check whether the return type is an async type
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

            // 4. Check whether parameters include CancellationToken (async methods usually have this parameter)
            bool hasCancellationToken = false;
            if (!string.IsNullOrEmpty(paramsPart))
            {
                hasCancellationToken = paramsPart.Contains("CancellationToken", StringComparison.OrdinalIgnoreCase) ||
                                     paramsPart.Contains("System.Threading.CancellationToken", StringComparison.OrdinalIgnoreCase);
            }

            // Return combined judgment result
            return isAsyncByName || isAsyncByGeneric || isAsyncByReturnType || hasCancellationToken;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            
            // Add method signature
            sb.AppendLine($"Method: {MethodName}{ParamsPart}");
            
            // Add async marker
            if (IsAsync)
            {
                sb.AppendLine("Type: Async");
            }

            // Add summary information
            if (!string.IsNullOrEmpty(Summary))
            {
                sb.AppendLine($"Summary: {Summary}");
            }

            // Add parameter information
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

            // Add return value information
            if (HasReturnValue)
            {
                sb.AppendLine($"Returns: {Returns}");
            }

            return sb.ToString().TrimEnd();
        }
    }
}
