/*----------------------------------------------------------------
    Copyright (C) 2026 Senparc

    FileName：StackExchangeRedisExtensions.cs
    File Function Description：StackExchange.Redis extension.

    Creation Identifier：Senparc - 20160309

    Modification Identifier：Senparc - 20170204
    Modification Description：v1.2.0 Serialization method changed to JSON

    Modification Identifier：Senparc - 20260726
    Modification Description：v5.3.0 Remove BinaryFormatter; serialize with UTF-8 JSON for Native AOT readiness

----------------------------------------------------------------*/

using System.Text;
using Senparc.CO2NET.Cache;
#if NET8_0_OR_GREATER
using System.Diagnostics.CodeAnalysis;
#endif

namespace Senparc.CO2NET.Cache.Redis
{
    /// <summary>
    ///  StackExchangeRedis extension
    /// </summary>
    public static class StackExchangeRedisExtensions
    {
        /// <summary>
        /// Serialize object to UTF-8 JSON bytes (same payload family as <see cref="CacheSerializeExtension.SerializeToCache"/>).
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
#if NET8_0_OR_GREATER
        [RequiresDynamicCode("Runtime JSON metadata may require dynamic code. Prefer SerializeToCache with JsonTypeInfo for Native AOT.")]
        [RequiresUnreferencedCode("Runtime JSON metadata may be removed by trimming. Prefer SerializeToCache with JsonTypeInfo for Native AOT.")]
#endif
        public static byte[] Serialize(this object o)
        {
            if (o == null)
            {
                return null;
            }

            return Encoding.UTF8.GetBytes(o.SerializeToCache());
        }

        /// <summary>
        /// Deserialize object from UTF-8 JSON bytes.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="stream"></param>
        /// <returns></returns>
#if NET8_0_OR_GREATER
        [RequiresDynamicCode("Runtime JSON metadata may require dynamic code. Prefer DeserializeFromCache with JsonTypeInfo for Native AOT.")]
        [RequiresUnreferencedCode("Runtime JSON metadata may be removed by trimming. Prefer DeserializeFromCache with JsonTypeInfo for Native AOT.")]
#endif
        public static T Deserialize<T>(this byte[] stream)
        {
            if (stream == null)
            {
                return default(T);
            }

            return Encoding.UTF8.GetString(stream).DeserializeFromCache<T>();
        }
    }
}
