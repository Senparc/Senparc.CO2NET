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

    修改标识：Senparc - 20260722
    修改描述：v4.1.0 扩展 Newtonsoft 属性、公开字段、私有成员和 System.Type AOT 兼容

----------------------------------------------------------------*/

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
#if NET8_0_OR_GREATER
using System.Diagnostics.CodeAnalysis;
#endif

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
            AddNewtonsoftAttributedMembers(typeInfo);
            ApplyNewtonsoftConstructorCompatibility(typeInfo);

            var memberSerializationOptIn = UsesNewtonsoftOptIn(typeInfo.Type);
            for (var index = typeInfo.Properties.Count - 1; index >= 0; index--)
            {
                var property = typeInfo.Properties[index];
                var attributes = property.AttributeProvider?.GetCustomAttributes(false) ?? Array.Empty<object>();
                var memberName = (property.AttributeProvider as MemberInfo)?.Name ?? property.Name;
                var ignoreNull = IgnoreNulls || PropertiesToIgnoreNull.Contains(memberName) || TypesToIgnoreNull.Contains(property.PropertyType);
                object ignoredValue = null;
                var hasIgnoredValue = false;
                var ignoreProperty = false;
                var includedByNewtonsoft = false;

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

                    ApplyNewtonsoftAttributeCompatibility(
                        property,
                        attribute,
                        ref ignoreNull,
                        ref ignoreProperty,
                        ref ignoredValue,
                        ref hasIgnoredValue,
                        ref includedByNewtonsoft);
                }

                if (memberSerializationOptIn && !includedByNewtonsoft)
                {
                    ignoreProperty = true;
                }

                if (hasIgnoredValue)
                {
                    foreach (var attribute in attributes)
                    {
                        if (attribute is DefaultValueAttribute defaultValueAttribute)
                        {
                            ignoredValue = defaultValueAttribute.Value;
                            break;
                        }
                    }
                }

                if (ignoreProperty)
                {
                    typeInfo.Properties.RemoveAt(index);
                    continue;
                }

                if (ignoreNull || hasIgnoredValue)
                {
                    var existingShouldSerialize = property.ShouldSerialize;
                    property.ShouldSerialize = (obj, value) =>
                        (!ignoreNull || value != null) &&
                        (!hasIgnoredValue || !Equals(value, ignoredValue)) &&
                        (existingShouldSerialize == null || existingShouldSerialize(obj, value));
                }
            }
        }

        private static void AddNewtonsoftAttributedMembers(JsonTypeInfo typeInfo)
        {
            for (var currentType = typeInfo.Type; currentType != null && currentType != typeof(object); currentType = currentType.BaseType)
            {
                const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;
                foreach (var member in currentType.GetMembers(flags))
                {
                    if (!(member is PropertyInfo) && !(member is FieldInfo))
                    {
                        continue;
                    }

                    var includedByNewtonsoft =
                        HasAttribute(member, "Newtonsoft.Json.JsonPropertyAttribute") ||
                        HasAttribute(member, "Newtonsoft.Json.JsonRequiredAttribute") ||
                        HasAttribute(member, "Newtonsoft.Json.JsonExtensionDataAttribute");
                    if (!includedByNewtonsoft || ContainsMember(typeInfo, member))
                    {
                        continue;
                    }

                    var property = CreateJsonPropertyInfo(typeInfo, member);
                    if (property != null)
                    {
                        typeInfo.Properties.Add(property);
                    }
                }
            }
        }

        private static JsonPropertyInfo CreateJsonPropertyInfo(JsonTypeInfo typeInfo, MemberInfo member)
        {
            Type memberType;
            Func<object, object> getter = null;
            Action<object, object> setter = null;

            if (member is PropertyInfo propertyInfo)
            {
                if (propertyInfo.GetIndexParameters().Length != 0)
                {
                    return null;
                }

                memberType = propertyInfo.PropertyType;
                if (propertyInfo.GetMethod != null)
                {
                    getter = instance => propertyInfo.GetValue(instance);
                }
                if (propertyInfo.SetMethod != null)
                {
                    setter = (instance, value) => propertyInfo.SetValue(instance, value);
                }
            }
            else
            {
                var fieldInfo = (FieldInfo)member;
                memberType = fieldInfo.FieldType;
                getter = instance => fieldInfo.GetValue(instance);
                if (!fieldInfo.IsInitOnly)
                {
                    setter = (instance, value) => fieldInfo.SetValue(instance, value);
                }
            }

            var jsonProperty = typeInfo.CreateJsonPropertyInfo(memberType, member.Name);
            jsonProperty.AttributeProvider = member;
            jsonProperty.Get = getter;
            jsonProperty.Set = setter;
            return jsonProperty;
        }

        private static bool ContainsMember(JsonTypeInfo typeInfo, MemberInfo member)
        {
            foreach (var property in typeInfo.Properties)
            {
                if (Equals(property.AttributeProvider, member))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool UsesNewtonsoftOptIn(Type type)
        {
            foreach (var attribute in type.GetCustomAttributes(true))
            {
                var attributeType = attribute.GetType();
                if (attributeType.FullName == "Newtonsoft.Json.JsonObjectAttribute" &&
                    string.Equals(attributeType.GetProperty("MemberSerialization")?.GetValue(attribute)?.ToString(), "OptIn", StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }

        private static void ApplyNewtonsoftConstructorCompatibility(JsonTypeInfo typeInfo)
        {
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            foreach (var constructor in typeInfo.Type.GetConstructors(flags))
            {
                if (constructor.GetParameters().Length == 0 && HasAttribute(constructor, "Newtonsoft.Json.JsonConstructorAttribute"))
                {
                    typeInfo.CreateObject = () => constructor.Invoke(null);
                    return;
                }
            }
        }

        private static bool HasAttribute(MemberInfo member, string attributeFullName)
        {
            foreach (var attribute in member.GetCustomAttributes(false))
            {
                if (attribute.GetType().FullName == attributeFullName)
                {
                    return true;
                }
            }

            return false;
        }

        private static void ApplyNewtonsoftAttributeCompatibility(
            JsonPropertyInfo property,
            object attribute,
            ref bool ignoreNull,
            ref bool ignoreProperty,
            ref object ignoredValue,
            ref bool hasIgnoredValue,
            ref bool includedByNewtonsoft)
        {
            var attributeType = attribute.GetType();
            switch (attributeType.FullName)
            {
                case "Newtonsoft.Json.JsonIgnoreAttribute":
                    ignoreProperty = true;
                    break;
                case "Newtonsoft.Json.JsonPropertyAttribute":
                    includedByNewtonsoft = true;
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

                    var required = attributeType.GetProperty("Required")?.GetValue(attribute)?.ToString();
                    if (string.Equals(required, "Always", StringComparison.Ordinal) ||
                        string.Equals(required, "AllowNull", StringComparison.Ordinal))
                    {
                        property.IsRequired = true;
                    }
                    break;
                case "Newtonsoft.Json.JsonRequiredAttribute":
                    includedByNewtonsoft = true;
                    property.IsRequired = true;
                    break;
                case "Newtonsoft.Json.JsonExtensionDataAttribute":
                    includedByNewtonsoft = true;
                    property.IsExtensionData = SupportsSystemTextJsonExtensionData(property.PropertyType);
                    break;
            }
        }

        private static bool SupportsSystemTextJsonExtensionData(Type type)
        {
            if (type == typeof(IDictionary<string, object>) || type == typeof(IDictionary<string, JsonElement>))
            {
                return true;
            }

            foreach (var interfaceType in type.GetInterfaces())
            {
                if (!interfaceType.IsGenericType || interfaceType.GetGenericTypeDefinition() != typeof(IDictionary<,>))
                {
                    continue;
                }

                var arguments = interfaceType.GetGenericArguments();
                if (arguments[0] == typeof(string) && (arguments[1] == typeof(object) || arguments[1] == typeof(JsonElement)))
                {
                    return true;
                }
            }

            return false;
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
                IncludeFields = true,
                TypeInfoResolver = resolver
            };

            options.Converters.Add(new SystemTypeJsonConverter());
            return options;
        }
    }

    /// <summary>
    /// Serializes <see cref="Type"/> values by assembly-qualified name. Source-generated
    /// contexts can register this converter to keep existing cache payloads AOT-compatible.
    /// </summary>
    public sealed class SystemTypeJsonConverter : JsonConverter<Type>
    {
        private static readonly ConcurrentDictionary<string, Type> KnownTypes = new ConcurrentDictionary<string, Type>(StringComparer.Ordinal);

        /// <summary>
        /// Register a type that may appear in cached JSON. Generic registration keeps the type visible to Native AOT trimming.
        /// </summary>
        public static void RegisterType<T>()
        {
            RegisterType(typeof(T));
        }

        /// <summary>
        /// Register a type that may appear in cached JSON.
        /// </summary>
        public static void RegisterType(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            if (!string.IsNullOrEmpty(type.AssemblyQualifiedName))
            {
                KnownTypes[type.AssemblyQualifiedName] = type;
            }
            if (!string.IsNullOrEmpty(type.FullName))
            {
                KnownTypes[type.FullName] = type;
            }
        }

#if NET8_0_OR_GREATER
        [UnconditionalSuppressMessage("Trimming", "IL2057", Justification = "Native AOT callers can register preserved types with RegisterType<T>(); Type.GetType remains as the backward-compatible fallback for existing cache values.")]
#endif
        public override Type Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var typeName = reader.GetString();
            if (string.IsNullOrEmpty(typeName))
            {
                return null;
            }

            return KnownTypes.TryGetValue(typeName, out var knownType)
                ? knownType
                : Type.GetType(typeName, throwOnError: false);
        }

        public override void Write(Utf8JsonWriter writer, Type value, JsonSerializerOptions options)
        {
            if (value != null)
            {
                RegisterType(value);
            }
            writer.WriteStringValue(value?.AssemblyQualifiedName);
        }
    }
}
