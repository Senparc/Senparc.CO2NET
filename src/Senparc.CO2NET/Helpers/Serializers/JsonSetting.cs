#region Apache License Version 2.0
/*----------------------------------------------------------------

Copyright 2025 Suzhou Senparc Network Technology Co.,Ltd.

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
    Copyright (C) 2026 Senparc

    文件名：JsonSetting.cs
    文件功能描述：定义 Senparc JSON 配置、契约解析和兼容转换规则


    创建标识：Senparc - 20180602

    修改标识：Senparc - 20260721
    修改描述：v4.0.0 使用 System.Text.Json 重构 JSON 配置并兼容既有 Senparc 特性

----------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace Senparc.CO2NET.Helpers.Serializers
{
    /// <summary>
    /// Senparc JSON output settings. The type name and constructors are retained for compatibility;
    /// serialization is implemented by <see cref="System.Text.Json"/>.
    /// </summary>
    public class JsonSetting
    {
        /// <summary>
        /// Whether null properties should be omitted.
        /// </summary>
        public bool IgnoreNulls { get; set; }

        /// <summary>
        /// Property names whose null values should be omitted.
        /// </summary>
        public List<string> PropertiesToIgnoreNull { get; set; }

        /// <summary>
        /// Property types whose null values should be omitted.
        /// </summary>
        public List<Type> TypesToIgnoreNull { get; set; }

        public class IgnoreValueAttribute : System.ComponentModel.DefaultValueAttribute
        {
            public IgnoreValueAttribute(object value) : base(value)
            {
            }
        }

        public class IgnoreNullAttribute : Attribute
        {
        }

        /// <summary>
        /// Exception property marker retained for compatibility.
        /// </summary>
        public class ExcludedAttribute : Attribute
        {
        }

        /// <summary>
        /// Serialize the enum property as a string.
        /// </summary>
        public class EnumStringAttribute : Attribute
        {
        }

        /// <summary>
        /// JSON output settings constructor.
        /// </summary>
        public JsonSetting(bool ignoreNulls = false, List<string> propertiesToIgnoreNull = null, List<Type> typesToIgnoreNull = null)
        {
            IgnoreNulls = ignoreNulls;
            PropertiesToIgnoreNull = propertiesToIgnoreNull ?? new List<string>();
            TypesToIgnoreNull = typesToIgnoreNull ?? new List<Type>();
        }

        /// <summary>
        /// Creates equivalent <see cref="JsonSerializerOptions"/>.
        /// </summary>
        public JsonSerializerOptions ToJsonSerializerOptions()
        {
            return JsonSerializerOptionsFactory.Create(this);
        }

        public static implicit operator JsonSerializerOptions(JsonSetting jsonSetting)
        {
            return JsonSerializerOptionsFactory.Create(jsonSetting);
        }
    }

    /// <summary>
    /// Compatibility wrapper for the historical Senparc JSON settings type.
    /// </summary>
    public class JsonSettingWrap
    {
        public JsonSerializerOptions Options { get; }

        public JsonContractResolver ContractResolver { get; }

        public JsonSettingWrap() : this((JsonSetting)null)
        {
        }

        public JsonSettingWrap(JsonSetting jsonSetting)
        {
            ContractResolver = new JsonContractResolver(
                jsonSetting?.IgnoreNulls ?? false,
                jsonSetting?.PropertiesToIgnoreNull,
                jsonSetting?.TypesToIgnoreNull,
                jsonSetting != null);
            Options = JsonSerializerOptionsFactory.Create(jsonSetting, ContractResolver);
        }

        /// <summary>
        /// JSON output settings constructor. Priority: ignoreNulls &lt; propertiesToIgnoreNull &lt; typesToIgnoreNull.
        /// </summary>
        public JsonSettingWrap(bool ignoreNulls = false, List<string> propertiesToIgnoreNull = null, List<Type> typesToIgnoreNull = null)
            : this(new JsonSetting(ignoreNulls, propertiesToIgnoreNull, typesToIgnoreNull))
        {
        }

        public static implicit operator JsonSerializerOptions(JsonSettingWrap jsonSettingWrap)
        {
            return jsonSettingWrap?.Options;
        }
    }

    /// <summary>
    /// System.Text.Json contract resolver that preserves Senparc's historical filtering behavior
    /// and recognizes Newtonsoft attributes on existing DTOs without taking a Newtonsoft dependency.
    /// </summary>
    public class JsonContractResolver : IJsonTypeInfoResolver
    {
        private readonly DefaultJsonTypeInfoResolver _defaultResolver = new DefaultJsonTypeInfoResolver();
        private readonly bool _applySenparcAttributes;

        public bool IgnoreNulls { get; }

        public List<string> PropertiesToIgnoreNull { get; set; }

        public List<Type> TypesToIgnoreNull { get; set; }

        public JsonContractResolver(bool ignoreNulls = false, List<string> propertiesToIgnoreNull = null, List<Type> typesToIgnoreNull = null)
            : this(ignoreNulls, propertiesToIgnoreNull, typesToIgnoreNull, true)
        {
        }

        internal JsonContractResolver(bool ignoreNulls, List<string> propertiesToIgnoreNull, List<Type> typesToIgnoreNull, bool applySenparcAttributes)
        {
            IgnoreNulls = ignoreNulls;
            PropertiesToIgnoreNull = propertiesToIgnoreNull ?? new List<string>();
            TypesToIgnoreNull = typesToIgnoreNull ?? new List<Type>();
            _applySenparcAttributes = applySenparcAttributes;
        }

        public JsonTypeInfo GetTypeInfo(Type type, JsonSerializerOptions options)
        {
            var typeInfo = _defaultResolver.GetTypeInfo(type, options);
            if (typeInfo?.Kind == JsonTypeInfoKind.Object)
            {
                ModifyTypeInfo(typeInfo);
            }

            return typeInfo;
        }

        private void ModifyTypeInfo(JsonTypeInfo typeInfo)
        {
            foreach (var property in typeInfo.Properties)
            {
                var attributes = property.AttributeProvider?.GetCustomAttributes(false) ?? Array.Empty<object>();
                var memberName = (property.AttributeProvider as MemberInfo)?.Name ?? property.Name;
                var ignoreNull = IgnoreNulls || PropertiesToIgnoreNull.Contains(memberName) || TypesToIgnoreNull.Contains(property.PropertyType);
                object ignoredValue = null;
                var hasIgnoredValue = false;
                var ignoreProperty = false;

                foreach (var attribute in attributes)
                {
                    if (_applySenparcAttributes)
                    {
                        if (attribute is JsonSetting.IgnoreNullAttribute)
                        {
                            ignoreNull = true;
                        }
                        else if (attribute is JsonSetting.IgnoreValueAttribute ignoreValueAttribute)
                        {
                            ignoredValue = ignoreValueAttribute.Value;
                            hasIgnoredValue = true;
                        }
                        else if (attribute is JsonSetting.EnumStringAttribute)
                        {
                            property.CustomConverter = new JsonStringEnumConverter();
                        }
                    }

                    ApplyNewtonsoftAttributeCompatibility(property, attribute, ref ignoreNull, ref ignoreProperty, ref ignoredValue, ref hasIgnoredValue);
                }

                if (ignoreNull || hasIgnoredValue || ignoreProperty)
                {
                    var existingShouldSerialize = property.ShouldSerialize;
                    property.ShouldSerialize = (obj, value) =>
                        !ignoreProperty &&
                        (!ignoreNull || value != null) &&
                        (!hasIgnoredValue || !Equals(value, ignoredValue)) &&
                        (existingShouldSerialize == null || existingShouldSerialize(obj, value));
                }
            }
        }

        private static void ApplyNewtonsoftAttributeCompatibility(
            JsonPropertyInfo property,
            object attribute,
            ref bool ignoreNull,
            ref bool ignoreProperty,
            ref object ignoredValue,
            ref bool hasIgnoredValue)
        {
            var attributeType = attribute.GetType();
            switch (attributeType.FullName)
            {
                case "Newtonsoft.Json.JsonIgnoreAttribute":
                    ignoreProperty = true;
                    break;
                case "Newtonsoft.Json.JsonPropertyAttribute":
                    var propertyName = attributeType.GetProperty("PropertyName")?.GetValue(attribute) as string;
                    if (!string.IsNullOrEmpty(propertyName))
                    {
                        property.Name = propertyName;
                    }

                    if (string.Equals(attributeType.GetProperty("NullValueHandling")?.GetValue(attribute)?.ToString(), "Ignore", StringComparison.Ordinal))
                    {
                        ignoreNull = true;
                    }

                    var defaultValueHandling = attributeType.GetProperty("DefaultValueHandling")?.GetValue(attribute)?.ToString();
                    if (!string.IsNullOrEmpty(defaultValueHandling) && defaultValueHandling.Contains("Ignore"))
                    {
                        ignoredValue = property.PropertyType.IsValueType ? Activator.CreateInstance(property.PropertyType) : null;
                        hasIgnoredValue = true;
                    }

                    var order = attributeType.GetProperty("Order")?.GetValue(attribute);
                    if (order is int orderValue)
                    {
                        property.Order = orderValue;
                    }
                    break;
            }
        }
    }

    internal static class JsonSerializerOptionsFactory
    {
        internal static JsonSerializerOptions Create(JsonSetting jsonSetting = null, JsonContractResolver resolver = null)
        {
            resolver ??= new JsonContractResolver(
                jsonSetting?.IgnoreNulls ?? false,
                jsonSetting?.PropertiesToIgnoreNull,
                jsonSetting?.TypesToIgnoreNull,
                jsonSetting != null);

            var options = new JsonSerializerOptions
            {
                AllowTrailingCommas = true,
                DefaultIgnoreCondition = jsonSetting?.IgnoreNulls == true
                    ? JsonIgnoreCondition.WhenWritingNull
                    : JsonIgnoreCondition.Never,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                NumberHandling = JsonNumberHandling.AllowReadingFromString,
                PropertyNameCaseInsensitive = true,
                ReadCommentHandling = JsonCommentHandling.Skip,
                TypeInfoResolver = resolver
            };

            options.Converters.Add(new SystemTypeJsonConverter());
            return options;
        }
    }

    internal sealed class SystemTypeJsonConverter : JsonConverter<Type>
    {
        public override Type Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var typeName = reader.GetString();
            return string.IsNullOrEmpty(typeName) ? null : Type.GetType(typeName, throwOnError: false);
        }

        public override void Write(Utf8JsonWriter writer, Type value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value?.AssemblyQualifiedName);
        }
    }
}
