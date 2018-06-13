using Senparc.CO2NET.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Senparc.CO2NET.Cache
{
    public class CacheWrapper<T>
    {
        public Type Type { get; set; }
        public T Object { get; set; }

        public CacheWrapper(T obj)
        {
            this.Object = obj;
            this.Type = typeof(T);
        }
    }


    public static class CacheSerializeExtension
    {
        /// <summary>
        /// 序列化到缓存可用的对象
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string SerializeToCache<T>(this T obj)
        {
            var cacheWarpper = new CacheWrapper<T>(obj);
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(cacheWarpper);
            return json;
        }

        /// <summary>
        /// 从缓存对象反序列化到实例
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static object DeserializeToCache(this string value)
        {
            var cacheWarpper = (CacheWrapper<object>)Newtonsoft.Json.JsonConvert.DeserializeObject(value,typeof(CacheWrapper<object>));
            var obj = Newtonsoft.Json.JsonConvert.DeserializeObject(cacheWarpper.Object.ToJson(),cacheWarpper.Type);
            return obj;
        }

        /// <summary>
        /// 从缓存对象反序列化到实例
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        public static T DeserializeToCache<T>(this string value)
        {
            var cacheWarpper = (CacheWrapper<T>)Newtonsoft.Json.JsonConvert.DeserializeObject(value, typeof(CacheWrapper<object>));
            return cacheWarpper.Object;
        }
    }
}
