using Senparc.CO2NET.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
        //    var json = Newtonsoft.Json.JsonConvert.SerializeObject(cacheWarpper);
        //    return json;
        //}

        ///// <summary>
        ///// Deserialize from cache object to instance
        ///// </summary>
        ///// <param name="value"></param>
        ///// <returns></returns>
        //public static object DeserializeFromCache(this string value)
        //{
        //    var cacheWarpper = (CacheWrapper<object>)Newtonsoft.Json.JsonConvert.DeserializeObject(value, typeof(CacheWrapper<object>));
        //    var obj = Newtonsoft.Json.JsonConvert.DeserializeObject(cacheWarpper.Object.ToJson(), cacheWarpper.Type);
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
        //    var cacheWarpper = Newtonsoft.Json.JsonConvert.DeserializeObject<CacheWrapper<T>>(value);
        //    return cacheWarpper.Object;
        //}

        #endregion

        #region Direct JSON serialization scheme

        /// <summary>
        /// Serialize to an object usable by the cache
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string SerializeToCache<T>(this T obj)
        {
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(obj);
            return json;
        }

        /// <summary>
        /// Deserialize from cache object to instance
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static object DeserializeFromCache(this string value, Type type = null)
        {
            var obj = Newtonsoft.Json.JsonConvert.DeserializeObject(value, type);
            return obj;
        }

        /// <summary>
        /// Deserialize from cache object to instance (more efficient, recommended)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        public static T DeserializeFromCache<T>(this string value)
        {
            var obj = Newtonsoft.Json.JsonConvert.DeserializeObject<T>(value);
            return obj;
        }


        #endregion
    }
}
