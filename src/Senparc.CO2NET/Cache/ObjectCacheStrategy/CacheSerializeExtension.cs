using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Senparc.CO2NET.Cache
{
    public static class CacheSerializeExtension
    {
        /// <summary>
        /// 序列化到缓存可用的对象
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static object SerializeToCache(this object obj)
        {
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(obj);
            return json;
        }

        /// <summary>
        /// 从缓存对象反序列化到实例
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static object DeserializeToCache(this string value)
        {
            var json = Newtonsoft.Json.JsonConvert.DeserializeObject(value);
            return json;
        }

        /// <summary>
        /// 从缓存对象反序列化到实例
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        public static T DeserializeToCache<T>(this string value)
        {
            var obj = (T)Newtonsoft.Json.JsonConvert.DeserializeObject(value, typeof(T));
            return obj;
        }
    }
}
