# System.Text.Json 与 Native AOT 迁移说明

Senparc.CO2NET 4.x 的 JSON 实现已切换到 `System.Text.Json`。本次 Native AOT 支持面向 .NET 8 及以上项目。

## 模块 AOT 兼容矩阵

| 模块 | AOT 级别 | 说明 |
|------|----------|------|
| `Senparc.CO2NET` | **Ready** | `IsAotCompatible`（net8.0）；须使用 `JsonTypeInfo` 源生成重载；`autoScanExtensionCacheStrategies` 必须为 `false` |
| `Senparc.CO2NET.AspNet` | **Ready** | 透传核心注册；同样禁止 AutoScan |
| `Senparc.CO2NET.APM` | **Ready** | 已含 net8.0；Windows `PerformanceCounter` 路径仅 net462 |
| `Senparc.CO2NET.MagicObject` | **Conditional** | 含 net8.0；`MemberwiseClone`/属性还原仍依赖反射 |
| `Senparc.CO2NET.Cache.Redis` | **Conditional** | 已移除 BinaryFormatter；对象缓存默认反射 JSON，AOT 请用 `JsonTypeInfo` 重载 |
| `Senparc.CO2NET.Cache.Redis.RedLock` | **Ready** | net8.0 + 分析器；无 JSON 反射主路径 |
| `Senparc.CO2NET.Cache.Dapr` | **Conditional** | 依赖 `Dapr.Client` 运行时泛型序列化，需自行验证上游 AOT |
| `Senparc.CO2NET.Cache.CsRedis` | **No** | 上游 `CSRedisCore` 传递 Newtonsoft.Json |
| `Senparc.CO2NET.Cache.Memcached` | **No** | 上游 `EnyimMemcachedCore` 传递 Newtonsoft.Json |
| `Senparc.CO2NET.WebApi` | **No** | `Reflection.Emit` + 程序集扫描 + Swashbuckle/MCP；AOT 宿主请用手写 Minimal API/控制器 |

严格 Native AOT 推荐组合：`Senparc.CO2NET` + `Senparc.CO2NET.AspNet`（可选 APM）+ `Cache.Redis`/`RedLock`（配合 `JsonTypeInfo`）。

## 向下兼容范围

- 保留 `ToJson`、`GetJsonString`、`GetObject`、`SerializeToCache`、`DeserializeFromCache` 及原有 HTTP JSON 方法。
- 旧 `JsonSerializerSettings` 对象仍可传给原参数位置；常用的 null/default 忽略、命名策略、枚举、深度、缺少成员、循环和引用保留设置会转换为 `JsonSerializerOptions`。
- 兼容常用 Newtonsoft `JsonIgnore`、`JsonProperty`、`JsonRequired`、`JsonExtensionData`、`JsonObject(MemberSerialization.OptIn)` 和无参 `JsonConstructor` 标记。
- 旧 Newtonsoft 缓存 JSON 与新 System.Text.Json JSON 已覆盖双向读取测试，包括公开字段、日期和 `System.Type`。

纯 `System.Text.Json` 程序集无法继续公开以 Newtonsoft 类型为参数或基类的相同 CLR 元数据。因此，从 CO2NET 3.x 升级到 4.x 的应用需要重新编译；常见源码调用可以保持不变。需要旧二进制直接加载的应用应继续使用对应的 3.x 版本。

任意 Newtonsoft Converter/ContractResolver、`TypeNameHandling`、私有参数化构造函数等没有安全的一对一转换。此类模型应改用 System.Text.Json Converter、`JsonPolymorphic`/`JsonDerivedType` 或源生成上下文。`ReferenceHandler.IgnoreCycles` 会把循环成员写为 `null`，与 Newtonsoft 省略该成员的文本结果不同。

## Native AOT 调用

Native AOT 项目必须为业务 DTO 创建 `JsonSerializerContext`，并调用接收 `JsonTypeInfo<T>` 的重载：

```csharp
[JsonSerializable(typeof(MyPayload))]
internal sealed partial class AppJsonContext : JsonSerializerContext;

var typeInfo = AppJsonContext.Default.MyPayload;
var json = payload.ToJson(typeInfo);
var value = SerializerHelper.GetObject(typeInfo, json);
var cached = payload.SerializeToCache(typeInfo);
var response = await Get.GetJsonAsync(typeInfo, services, url);
```

普通反射重载仍保留给既有代码，但在 .NET 8+ 上带有 `RequiresDynamicCode` 和 `RequiresUnreferencedCode` 标记。

注册时关闭扩展缓存自动扫描，改为显式注册：

```csharp
register.UseSenparcGlobal(
    autoScanExtensionCacheStrategies: false,
    extensionCacheStrategiesFunc: () => new List<IDomainExtensionCacheStrategy>
    {
        // 显式列出需要的扩展缓存策略
    });
```

缓存中使用 `System.Type` 时，在源生成上下文注册 `SystemTypeJsonConverter`，并在读取已有缓存前显式保留可能出现的类型：

```csharp
[JsonSourceGenerationOptions(Converters = new[] { typeof(SystemTypeJsonConverter) })]
[JsonSerializable(typeof(MyCachePayload))]
internal sealed partial class CacheJsonContext : JsonSerializerContext;

SystemTypeJsonConverter.RegisterType<MyPayload>();
```

## 依赖边界

核心 `Senparc.CO2NET` 包不引用 Newtonsoft.Json。`Senparc.CO2NET.Cache.CsRedis` 和 `Senparc.CO2NET.Cache.Memcached` 当前上游包仍会传递引入 Newtonsoft.Json；严格 Native AOT 项目应选用不含该依赖的缓存实现（例如 StackExchange.Redis 模块），或替换相应上游包。

`Senparc.CO2NET.Cache.Redis` 自 5.3.0 起移除 `BinaryFormatter`；`StackExchangeRedisExtensions.Serialize/Deserialize` 改为 UTF-8 JSON。若仍有历史二进制缓存载荷，需自行迁移或清理。

`Senparc.CO2NET.WebApi` **不支持** Native AOT，请勿在 `PublishAot=true` 宿主中引用。

## 验证

仓库内 `src/Senparc.CO2NET.AotSmokeTest` 使用 `PublishAot=true`，并将 IL2026/IL3050 视为错误，覆盖核心 JSON / 缓存 / HTTP 的 `JsonTypeInfo` 路径。
