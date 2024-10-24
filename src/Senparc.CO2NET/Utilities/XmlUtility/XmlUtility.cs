#region Apache License Version 2.0
/*----------------------------------------------------------------

Copyright 2023 Suzhou Senparc Network Technology Co.,Ltd.

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
    Copyright (C) 2024 Senparc

    FileName：BrowserUtility.cs
    File Function Description：Browser utility class


    Creation Identifier：Senparc - 20150419

    Modification Identifier：Senparc - 20161219
    Modification Description：v4.9.6 Corrected typo: Browser->Browser

    Modification Identifier：Senparc - 20161219
    Modification Description：v4.11.2 Modified SideInWeixinBrowser logic

    Modification Identifier：Senparc - 20180513
    Modification Description：v4.11.2 1. Added method to determine mini program requests SideInWeixinMiniProgram()
                      2. Added GetUserAgent() method

    ----  CO2NET   ----
    ----  split from Senparc.Weixin/Utilities/XmlUtility.cs  ----

    Modification Identifier：Senparc - 20180601
    Modification Description：v0.1.0 Migrated XmlUtility

    Modification Identifier：Senparc - 20220208
    Modification Description：v2.0.2 Added XmlUtility.Deserialize() override method

----------------------------------------------------------------*/

using System;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace Senparc.CO2NET.Utilities
{
    /// <summary>
    /// XML utility class
    /// </summary>
    public static class XmlUtility
    {

        #region 反序列化

        /// <summary>
        /// Deserialize
        /// </summary>
        /// <param name="xml">XML string</param>
        /// <param name="rootNodeName"></param>
        /// <returns></returns>
        public static object Deserialize(Type type, string xml, string rootNodeName = null)
        {
            try
            {
                using (StringReader sr = new StringReader(xml))
                {
                    XmlSerializer xmldes;
                    if (rootNodeName != null)
                    {
                        xmldes=new XmlSerializer(type, new XmlRootAttribute(rootNodeName));
                    }
                    else
                    {
                        xmldes = new XmlSerializer(type);
                    }
                    return xmldes.Deserialize(sr);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
        }

        /// <summary>
        /// Deserialize
        /// </summary>
        /// <param name="xml">XML string</param>
        /// <param name="rootNodeName"></param>
        /// <returns></returns>
        public static object Deserialize<T>(string xml, string rootNodeName = null)
        {
            return Deserialize(typeof(T), xml,rootNodeName);
        }

        /// <summary>
        /// Deserialize
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="rootNodeName"></param>
        /// <returns></returns>
        public static object Deserialize<T>(Stream stream, string rootNodeName = null)
        {
            XmlSerializer xmldes = new XmlSerializer(typeof(T));
            return xmldes.Deserialize(stream);
        }

        #endregion

        #region 序列化

        /// <summary>
        /// Serialize
        /// Note: This method serializes complex classes. If XmlInclude and other attributes are not declared, it may cause the error "Use the XmlInclude or SoapInclude attribute to specify types that are not known statically."
        /// </summary>
        /// <param name="obj">Object</param>
        /// <returns></returns>
        public static string Serializer<T>(T obj)
        {
            MemoryStream Stream = new MemoryStream();
            XmlSerializer xml = new XmlSerializer(typeof(T));
            try
            {
                //Serialize object
                xml.Serialize(Stream, obj);
            }
            catch (InvalidOperationException)
            {
                throw;
            }
            Stream.Position = 0;
            StreamReader sr = new StreamReader(Stream);
            string str = sr.ReadToEnd();

            sr.Dispose();
            Stream.Dispose();

            return str;
        }

        #endregion

        /// <summary>
        /// Serialize stream to XML string
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static XDocument Convert(Stream stream)
        {
            if (stream.CanSeek)
            {
                stream.Seek(0, SeekOrigin.Begin);//Force adjust pointer position
            }
            using (XmlReader xr = XmlReader.Create(stream))
            {
                return XDocument.Load(xr);
            }
            //#if NET462
            //            using (XmlReader xr = XmlReader.Create(stream))
            //            {
            //                return XDocument.Load(xr);
            //            }
            //#else
            //            using (var sr = new StreamReader(stream))
            //            {
            //                var xml = sr.ReadToEnd();
            //                return XDocument.Parse(xml);
            //            }
            //#endif
        }

    }
}
