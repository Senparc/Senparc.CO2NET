/*----------------------------------------------------------------
    Copyright (C) 2018 Senparc

    文件名：ConfigHelper.cs
    文件功能描述：Senparc.Common.SDK 配置文件帮助类


    创建标识：MartyZane - 20181030

----------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Web;

namespace Senparc.Common.SDK
{
    /// <summary>
    ///  Config配置文件 公共帮助类
    /// 版本：2.0
    /// <author>
    ///		<name>MartyZane</name>
    ///		<date>2013.09.27</date>
    /// </author>
    /// </summary>
    public class ConfigHelper
    {
        /// <summary>
        /// 根据Key取Value值
        /// </summary>
        /// <param name="key"></param>
        public static string AppSettings(string key)
        {
            return ConfigurationManager.AppSettings[key].ToString().Trim();
        }
        /// <summary>
        /// 根据name取connectionString值
        /// </summary>
        /// <param name="name"></param>
        public static string ConnectionStrings(string name)
        {
            return ConfigurationManager.ConnectionStrings[name].ConnectionString.Trim();
        }
        /// <summary>
        /// 根据Key修改Value
        /// </summary>
        /// <param name="key">要修改的Key</param>
        /// <param name="value">要修改为的值</param>
        public static void SetValue(string key, string value)
        {
            System.Xml.XmlDocument xDoc = new System.Xml.XmlDocument();
            xDoc.Load(HttpContext.Current.Server.MapPath("/XmlConfig/Config.xml"));
            System.Xml.XmlNode xNode;
            System.Xml.XmlElement xElem1;
            System.Xml.XmlElement xElem2;
            xNode = xDoc.SelectSingleNode("//appSettings");

            xElem1 = (System.Xml.XmlElement)xNode.SelectSingleNode("//add[@key='" + key + "']");
            if (xElem1 != null) xElem1.SetAttribute("value", value);
            else
            {
                xElem2 = xDoc.CreateElement("add");
                xElem2.SetAttribute("key", key);
                xElem2.SetAttribute("value", value);
                xNode.AppendChild(xElem2);
            }
            xDoc.Save(HttpContext.Current.Server.MapPath("/XmlConfig/Config.xml"));
        }
    }
}
