/*----------------------------------------------------------------
    Copyright (C) 2026 Senparc

    文件名：Program.cs
    文件功能描述：验证 CO2NET System.Text.Json 源生成路径的 Native AOT 发布与运行


    创建标识：Senparc - 20260721

    修改标识：Senparc - 20260721
    修改描述：v1.0.0 新增 ToJson、GetObject 和缓存序列化 Native AOT 冒烟验证

----------------------------------------------------------------*/

using Senparc.CO2NET.Cache;
using Senparc.CO2NET.Extensions;
using Senparc.CO2NET.Helpers;
using System.Text.Json.Serialization;

var payload = new AotPayload(3314, "Native AOT");
var typeInfo = AotJsonContext.Default.AotPayload;

var json = payload.ToJson(typeInfo);
var fromJson = SerializerHelper.GetObject(typeInfo, json);

var helperJson = SerializerHelper.GetJsonString(typeInfo, payload);
var cacheJson = payload.SerializeToCache(typeInfo);
var fromCache = cacheJson.DeserializeFromCache(typeInfo);

if (fromJson != payload || fromCache != payload || helperJson != json || cacheJson != json)
{
    return 1;
}

Console.WriteLine(json);
return 0;

internal sealed record AotPayload(int Issue, string Mode);

[JsonSerializable(typeof(AotPayload))]
internal sealed partial class AotJsonContext : JsonSerializerContext
{
}
