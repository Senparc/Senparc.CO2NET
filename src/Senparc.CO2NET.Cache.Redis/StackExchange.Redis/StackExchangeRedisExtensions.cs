/*----------------------------------------------------------------
    Copyright (C) 2024 Senparc

    FileName：StackExchangeRedisExtensions.cs
    File Function Description：StackExchange.Redis extension.

    Creation Identifier：Senparc - 20160309

    Modification Identifier：Senparc - 20170204
    Modification Description：v1.2.0 Serialization method changed to JSON

----------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
//using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using Senparc.CO2NET.Helpers;

//#if !NETSTANDARD1_6
//using System.Runtime.Serialization.Formatters.Binary;
//#endif

namespace Senparc.CO2NET.Cache.Redis
{
    /// <summary>
    ///  StackExchangeRedis extension
    /// </summary>
    public static class StackExchangeRedisExtensions
    {

        //public static T Get<T>(string key)
        //{
        //    var connect = AzureredisDb.Cache;
        //    var r = AzureredisDb.Cache.StringGet(key);
        //    return Deserialize<T>(r);
        //}

        //public static List<T> GetList<T>(string key)
        //{
        //    return (List<T>)Get(key);
        //}

        //public static void SetList<T>(string key, List<T> list)
        //{
        //    Set(key, list);
        //}

        //public static object Get(string key)
        //{
        //    return Deserialize<object>(AzureredisDb.Cache.StringGet(key));
        //}

        //public static void Set(string key, object value)
        //{
        //    AzureredisDb.Cache.StringSet(key, Serialize(value));
        //}

        /// <summary>
        /// Serialize object
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        public static byte[] Serialize(this object o)
        {
            if (o == null)
            {
                return null;
            }

            var dtx = SystemTime.Now;

#if !NET462
            ////Binary serialization scheme
            //using (MemoryStream memoryStream = new MemoryStream())
            //{

            //    ProtoBuf.Serializer.Serialize(memoryStream, o);
            //    byte[] objectDataAsStream = memoryStream.ToArray();
            //    return objectDataAsStream;
            //}

            BinaryFormatter.BinaryConverter binaryConverter = new BinaryFormatter.BinaryConverter();
            return binaryConverter.Serialize(o);
#else
            #region .net 4.5 和 .net core 2.0 都提供对 BinaryFormatter 的支持，但是 .net core 2.0 不支持委托的序列化
            //Binary serialization scheme
            var binaryFormatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
            using (MemoryStream memoryStream = new MemoryStream())
            {
                binaryFormatter.Serialize(memoryStream, o);
                byte[] objectDataAsStream = memoryStream.ToArray();
                return objectDataAsStream;
            }
            #endregion
#endif

            //Console.WriteLine($"StackExchangeRedisExtensions.Serialize time taken：{SystemTime.DiffTotalMS(dtx)}ms");


            //Using JSON serialization, there will be an error in deserializing to IContainerBag in the Get() method
            //JSON serialization scheme
            //SerializerHelper serializerHelper = new SerializerHelper();
            //var jsonSetting = serializerHelper.GetJsonString(o);
            //return Encoding.UTF8.GetBytes(jsonSetting);
        }

        /// <summary>
        /// Deserialize object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static T Deserialize<T>(this byte[] stream)
        {
            if (stream == null)
            {
                return default(T);
            }

#if !NET462
            ////Binary serialization scheme
            //using (MemoryStream memoryStream = new MemoryStream(stream))
            //{
            //    T result = ProtoBuf.Serializer.Deserialize<T>(memoryStream);
            //    return result;
            //}

            BinaryFormatter.BinaryConverter binaryConverter = new BinaryFormatter.BinaryConverter();
            return binaryConverter.Deserialize<T>(stream);

#else
            #region .net 4.5 和 .net core 2.0 都提供对 BinaryFormatter 的支持，但是 .net core 2.0 不支持委托的序列化
            //Binary serialization scheme
            var binaryFormatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
            using (MemoryStream memoryStream = new MemoryStream(stream))
            {
                T result = (T)binaryFormatter.Deserialize(memoryStream);
                return result;
            }
            #endregion
#endif


            //JSON serialization scheme
            //SerializerHelper serializerHelper = new SerializerHelper();
            //T result = serializerHelper.GetObject<T>(Encoding.UTF8.GetString(stream));
            //return result;
        }
    }
}
