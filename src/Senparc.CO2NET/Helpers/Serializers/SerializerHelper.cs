#region Apache License Version 2.0
/*----------------------------------------------------------------

Copyright 2019 Suzhou Senparc Network Technology Co.,Ltd.

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
    Copyright (C) 2020 Senparc
    
    文件名：SerializerHelper.cs
    文件功能描述：unicode解码
    
    
    创建标识：Senparc - 20150211
    
    修改标识：Senparc - 20150303
    修改描述：整理接口

    修改标识：Senparc - 20180526
    修改描述：v4.22.0-rc1 使用 Newtonsoft.Json 进行序列化

    修改标识：Senparc - 20180526
    修改描述：v0.2.9 添加 SerializerHelper.GetObject(this string jsonString, Type type) 方法

----------------------------------------------------------------*/



using Senparc.CO2NET.Helpers.Serializers;
using System;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using System.Text;
#if NET45
using System.Web.Script.Serialization;
#else
using Newtonsoft.Json;
#endif

namespace Senparc.CO2NET.Helpers
{

    /// <summary>
    /// 序列化帮助类
    /// </summary>
    public static class SerializerHelper
    {
        #region JSON

        /// <summary>
        /// 将对象转为JSON字符串（进行Json输出配置）
        /// </summary>
        /// <param name="data">需要生成JSON字符串的数据</param>
        /// <param name="jsonSetting">JSON输出设置</param>
        /// <returns></returns>
        public static string GetJsonString(object data, JsonSetting jsonSetting = null)
        {

            return Newtonsoft.Json.JsonConvert.SerializeObject(data, new JsonSettingWrap(jsonSetting));

            //TODO：视情况启用

            ////解码Unicode，也可以通过设置App.Config（Web.Config）设置来做，这里只是暂时弥补一下，用到的地方不多
            //string jsonString;
            //MatchEvaluator evaluator = new MatchEvaluator(DecodeUnicode);
            //var json = Regex.Replace(jsonString, @"\\u[0123456789abcdef]{4}", evaluator);//或：[\\u007f-\\uffff]，\对应为\u000a，但一般情况下会保持\
            //return json;
        }

        /// <summary>
        /// 反序列化到对象
        /// </summary>
        /// <typeparam name="T">反序列化对象类型</typeparam>
        /// <param name="jsonString">JSON字符串</param>
        /// <returns></returns>
        public static T GetObject<T>(this string jsonString)
        {
            return (T)Newtonsoft.Json.JsonConvert.DeserializeObject(jsonString, typeof(T));
            //#if NET45
            //            JavaScriptSerializer jsSerializer = new JavaScriptSerializer();
            //            return jsSerializer.Deserialize<T>(jsonString);
            //#else
            //            return (T)Newtonsoft.Json.JsonConvert.DeserializeObject(jsonString, typeof(T));
            //#endif
        }

        /// <summary>
        /// 反序列化到对象
        /// </summary>
        /// <param name="jsonString">JSON字符串</param>
        /// <param name="type">反序列化类型</param>
        /// <returns></returns>
        public static object GetObject(this string jsonString, Type type)
        {
            return Newtonsoft.Json.JsonConvert.DeserializeObject(jsonString, type);
        }

        //        #region 序列化对象 - byte[]

        //        #region 二进制实体对象
        //        /// <summary>
        //        /// 序列化对象（二进制实体对象）
        //        /// </summary>
        //        /// <param name="o"></param>
        //        /// <returns></returns>
        //        public static byte[] BinarySerialize(this object o)
        //        {
        //            if (o == null)
        //            {
        //                return null;
        //            }

        //#if !NET45
        //            ////二进制序列化方案
        //            //using (MemoryStream memoryStream = new MemoryStream())
        //            //{

        //            //    ProtoBuf.Serializer.Serialize(memoryStream, o);
        //            //    byte[] objectDataAsStream = memoryStream.ToArray();
        //            //    return objectDataAsStream;
        //            //}

        //            BinaryFormatter.BinaryConverter binaryConverter = new BinaryFormatter.BinaryConverter();
        //            return binaryConverter.Serialize(o);
        //#else
        //            #region .net 4.5 和 .net core 2.0 都提供对 BinaryFormatter 的支持，但是 .net core 2.0 不支持委托的序列化
        //            //二进制序列化方案
        //            var binaryFormatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
        //            using (MemoryStream memoryStream = new MemoryStream())
        //            {
        //                binaryFormatter.Serialize(memoryStream, o);
        //                byte[] objectDataAsStream = memoryStream.ToArray();
        //                return objectDataAsStream;
        //            }
        //            #endregion
        //#endif

        //            //使用JSON序列化，会在Get()方法反序列化到IContainerBag的过程中出错
        //            //JSON序列化方案
        //            //SerializerHelper serializerHelper = new SerializerHelper();
        //            //var jsonSetting = serializerHelper.GetJsonString(o);
        //            //return Encoding.UTF8.GetBytes(jsonSetting);
        //        }

        //        /// <summary>
        //        /// 反序列化对象（二进制实体对象）
        //        /// </summary>
        //        /// <typeparam name="T"></typeparam>
        //        /// <param name="stream"></param>
        //        /// <returns></returns>
        //        public static T BinaryDeserialize<T>(this byte[] stream)
        //        {
        //            if (stream == null)
        //            {
        //                return default(T);
        //            }

        //#if !NET45
        //            ////二进制序列化方案
        //            //using (MemoryStream memoryStream = new MemoryStream(stream))
        //            //{
        //            //    T result = ProtoBuf.Serializer.Deserialize<T>(memoryStream);
        //            //    return result;
        //            //}

        //            BinaryFormatter.BinaryConverter binaryConverter = new BinaryFormatter.BinaryConverter();
        //            return binaryConverter.Deserialize<T>(stream);

        //#else
        //            #region .net 4.5 和 .net core 2.0 都提供对 BinaryFormatter 的支持，但是 .net core 2.0 不支持委托的序列化
        //            //二进制序列化方案
        //            var binaryFormatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
        //            using (MemoryStream memoryStream = new MemoryStream(stream))
        //            {
        //                T result = (T)binaryFormatter.Deserialize(memoryStream);
        //                return result;
        //            }
        //            #endregion
        //#endif
        //        }
        //        #endregion

        //        #endregion

        #endregion

        #region Unicode

        /// <summary>
        /// 将字符串转为Unicode
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
        /// unicode解码
        /// </summary>
        /// <param name="unicodeStr">unicode编码字符串</param>
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
                        //将unicode字符转为10进制整数，然后转为char中文字符
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


        //TODO：需要优化Match匹配条件后，可以启用
        ///// <summary>
        ///// unicode解码
        ///// </summary>
        ///// <param name="match"></param>
        ///// <returns></returns>
        //public static string DecodeUnicode(Match match)
        //{
        //    //Unicode码对照表：http://www.cnblogs.com/whiteyun/archive/2010/07/06/1772218.html

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
