/*----------------------------------------------------------------
    Copyright (C) 2026 Senparc

    文件名：SystemTextJsonSerializer.cs
    文件功能描述：封装 System.Text.Json 序列化、旧配置适配和动态 JSON 转换


    创建标识：Senparc - 20260722

    修改标识：Senparc - 20260721
    修改描述：v4.0.0 新增 System.Text.Json 兼容实现和 JsonTypeInfo Native AOT 安全路径

    修改标识：Senparc - 20260722
    修改描述：v4.1.0 扩展旧设置映射与动态日期兼容行为

----------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace Senparc.CO2NET.Helpers.Serializers
{
    internal static class SystemTextJsonSerializer
    {
        internal static string Serialize(object data, bool indented = false, object settings = null)
        {
            var options = CreateOptions(settings, indented);
            return data == null
                ? JsonSerializer.Serialize<object>(null, options)
                : JsonSerializer.Serialize(data, data.GetType(), options);
        }

        internal static string Serialize<T>(T data, JsonTypeInfo<T> jsonTypeInfo)
        {
            if (jsonTypeInfo == null)
            {
                throw new ArgumentNullException(nameof(jsonTypeInfo));
            }

            return JsonSerializer.Serialize(data, jsonTypeInfo);
        }

        internal static string Serialize(object data, JsonTypeInfo jsonTypeInfo)
        {
            if (jsonTypeInfo == null)
            {
                throw new ArgumentNullException(nameof(jsonTypeInfo));
            }

            return JsonSerializer.Serialize(data, jsonTypeInfo);
        }

        internal static T Deserialize<T>(string json, object settings = null)
        {
            if (typeof(T) == typeof(object))
            {
                return (T)DeserializeDynamic(json, settings);
            }

            return JsonSerializer.Deserialize<T>(json, CreateOptions(settings));
        }

        internal static object Deserialize(string json, Type type = null, object settings = null)
        {
            if (type == null || type == typeof(object))
            {
                return DeserializeDynamic(json, settings);
            }

            return JsonSerializer.Deserialize(json, type, CreateOptions(settings));
        }

        internal static T Deserialize<T>(string json, JsonTypeInfo<T> jsonTypeInfo)
        {
            if (jsonTypeInfo == null)
            {
                throw new ArgumentNullException(nameof(jsonTypeInfo));
            }

            return JsonSerializer.Deserialize(json, jsonTypeInfo);
        }

        internal static object Deserialize(string json, JsonTypeInfo jsonTypeInfo)
        {
            if (jsonTypeInfo == null)
            {
                throw new ArgumentNullException(nameof(jsonTypeInfo));
            }

            return JsonSerializer.Deserialize(json, jsonTypeInfo);
        }

        internal static JsonSerializerOptions CreateOptions(object settings = null, bool? indented = null)
        {
            JsonSerializerOptions options;
            switch (settings)
            {
                case null:
                    options = JsonSerializerOptionsFactory.Create();
                    break;
                case JsonSerializerOptions jsonSerializerOptions:
                    options = new JsonSerializerOptions(jsonSerializerOptions);
                    break;
                case JsonSetting jsonSetting:
                    options = JsonSerializerOptionsFactory.Create(jsonSetting);
                    break;
                case JsonSettingWrap jsonSettingWrap:
                    options = new JsonSerializerOptions(jsonSettingWrap.Options);
                    break;
                default:
                    options = CreateLegacyNewtonsoftOptions(settings);
                    break;
            }

            if (indented.HasValue)
            {
                options.WriteIndented = indented.Value;
            }

            return options;
        }

        private static JsonSerializerOptions CreateLegacyNewtonsoftOptions(object settings)
        {
            var settingsType = settings.GetType();
            if (!IsNewtonsoftSettings(settingsType))
            {
                throw new ArgumentException(
                    $"Unsupported JSON settings type: {settingsType.FullName}. Use JsonSerializerOptions, JsonSetting, or the legacy Newtonsoft.Json.JsonSerializerSettings type.",
                    nameof(settings));
            }

            var options = JsonSerializerOptionsFactory.Create();
            var nullValueHandling = GetPropertyValue(settings, "NullValueHandling")?.ToString();
            var defaultValueHandling = GetPropertyValue(settings, "DefaultValueHandling")?.ToString();
            var referenceLoopHandling = GetPropertyValue(settings, "ReferenceLoopHandling")?.ToString();
            var preserveReferencesHandling = GetPropertyValue(settings, "PreserveReferencesHandling")?.ToString();
            var missingMemberHandling = GetPropertyValue(settings, "MissingMemberHandling")?.ToString();

            if (!string.IsNullOrEmpty(defaultValueHandling) && defaultValueHandling.Contains("Ignore"))
            {
                options.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault;
            }
            else if (string.Equals(nullValueHandling, "Ignore", StringComparison.Ordinal))
            {
                options.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
            }

            if (string.Equals(referenceLoopHandling, "Ignore", StringComparison.Ordinal))
            {
                options.ReferenceHandler = ReferenceHandler.IgnoreCycles;
            }
            else if (!string.IsNullOrEmpty(preserveReferencesHandling) &&
                     !string.Equals(preserveReferencesHandling, "None", StringComparison.Ordinal))
            {
                options.ReferenceHandler = ReferenceHandler.Preserve;
            }

#if NET8_0_OR_GREATER
            if (string.Equals(missingMemberHandling, "Error", StringComparison.Ordinal))
            {
                options.UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow;
            }
#endif

            if (GetPropertyValue(settings, "MaxDepth") is int maxDepth && maxDepth > 0)
            {
                options.MaxDepth = maxDepth;
            }

            var contractResolver = GetPropertyValue(settings, "ContractResolver");
            var namingStrategy = contractResolver == null ? null : GetPropertyValue(contractResolver, "NamingStrategy");
            var namingPolicy = GetNamingPolicy(namingStrategy?.GetType() ?? contractResolver?.GetType());
            if (namingPolicy != null)
            {
                options.PropertyNamingPolicy = namingPolicy;
                options.DictionaryKeyPolicy = namingPolicy;
            }

            if (GetPropertyValue(settings, "Converters") is System.Collections.IEnumerable converters)
            {
                foreach (var converter in converters)
                {
                    if (converter?.GetType().FullName == "Newtonsoft.Json.Converters.StringEnumConverter")
                    {
                        var converterNamingStrategy = GetPropertyValue(converter, "NamingStrategy");
                        var converterNamingPolicy = GetNamingPolicy(converterNamingStrategy?.GetType());
                        var allowIntegerValues = GetPropertyValue(converter, "AllowIntegerValues") as bool? ?? true;
                        options.Converters.Add(new JsonStringEnumConverter(converterNamingPolicy, allowIntegerValues));
                    }
                }
            }

            return options;
        }

        private static JsonNamingPolicy GetNamingPolicy(Type namingStrategyType)
        {
            var fullName = namingStrategyType?.FullName;
            if (string.IsNullOrEmpty(fullName))
            {
                return null;
            }

            if (fullName.Contains("CamelCase"))
            {
                return JsonNamingPolicy.CamelCase;
            }

#if NET8_0_OR_GREATER
            if (fullName.Contains("SnakeCase"))
            {
                return JsonNamingPolicy.SnakeCaseLower;
            }

            if (fullName.Contains("KebabCase"))
            {
                return JsonNamingPolicy.KebabCaseLower;
            }
#endif

            return null;
        }

        private static bool IsNewtonsoftSettings(Type type)
        {
            while (type != null)
            {
                if (type.FullName == "Newtonsoft.Json.JsonSerializerSettings")
                {
                    return true;
                }

                type = type.BaseType;
            }

            return false;
        }

        private static object GetPropertyValue(object instance, string propertyName)
        {
            return instance.GetType().GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public)?.GetValue(instance);
        }

        private static object DeserializeDynamic(string json, object settings = null)
        {
            var dateParseHandling = GetDynamicDateParseHandling(settings);
            using (var document = JsonDocument.Parse(json, new JsonDocumentOptions
            {
                AllowTrailingCommas = true,
                CommentHandling = JsonCommentHandling.Skip
            }))
            {
                return ConvertElement(document.RootElement, dateParseHandling);
            }
        }

        private static DynamicDateParseHandling GetDynamicDateParseHandling(object settings)
        {
            if (settings != null && IsNewtonsoftSettings(settings.GetType()))
            {
                var value = GetPropertyValue(settings, "DateParseHandling")?.ToString();
                if (string.Equals(value, "None", StringComparison.Ordinal))
                {
                    return DynamicDateParseHandling.None;
                }

                if (string.Equals(value, "DateTimeOffset", StringComparison.Ordinal))
                {
                    return DynamicDateParseHandling.DateTimeOffset;
                }
            }

            return DynamicDateParseHandling.DateTime;
        }

        private static object ConvertElement(JsonElement element, DynamicDateParseHandling dateParseHandling)
        {
            switch (element.ValueKind)
            {
                case JsonValueKind.Object:
                    IDictionary<string, object> expando = new ExpandoObject();
                    foreach (var property in element.EnumerateObject())
                    {
                        expando[property.Name] = ConvertElement(property.Value, dateParseHandling);
                    }
                    return expando;
                case JsonValueKind.Array:
                    var list = new List<object>();
                    foreach (var item in element.EnumerateArray())
                    {
                        list.Add(ConvertElement(item, dateParseHandling));
                    }
                    return list;
                case JsonValueKind.String:
                    if (dateParseHandling == DynamicDateParseHandling.DateTimeOffset && element.TryGetDateTimeOffset(out var dateTimeOffset))
                    {
                        return dateTimeOffset;
                    }
                    if (dateParseHandling == DynamicDateParseHandling.DateTime && element.TryGetDateTime(out var dateTime))
                    {
                        return dateTime;
                    }
                    return element.GetString();
                case JsonValueKind.Number:
                    if (element.TryGetInt64(out var integer))
                    {
                        return integer;
                    }
                    if (element.TryGetDecimal(out var decimalValue))
                    {
                        return decimalValue;
                    }
                    return element.GetDouble();
                case JsonValueKind.True:
                    return true;
                case JsonValueKind.False:
                    return false;
                case JsonValueKind.Null:
                case JsonValueKind.Undefined:
                    return null;
                default:
                    throw new JsonException($"Unsupported JSON token: {element.ValueKind}");
            }
        }

        private enum DynamicDateParseHandling
        {
            None,
            DateTime,
            DateTimeOffset
        }
    }
}
