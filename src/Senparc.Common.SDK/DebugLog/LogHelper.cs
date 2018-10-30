/*----------------------------------------------------------------
    Copyright (C) 2018 Senparc

    文件名：LogHelper.cs
    文件功能描述：Senparc.Common.SDK 日志帮助类


    创建标识：MartyZane - 20181030

----------------------------------------------------------------*/

using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Senparc.Common.SDK
{
    /// <summary>
    /// Log4Net日志类
    /// 版本：2.0
    /// <author>
    ///		<name>MartyZane</name>
    ///		<date>2014.03.03</date>
    /// </author>
    /// </summary>
    public class LogHelper
    {
        private ILog logger;

        public LogHelper(ILog log)
        {
            this.logger = log;
        }
        public void Debug(object message)
        {
            this.logger.Debug(message);
        }
        public void Debug(object message, Exception e)
        {
            this.logger.Debug(message, e);
        }
        public void Error(object message)
        {
            this.logger.Error(message);
        }
        public void Error(object message, Exception e)
        {
            this.logger.Error(message, e);
        }
    }
}
