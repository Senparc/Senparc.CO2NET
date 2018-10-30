/*----------------------------------------------------------------
    Copyright (C) 2018 Senparc

    文件名：LogFactory.cs
    文件功能描述：Senparc.Common.SDK 日志工厂


    创建标识：MartyZane - 20181030

----------------------------------------------------------------*/

using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;

namespace Senparc.Common.SDK
{
    /// <summary>
    /// Log4Net日志 工厂
    /// 版本：2.0
    /// <author>
    ///		<name>MartyZane</name>
    ///		<date>2014.03.03</date>
    /// </author>
    /// </summary>
    public class LogFactory
    {
        static LogFactory()
        {
            FileInfo configFile = new FileInfo(HttpContext.Current.Server.MapPath("/XmlConfig/log4net.config"));

            log4net.Config.XmlConfigurator.Configure(configFile);
        }

        public static LogHelper GetLogger(Type type)
        {
            return new LogHelper(LogManager.GetLogger(type));
        }

        public static LogHelper GetLogger(string str)
        {
            return new LogHelper(LogManager.GetLogger(str));
        }
    }
}
