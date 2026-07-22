using Microsoft.VisualStudio.TestTools.UnitTesting;
using Senparc.CO2NET.Cache;
using Senparc.CO2NET.Extensions;
using Senparc.CO2NET.Helpers;
using Senparc.CO2NET.Helpers.Serializers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace Senparc.CO2NET.Tests.Helpers.Serializers
{
    [TestClass]
    public class SystemTextJsonCompatibilityTests
    {
        [TestMethod]
        public void ToJson_RetainsCommonCallShapes()
        {
            var payload = new CompatibilityPayload { Id = 1, Name = "Senparc" };

            Assert.AreEqual("{\"Id\":1,\"Name\":\"Senparc\"}", payload.ToJson());
            Assert.IsTrue(payload.ToJson(true).Contains(Environment.NewLine + "  \"Id\""));
            Assert.AreEqual("{\"Id\":1,\"Name\":\"Senparc\"}", payload.ToJson(indented: false, jsonSerializerSettings: null));

            var camelCase = payload.ToJson(new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
            Assert.AreEqual("{\"id\":1,\"name\":\"Senparc\"}", camelCase);
        }

        [TestMethod]
        public void LegacyMethodNamesAndNamedArgumentsRemainAvailable()
        {
            const string json = "{\"Id\":1,\"Name\":\"Senparc\"}";

            var result = json.GetObject<CompatibilityPayload>(settings: null);
            var runtimeResult = json.GetObject(typeof(CompatibilityPayload), settings: null);
            var positionalNullResult = json.GetObject<CompatibilityPayload>(null);
            dynamic dynamicResult = json.GetObject(null);

            Assert.AreEqual("Senparc", result.Name);
            Assert.IsInstanceOfType(runtimeResult, typeof(CompatibilityPayload));
            Assert.AreEqual("Senparc", positionalNullResult.Name);
            Assert.AreEqual("Senparc", dynamicResult.Name);
            Assert.IsTrue(typeof(ObjectExtensions).GetMethods().Any(method => method.Name == nameof(ObjectExtensions.ToJson)));
            Assert.IsTrue(typeof(SerializerHelper).GetMethods().Any(method => method.Name == nameof(SerializerHelper.GetJsonString)));
            Assert.IsTrue(typeof(SerializerHelper).GetMethods().Any(method => method.Name == nameof(SerializerHelper.GetObject)));
            Assert.IsTrue(typeof(CacheSerializeExtension).GetMethods().Any(method => method.Name == nameof(CacheSerializeExtension.SerializeToCache)));
            Assert.IsTrue(typeof(CacheSerializeExtension).GetMethods().Any(method => method.Name == nameof(CacheSerializeExtension.DeserializeFromCache)));
        }

        [TestMethod]
        public void CoreAssemblyDoesNotReferenceNewtonsoftJson()
        {
            var references = typeof(ObjectExtensions).Assembly.GetReferencedAssemblies();

            Assert.IsFalse(references.Any(reference => reference.Name == "Newtonsoft.Json"));
        }

        [TestMethod]
        public void LegacyNewtonsoftSettingsObjectIsAdaptedWithoutCoreDependency()
        {
            var settings = new Newtonsoft.Json.JsonSerializerSettings
            {
                ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver(),
                NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore
            };
            var payload = new CompatibilityPayload { Id = 1, Name = null };

            var json = payload.ToJson(jsonSerializerSettings: settings);
            var result = json.GetObject<CompatibilityPayload>(settings: settings);

            Assert.AreEqual("{\"id\":1}", json);
            Assert.AreEqual(1, result.Id);
        }

        [TestMethod]
        public void LegacyNewtonsoftSettings_RetainCommonPolicies()
        {
            var settings = new Newtonsoft.Json.JsonSerializerSettings
            {
                ContractResolver = new DefaultContractResolver
                {
                    NamingStrategy = new SnakeCaseNamingStrategy()
                },
                MissingMemberHandling = Newtonsoft.Json.MissingMemberHandling.Error,
                Converters =
                {
                    new Newtonsoft.Json.Converters.StringEnumConverter(new CamelCaseNamingStrategy(), allowIntegerValues: false)
                }
            };
            var payload = new NamingPayload { DisplayName = "Senparc", State = PayloadState.ReadyState };

            var json = payload.ToJson(jsonSerializerSettings: settings);

            Assert.AreEqual("{\"display_name\":\"Senparc\",\"state\":\"readyState\"}", json);
            AssertJsonException(() => "{\"display_name\":\"Senparc\",\"unknown\":1}".GetObject<NamingPayload>(settings));
        }

        [TestMethod]
        public void LegacyNewtonsoftAttributes_RetainOptInPrivateAndRequiredBehavior()
        {
            var payload = LegacyAttributedPayload.Create("Senparc", "private", "ignored");
            var newJson = payload.ToJson();
            var oldJson = Newtonsoft.Json.JsonConvert.SerializeObject(payload);

            Assert.IsTrue(JToken.DeepEquals(JToken.Parse(oldJson), JToken.Parse(newJson)));
            Assert.IsFalse(newJson.Contains(nameof(LegacyAttributedPayload.NotOptedIn)));
            Assert.IsFalse(newJson.Contains(nameof(LegacyAttributedPayload.Ignored)));

            var result = newJson.GetObject<LegacyAttributedPayload>();
            Assert.AreEqual("Senparc", result.Name);
            Assert.AreEqual("private", result.GetPrivateValue());
            Assert.IsTrue(result.HasExtensionValue("legacy_extra"));
            AssertJsonException(() => "{\"private_value\":\"private\"}".GetObject<LegacyAttributedPayload>());
        }

        [TestMethod]
        public void JsonSetting_RetainsFilteringAndEnumBehavior()
        {
            var payload = new FilterPayload
            {
                IgnoredValue = "IGNORE",
                NullValue = null,
                State = PayloadState.Ready
            };

            var json = SerializerHelper.GetJsonString(payload, new JsonSetting(false));

            Assert.AreEqual("{\"State\":\"Ready\"}", json);
        }

        [TestMethod]
        public void GetObject_RetainsDynamicPropertyAccess()
        {
            dynamic result = SerializerHelper.GetObject("{\"msg\":\"ok\",\"count\":2}");

            Assert.AreEqual("ok", result.msg);
            Assert.AreEqual(2L, result.count);
        }

        [TestMethod]
        public void GetObject_RetainsLegacyDynamicDateParsingAndSupportsOfficialDom()
        {
            const string json = "{\"timestamp\":\"2026-07-22T12:34:56Z\",\"items\":[1,2]}";

            dynamic defaultResult = SerializerHelper.GetObject(json);
            dynamic noDateResult = SerializerHelper.GetObject(json, new Newtonsoft.Json.JsonSerializerSettings
            {
                DateParseHandling = Newtonsoft.Json.DateParseHandling.None
            });
            var node = json.GetJsonNode();
            using var document = json.GetJsonDocument();

            Assert.IsInstanceOfType(defaultResult.timestamp, typeof(DateTime));
            Assert.IsInstanceOfType(noDateResult.timestamp, typeof(string));
            Assert.AreEqual(2, node["items"].AsArray().Count);
            Assert.AreEqual("2026-07-22T12:34:56Z", document.RootElement.GetProperty("timestamp").GetString());
        }

        [TestMethod]
        public void CacheSerialization_RoundTripsSystemType()
        {
            var payload = new TypePayload { ValueType = typeof(CompatibilityPayload) };

            var json = payload.SerializeToCache();
            var result = json.DeserializeFromCache<TypePayload>();

            Assert.AreEqual(typeof(CompatibilityPayload), result.ValueType);
        }

        [TestMethod]
        public void CacheSerialization_ReadsOldNewtonsoftPayloadsAndRemainsReadableByNewtonsoft()
        {
            var payload = new LegacyCachePayload
            {
                Id = 3314,
                Name = "Native AOT",
                CreatedAt = new DateTime(2026, 7, 22, 12, 34, 56, DateTimeKind.Utc)
            };

            var oldJson = Newtonsoft.Json.JsonConvert.SerializeObject(payload);
            var readBySystemTextJson = oldJson.DeserializeFromCache<LegacyCachePayload>();

            var newJson = payload.SerializeToCache();
            var readByNewtonsoft = Newtonsoft.Json.JsonConvert.DeserializeObject<LegacyCachePayload>(newJson);

            Assert.AreEqual(payload.Id, readBySystemTextJson.Id);
            Assert.AreEqual(payload.Name, readBySystemTextJson.Name);
            Assert.AreEqual(payload.CreatedAt, readBySystemTextJson.CreatedAt);
            Assert.AreEqual(payload.Id, readByNewtonsoft.Id);
            Assert.AreEqual(payload.Name, readByNewtonsoft.Name);
            Assert.AreEqual(payload.CreatedAt, readByNewtonsoft.CreatedAt);
        }

        [TestMethod]
        public void SourceGeneratedOverloads_RoundTripWithoutOptionsReflection()
        {
            var payload = new AotCompatibilityPayload(3314, "Native AOT");
            var typeInfo = CompatibilityJsonContext.Default.AotCompatibilityPayload;

            var json = payload.ToJson(typeInfo);
            var result = SerializerHelper.GetObject(typeInfo, json);

            Assert.AreEqual(payload, result);
            Assert.AreEqual(json, SerializerHelper.GetJsonString(typeInfo, payload));
            Assert.AreEqual(payload, json.DeserializeFromCache(typeInfo));
        }

        [TestMethod]
        public void SourceGeneratedOverloads_RoundTripSystemTypeWithPublicConverter()
        {
            var payload = new TypePayload { ValueType = typeof(CompatibilityPayload) };
            var typeInfo = CompatibilityJsonContext.Default.TypePayload;

            var json = payload.ToJson(typeInfo);
            var result = SerializerHelper.GetObject(typeInfo, json);

            Assert.AreEqual(typeof(CompatibilityPayload), result.ValueType);
        }

        public class CompatibilityPayload
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        public class FilterPayload
        {
            [JsonSetting.IgnoreValue("IGNORE")]
            public string IgnoredValue { get; set; }

            [JsonSetting.IgnoreNull]
            public string NullValue { get; set; }

            [JsonSetting.EnumString]
            public PayloadState State { get; set; }
        }

        public class TypePayload
        {
            public Type ValueType { get; set; }
        }

        public class NamingPayload
        {
            public string DisplayName { get; set; }

            public PayloadState State { get; set; }
        }

        [Newtonsoft.Json.JsonObject(Newtonsoft.Json.MemberSerialization.OptIn)]
        public class LegacyAttributedPayload
        {
            [Newtonsoft.Json.JsonProperty("legacy_name", Order = 1, Required = Newtonsoft.Json.Required.Always)]
            public string Name { get; set; }

            [Newtonsoft.Json.JsonProperty("private_value", Order = 2)]
            private string PrivateValue { get; set; }

            [Newtonsoft.Json.JsonExtensionData]
            private Dictionary<string, object> ExtensionData { get; set; } = new Dictionary<string, object>();

            [Newtonsoft.Json.JsonIgnore]
            public string Ignored { get; set; }

            public string NotOptedIn { get; set; }

            public string GetPrivateValue() => PrivateValue;

            public bool HasExtensionValue(string name) => ExtensionData.ContainsKey(name);

            public static LegacyAttributedPayload Create(string name, string privateValue, string ignored)
            {
                return new LegacyAttributedPayload
                {
                    Name = name,
                    PrivateValue = privateValue,
                    Ignored = ignored,
                    NotOptedIn = "not-opted-in",
                    ExtensionData = new Dictionary<string, object> { ["legacy_extra"] = 42 }
                };
            }
        }

        public class LegacyCachePayload
        {
            public int Id;

            public string Name { get; set; }

            public DateTime CreatedAt { get; set; }
        }

        public enum PayloadState
        {
            Ready,
            ReadyState
        }

        private static void AssertJsonException(Action action)
        {
            try
            {
                action();
                Assert.Fail("Expected System.Text.Json.JsonException.");
            }
            catch (JsonException)
            {
            }
        }
    }

    public sealed record AotCompatibilityPayload(int Issue, string Mode);

    [JsonSourceGenerationOptions(Converters = new[] { typeof(SystemTypeJsonConverter) })]
    [JsonSerializable(typeof(AotCompatibilityPayload))]
    [JsonSerializable(typeof(SystemTextJsonCompatibilityTests.TypePayload))]
    internal sealed partial class CompatibilityJsonContext : JsonSerializerContext
    {
    }
}
