#region Apache License Version 2.0
/*----------------------------------------------------------------

Copyright 2025 Suzhou Senparc Network Technology Co.,Ltd.

Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file
except in compliance with the License. You may obtain a copy of the License at

http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software distributed under the
License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND,
either express or implied. See the License for the specific language governing permissions
and limitations under the License.

Detail: https://github.com/Senparc/Senparc.CO2NET/blob/master/LICENSE

----------------------------------------------------------------*/
#endregion Apache License Version 2.0

/*----------------------------------------------------------------
    Copyright (C) 2026 Senparc
    
    FileName：SerializerHelper.cs
    File Function Description：unicode decoding
    
    
    Creation Identifier：Senparc - 20150211
    
    Modification Identifier：Senparc - 20150303
    Modification Description：Organize interface

    Modification Identifier：Senparc - 20180526
    Modification Description：v4.22.0-rc1 Use Newtonsoft.Json for serialization

    Modification Identifier：Senparc - 20180526
    Modification Description：v0.2.9 Add SerializerHelper.GetObject(this string jsonString, Type type) method

    Modification Identifier：Senparc - 20220331
    Modification Description：v2.0.5.4 Add settings parameter to GetObject() method

    Modification Identifier：Senparc - 20220530
    Modification Description：v2.1.1 Add more overloads for GetObject() method

    修改标识：Senparc - 20260721
    修改描述：v4.0.0 将 GetJsonString 和 GetObject 迁移至 System.Text.Json 并新增源生成重载

----------------------------------------------------------------*/



using Senparc.CO2NET.Helpers.Serializers;
using System;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using System.Text;
using System.Text.Json.Serialization.Metadata;
#if NET462
using System.Web.Script.Serialization;
#endif

namespace Senparc.CO2NET.Helpers
{

    /// <summary>
    /// Serialization helper class
    /// </summary>
    public static class SerializerHelper
    {
        #region JSON

        /// <summary>
        /// Convert object to JSON string (with Json output configuration)
        /// </summary>
        /// <param name="data">Data to generate JSON string</param>
        /// <param name="jsonSetting">JSON output settings</param>
        /// <returns></returns>
        public static string GetJsonString(object data, JsonSetting jsonSetting = null)
        {
            return SystemTextJsonSerializer.Serialize(data, settings: jsonSetting);

            //TODO: Enable as needed

            ////Decode Unicode, can also be done through App.Config (Web.Config) settings, this is just a temporary fix, not used often
            //string jsonString;
            //MatchEvaluator evaluator = new MatchEvaluator(DecodeUnicode);
            //var json = Regex.Replace(jsonString, @"\\u[0123456789abcdef]{4}", evaluator);//or: [\\u007f-\\uffff], \ corresponds to \u000a, but generally \ is kept
            //return json;
        }

        /// <summary>
        /// Convert an object to JSON using source-generated metadata. This overload is Native AOT safe.
        /// The metadata parameter is first so existing GetJsonString(data, null) calls remain unambiguous.
        /// </summary>
        public static string GetJsonString<T>(JsonTypeInfo<T> jsonTypeInfo, T data)
        {
            return SystemTextJsonSerializer.Serialize(data, jsonTypeInfo);
        }

        /// <summary>
        /// Deserialize to object
        /// </summary>
        /// <typeparam name="T">Type of deserialized object</typeparam>
        /// <param name="jsonString">JSON string</param>
        /// <returns></returns>
        public static T GetObject<T>(this string jsonString)
        {
            return SystemTextJsonSerializer.Deserialize<T>(jsonString);
            //#if NET462
            //            JavaScriptSerializer jsSerializer = new JavaScriptSerializer();
            //            return jsSerializer.Deserialize<T>(jsonString);
            //#else
            //            return (T)Newtonsoft.Json.JsonConvert.DeserializeObject(jsonString, typeof(T));
            //#endif
        }

        /// <summary>
        /// Deserialize to object
        /// </summary>
        /// <typeparam name="T">Type of deserialized object</typeparam>
        /// <param name="jsonString">JSON string</param>
        /// <param name="settings">JsonSerializerOptions, JsonSetting, or a legacy Newtonsoft settings object.</param>
        /// <returns></returns>
        public static T GetObject<T>(this string jsonString, object settings = null)
        {
            return SystemTextJsonSerializer.Deserialize<T>(jsonString, settings);
            //#if NET462
            //            JavaScriptSerializer jsSerializer = new JavaScriptSerializer();
            //            return jsSerializer.Deserialize<T>(jsonString);
            //#else
            //            return (T)Newtonsoft.Json.JsonConvert.DeserializeObject(jsonString, typeof(T));
            //#endif
        }

        /// <summary>
        /// Deserialize JSON using source-generated metadata. This overload is Native AOT safe.
        /// </summary>
        public static T GetObject<T>(JsonTypeInfo<T> jsonTypeInfo, string jsonString)
        {
            return SystemTextJsonSerializer.Deserialize(jsonString, jsonTypeInfo);
        }


        /// <summary>
        /// Deserialize to object
        /// </summary>
        /// <param name="jsonString">JSON string</param>
        /// <returns></returns>
        public static object GetObject(this string jsonString)
        {
            return SystemTextJsonSerializer.Deserialize(jsonString);
        }


        /// <summary>
        /// Deserialize to object
        /// </summary>
        /// <param name="jsonString">JSON string</param>
        /// <param name="settings">JsonSerializerOptions, JsonSetting, or a legacy Newtonsoft settings object.</param>
        /// <returns></returns>
        public static object GetObject(this string jsonString, object settings)
        {
            return SystemTextJsonSerializer.Deserialize(jsonString, settings: settings);
        }

        /// <summary>
        /// Deserialize to object
        /// </summary>
        /// <param name="jsonString">JSON string</param>
        /// <param name="type">Deserialization type</param>
        /// <param name="settings">JsonSerializerOptions, JsonSetting, or a legacy Newtonsoft settings object.</param>
        /// <returns></returns>
        public static object GetObject(this string jsonString, Type type, object settings = null)
        {
            return SystemTextJsonSerializer.Deserialize(jsonString, type, settings);
        }

        //        #region Serialize object - byte[]

        //        #region Binary entity object
        //        /// <summary>
        //        /// Serialize object (binary entity object)
        //        /// </summary>
        //        /// <param name="o"></param>
        //        /// <returns></returns>
        //        public static byte[] BinarySerialize(this object o)
        //        {
        //            if (o == null)
        //////            {
        //                return null;
        //////            }

        //#if !NET462
        ////            ////Binary serialization scheme
        ////            //using (MemoryStream memoryStream = new MemoryStream())
        //////            //{

        ////            //    ProtoBuf.Serializer.Serialize(memoryStream, o);
        ////            //    byte[] objectDataAsStream = memoryStream.ToArray();
        ////            //    return objectDataAsStream;
        //////            //}

        //            BinaryFormatter.BinaryConverter binaryConverter = new BinaryFormatter.BinaryConverter();
        //            return binaryConverter.Serialize(o);
        //#else
        //            #region .net 4.5 and .net core 2.0 both support BinaryFormatter, but .net core 2.0 does not support delegate serialization
        ////            //Binary serialization scheme
        //////            var binaryFormatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
        ////            using (MemoryStream memoryStream = new MemoryStream())
        //////            {
        ////                binaryFormatter.Serialize(memoryStream, o);
        ////                byte[] objectDataAsStream = memoryStream.ToArray();
        ////                return objectDataAsStream;
        //////            }
        //            #endregion
        //#endif

        ////            //Using JSON serialization will cause errors during deserialization to IContainerBag in Get() method
        ////            //JSON serialization scheme
        ////            //SerializerHelper serializerHelper = new SerializerHelper();
        ////            //var jsonSetting = serializerHelper.GetJsonString(o);
        ////            //return Encoding.UTF8.GetBytes(jsonSetting);
        //        }

        //        /// <summary>
        //        /// Deserialize object (binary entity object)
        //        /// </summary>
        //        /// <typeparam name="T"></typeparam>
        //        /// <param name="stream"></param>
        //        /// <returns></returns>
        //        public static T BinaryDeserialize<T>(this byte[] stream)
        //        {
        //            if (stream == null)
        //////            {
        //                return default(T);
        //////            }

        //#if !NET462
        ////            ////Binary serialization scheme
        ////            //using (MemoryStream memoryStream = new MemoryStream(stream))
        //////            //{
        ////            //    T result = ProtoBuf.Serializer.Deserialize<T>(memoryStream);
        ////            //    return result;
        //////            //}

        //            BinaryFormatter.BinaryConverter binaryConverter = new BinaryFormatter.BinaryConverter();
        //            return binaryConverter.Deserialize<T>(stream);

        //#else
        //            #region .net 4.5 and .net core 2.0 both support BinaryFormatter, but .net core 2.0 does not support delegate serialization
        ////            //Binary serialization scheme
        //////            var binaryFormatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
        ////            using (MemoryStream memoryStream = new MemoryStream(stream))
        //////            {
        ////                T result = (T)binaryFormatter.Deserialize(memoryStream);
        ////                return result;
        //////            }
        //            #endregion
        //#endif
        //        }
        //        #endregion

        //        #endregion

        #endregion

        #region Unicode

        /// <summary>
        /// Convert string to Unicode
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string EncodeUnicode(string str)
        {
            char[] charbuffers = str.ToCharArray();
            byte[] buffer;
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < charbuffers.Length; i++)
            {
                buffer = System.Text.Encoding.Unicode.GetBytes(charbuffers[i].ToString());
                sb.Append(String.Format("\\u{0:X2}{1:X2}", buffer[1], buffer[0]));
            }
            return sb.ToString();
        }

        /// <summary>
        /// Unicode decoding
        /// </summary>
        /// <param name="unicodeStr">Unicode encoded string</param>
        /// <returns></returns>
        public static string DecodeUnicode(string unicodeStr)
        {
            StringBuilder sb = new StringBuilder();
            if (!string.IsNullOrEmpty(unicodeStr))
            {
                string[] strlist = unicodeStr.Replace("\\", "").Split('u');
                try
                {
                    for (int i = 1; i < strlist.Length; i++)
                    {
                        //Convert Unicode characters to decimal integers, then to char Chinese characters
                        sb.Append((char)int.Parse(strlist[i], System.Globalization.NumberStyles.HexNumber));
                    }
                }
                catch (FormatException ex)
                {
                    sb.Append("||出错：" + ex.Message);
                }
            }
            return sb.ToString();
        }


        //TODO: Need to optimize Match conditions before enabling
        ///// <summary>
        ///// Unicode decoding
        ///// </summary>
        ///// <param name="match"></param>
        ///// <returns></returns>
        //public static string DecodeUnicode(Match match)
        //{
        //    //Unicode code table: http://www.cnblogs.com/whiteyun/archive/2010/07/06/1772218.html

        //    if (!match.Success)
        //    {
        //        return null;
        //    }

        //    char outStr = (char)int.Parse(match.Value.Remove(0, 2), NumberStyles.HexNumber);
        //    return new string(outStr, 1);
        //}


        #endregion
    }
}
