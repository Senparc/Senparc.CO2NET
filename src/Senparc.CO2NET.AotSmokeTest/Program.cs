/*----------------------------------------------------------------
    Copyright (C) 2026 Senparc

    FileName: Program.cs
    File Function Description: Verify Native AOT publish and run for the CO2NET System.Text.Json source-generation path


    Creation Identifier: Senparc - 20260722

    Modification Identifier: Senparc - 20260721
    Modification Description: v1.0.0 Add Native AOT smoke tests for ToJson, GetObject, and cache serialization

    Modification Identifier: Senparc - 20260722
    Modification Description: v1.1.0 Cover type conversion, JSON DOM, cache, and source-generated HTTP Native AOT paths

----------------------------------------------------------------*/

using Microsoft.Extensions.DependencyInjection;
using Senparc.CO2NET.Cache;
using Senparc.CO2NET.Extensions;
using Senparc.CO2NET.Helpers;
using Senparc.CO2NET.Helpers.Serializers;
using Senparc.CO2NET.HttpUtility;
using System.Net;
using System.Text;
using System.Text.Json.Serialization;

var payload = new AotPayload(3314, "Native AOT");
var typeInfo = AotJsonContext.Default.AotPayload;

var json = payload.ToJson(typeInfo);
var fromJson = SerializerHelper.GetObject(typeInfo, json);

var helperJson = SerializerHelper.GetJsonString(typeInfo, payload);
var cacheJson = payload.SerializeToCache(typeInfo);
var fromCache = cacheJson.DeserializeFromCache(typeInfo);
var jsonNode = json.GetJsonNode();

var typePayload = new AotTypePayload(typeof(AotPayload));
var typePayloadInfo = AotJsonContext.Default.AotTypePayload;
SystemTypeJsonConverter.RegisterType<AotPayload>();
var typeJson = typePayload.SerializeToCache(typePayloadInfo);
var typeFromCache = typeJson.DeserializeFromCache(typePayloadInfo);

using var getServices = CreateHttpServices(json);
var fromGet = await Get.GetJsonAsync(typeInfo, getServices, "https://aot.senparc.test/get");

using var postServices = CreateHttpServices(json);
var fromPost = await Post.PostGetJsonAsync(
    typeInfo,
    postServices,
    "https://aot.senparc.test/post",
    formData: new Dictionary<string, string> { ["issue"] = "3314" });

if (fromJson != payload ||
    fromCache != payload ||
    fromGet != payload ||
    fromPost != payload ||
    helperJson != json ||
    cacheJson != json ||
    jsonNode?["Issue"]?.GetValue<int>() != 3314 ||
    typeFromCache.ValueType != typeof(AotPayload))
{
    return 1;
}

Console.WriteLine(json);
return 0;

static ServiceProvider CreateHttpServices(string json)
{
    var services = new ServiceCollection();
    services.AddSingleton(new SenparcHttpClient(new HttpClient(new StaticJsonHandler(json))));
    return services.BuildServiceProvider();
}

internal sealed record AotPayload(int Issue, string Mode);

[JsonSourceGenerationOptions(Converters = new[] { typeof(SystemTypeJsonConverter) })]
[JsonSerializable(typeof(AotPayload))]
[JsonSerializable(typeof(AotTypePayload))]
internal sealed partial class AotJsonContext : JsonSerializerContext
{
}

internal sealed record AotTypePayload(Type ValueType);

internal sealed class StaticJsonHandler(string json) : HttpMessageHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json"),
            RequestMessage = request
        };
        return Task.FromResult(response);
    }
}
