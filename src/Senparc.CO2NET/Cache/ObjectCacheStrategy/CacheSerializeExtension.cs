/*----------------------------------------------------------------
    Copyright (C) 2026 Senparc

    文件名：CacheSerializeExtension.cs
    文件功能描述：提供缓存对象统一 JSON 序列化与反序列化扩展方法


    创建标识：Senparc - 20180613

    修改标识：Senparc - 20260721
    修改描述：v4.0.0 使用 System.Text.Json 实现缓存序列化并新增 JsonTypeInfo Native AOT 重载

    修改标识：Senparc - 20260722
    修改描述：v4.1.0 标注反射缓存方法并强化 JsonTypeInfo Native AOT 使用指引

----------------------------------------------------------------*/

using Senparc.CO2NET.Helpers.Serializers;
using System;
using System.Text.Json.Serialization.Metadata;
#if NET8_0_OR_GREATER
using System.Diagnostics.CodeAnalysis;
#endif

namespace Senparc.CO2NET.Cache
{
    /// <summary>
    /// Used to provide encapsulated objects for cache storage, including object type (Type) information
    /// </summary>
    /// <typeparam name="T"></typeparam>
    sealed internal class CacheWrapper<T>
    {
        public Type Type { get; set; }
        public T Object { get; set; }

        public CacheWrapper(T obj)
        {
            this.Object = obj;
            if (obj == null)
            {
                this.Type = typeof(object);//TODO: It is better to also determine the type
            }
            else
            {
                this.Type = obj.GetType();
            }
        }
    }

    /// <summary>
    /// Cache serialization extension methods, all (distributed) cache serialization and deserialization processes must use these methods for unified read and write
    /// </summary>
    public static class CacheSerializeExtension
    {
        #region CacheWrapper Solution（efficiency is 3-5 times lower than direct serialization, but still acceptable, comparable to binary serialization, the advantage is automatic type recognition

        ///// <summary>
        ///// Serialize to an object usable by the cache
        ///// </summary>
        ///// <param name="obj"></param>
        ///// <returns></returns>
        //public static string SerializeToCache<T>(this T obj)
        //{
        //    var cacheWarpper = new CacheWrapper<T>(obj);
        //    var json = SystemTextJsonSerializer.Serialize(cacheWarpper);
        //    return json;
        //}

        ///// <summary>
        ///// Deserialize from cache object to instance
        ///// </summary>
        ///// <param name="value"></param>
        ///// <returns></returns>
        //public static object DeserializeFromCache(this string value)
        //{
        //    var cacheWarpper = (CacheWrapper<object>)SystemTextJsonSerializer.Deserialize(value, typeof(CacheWrapper<object>));
        //    var obj = SystemTextJsonSerializer.Deserialize(SystemTextJsonSerializer.Serialize(cacheWarpper.Object), cacheWarpper.Type);
        //    return obj;
        //}

        ///// <summary>
        ///// Deserialize from cache object to instance (more efficient, recommended)
        ///// </summary>
        ///// <typeparam name="T"></typeparam>
        ///// <param name="value"></param>
        ///// <returns></returns>
        //public static T DeserializeFromCache<T>(this string value)
        //{
        //    var cacheWarpper = SystemTextJsonSerializer.Deserialize<CacheWrapper<T>>(value);
        //    return cacheWarpper.Object;
        //}

        #endregion

        #region Direct JSON serialization scheme

        /// <summary>
        /// Serialize to an object usable by the cache
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
#if NET8_0_OR_GREATER
        [RequiresDynamicCode("Runtime JSON metadata may require dynamic code. Use SerializeToCache with JsonTypeInfo<T> for Native AOT.")]
        [RequiresUnreferencedCode("Runtime JSON metadata may be removed by trimming. Use SerializeToCache with JsonTypeInfo<T> for Native AOT.")]
#endif
        public static string SerializeToCache<T>(this T obj)
        {
            return SystemTextJsonSerializer.Serialize(obj);
        }

        /// <summary>
        /// Serialize to a cache string using source-generated metadata. This overload is Native AOT safe.
        /// </summary>
        public static string SerializeToCache<T>(this T obj, JsonTypeInfo<T> jsonTypeInfo)
        {
            return SystemTextJsonSerializer.Serialize(obj, jsonTypeInfo);
        }

        /// <summary>
        /// Deserialize from cache object to instance
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
#if NET8_0_OR_GREATER
        [RequiresDynamicCode("Runtime JSON metadata may require dynamic code. Use DeserializeFromCache with JsonTypeInfo<T> for Native AOT.")]
        [RequiresUnreferencedCode("Runtime JSON metadata may be removed by trimming. Use DeserializeFromCache with JsonTypeInfo<T> for Native AOT.")]
#endif
        public static object DeserializeFromCache(this string value, Type type = null)
        {
            return SystemTextJsonSerializer.Deserialize(value, type);
        }

        /// <summary>
        /// Deserialize from cache object to instance (more efficient, recommended)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
#if NET8_0_OR_GREATER
        [RequiresDynamicCode("Runtime JSON metadata may require dynamic code. Use DeserializeFromCache with JsonTypeInfo<T> for Native AOT.")]
        [RequiresUnreferencedCode("Runtime JSON metadata may be removed by trimming. Use DeserializeFromCache with JsonTypeInfo<T> for Native AOT.")]
#endif
        public static T DeserializeFromCache<T>(this string value)
        {
            return SystemTextJsonSerializer.Deserialize<T>(value);
        }

        /// <summary>
        /// Deserialize a cache string using source-generated metadata. This overload is Native AOT safe.
        /// </summary>
        public static T DeserializeFromCache<T>(this string value, JsonTypeInfo<T> jsonTypeInfo)
        {
            return SystemTextJsonSerializer.Deserialize(value, jsonTypeInfo);
        }


        #endregion
    }
}
