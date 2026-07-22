using Microsoft.VisualStudio.TestTools.UnitTesting;
using Senparc.CO2NET.Cache;
using Senparc.CO2NET.Extensions;
using Senparc.CO2NET.Helpers;
using Senparc.CO2NET.Helpers.Serializers;
using System;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

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
        public void CacheSerialization_RoundTripsSystemType()
        {
            var payload = new TypePayload { ValueType = typeof(CompatibilityPayload) };

            var json = payload.SerializeToCache();
            var result = json.DeserializeFromCache<TypePayload>();

            Assert.AreEqual(typeof(CompatibilityPayload), result.ValueType);
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

        public enum PayloadState
        {
            Ready
        }
    }

    public sealed record AotCompatibilityPayload(int Issue, string Mode);

    [JsonSerializable(typeof(AotCompatibilityPayload))]
    internal sealed partial class CompatibilityJsonContext : JsonSerializerContext
    {
    }
}
