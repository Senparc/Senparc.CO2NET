//#region Apache License Version 2.0
///*----------------------------------------------------------------

//Copyright 2018 Jeffrey Su & Suzhou Senparc Network Technology Co.,Ltd.

//Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file
//except in compliance with the License. You may obtain a copy of the License at

//http://www.apache.org/licenses/LICENSE-2.0

//Unless required by applicable law or agreed to in writing, software distributed under the
//License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND,
//either express or implied. See the License for the specific language governing permissions
//and limitations under the License.

//Detail: https://github.com/Senparc/Senparc.CO2NET/blob/master/LICENSE

//----------------------------------------------------------------*/
//#endregion Apache License Version 2.0

///*----------------------------------------------------------------
//    Copyright (C) 2018 Senparc
  
//    Filename: SenparcTrace.cs
//    File description: Senparc.CO2NET logging
    
    
//    Creation Identifier: Senparc - 20180602
 
//    Modification Identifier: Senparc - 20180721
//    Modification Description: v0.2.1 Added SenparcTrace.BaseExceptionLog(Exception ex) override method
//----------------------------------------------------------------*/


//using Senparc.CO2NET.Cache;
//using Senparc.CO2NET.Exceptions;
//using System;
//using System.Collections.Generic;
//using System.Diagnostics;
//using System.IO;
//using System.Linq;
//using System.Text;
//using System.Threading;

//namespace Senparc.CO2NET.Trace
//{
//    /// <summary>
//    /// Senparc.CO2NET logging
//    /// </summary>
//    public class SenparcTrace
//    {
//        /// <summary>
//        /// TraceListener
//        /// </summary>
//#if NET35 || NET40 || NET45 || NET461
//        private static TraceListener _traceListener = null;
//#elif NETSTANDARD2_0 || NETCOREAPP2_0 || NETCOREAPP2_1
//        private static TextWriterTraceListener _traceListener = null;
//#endif

//        /// <summary>
//        /// Unified log lock name
//        /// </summary>
//        const string LockName = "SenparcTraceLock";

//        /// <summary>
//        /// Globally unified cache strategy for Senparc.Weixin
//        /// </summary>
//        private static IBaseObjectCacheStrategy Cache
//        {
//            get
//            {
//                //Dynamically load via factory pattern or configuration
//                return CacheStrategyFactory.GetObjectCacheStrategyInstance();
//            }
//        }

//        /// <summary>
//        /// Task to execute when logging BaseException
//        /// </summary>
//        public static Action<BaseException> OnBaseExceptionFunc;

//        /// <summary>
//        /// Task executed after all logging operations complete
//        /// </summary>
//        public static Action OnLogFunc;

//        /// <summary>
//        /// Open logging and start recording
//        /// </summary>
//        internal static void Open()
//        {
//            Close();

//            using (Cache.BeginCacheLock(LockName, ""))
//            {
//                string logDir;
//#if NET35
//                logDir = Path.Combine(Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "App_Data"), "SenparcTraceLog");
//#else

//#if NET40 || NET45 || NET461
//                logDir = System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "App_Data", "SenparcTraceLog");
//#else
//                //var logDir = Path.Combine(AppContext.BaseDirectory, "App_Data", "SenparcTraceLog");
//                logDir = Path.Combine(Senparc.CO2NET.Config.RootDictionaryPath, "App_Data", "SenparcTraceLog");
//#endif
//#endif

//                if (!Directory.Exists(logDir))
//                {
//                    Directory.CreateDirectory(logDir);
//                }

//                string logFile = Path.Combine(logDir, string.Format("SenparcTrace-{0}.log", SystemTime.Now.ToString("yyyyMMdd")));

//#if NET35 || NET40 || NET45 || NET461

//                System.IO.TextWriter logWriter = new System.IO.StreamWriter(logFile, true);
//#elif NETSTANDARD2_0 || NETCOREAPP2_0 || NETCOREAPP2_1
//                System.IO.TextWriter logWriter = new System.IO.StreamWriter(logFile, true);
//#endif


//#if NET35 || NET40 || NET45 || NET461
//                _traceListener = new TextWriterTraceListener(logWriter);
//                System.Diagnostics.Trace.Listeners.Add(_traceListener);
//                System.Diagnostics.Trace.AutoFlush = true;
//#elif NETSTANDARD2_0 || NETCOREAPP2_0 || NETCOREAPP2_1
//                //TODO: if not enabled here, netstandard1.6 cannot use logging
//                //ILoggerFactory loggerFactory = new LoggerFactory();

//                _traceListener = new TextWriterTraceListener(logWriter);
//                System.Diagnostics.Trace.Listeners.Add(_traceListener);
//                System.Diagnostics.Trace.AutoFlush = true;
//#endif

//            }
//        }

//        /// <summary>
//        /// Close logging
//        /// </summary>
//        internal static void Close()
//        {
//            using (Cache.BeginCacheLock(LockName, ""))
//            {
//#if NET35 || NET40 || NET45 || NET461

//                if (_traceListener != null && System.Diagnostics.Trace.Listeners.Contains(_traceListener))
//                {
//                    _traceListener.Close();
//                    System.Diagnostics.Trace.Listeners.Remove(_traceListener);
//                }
//#elif NETSTANDARD2_0 || NETCOREAPP2_0 || NETCOREAPP2_1
//                if (_traceListener != null && System.Diagnostics.Trace.Listeners.Contains(_traceListener))
//                {
//                    _traceListener.Close();
//                    System.Diagnostics.Trace.Listeners.Remove(_traceListener);
//                }
//#endif
//            }
//        }

//        #region Private Methods

//        /// <summary>
//        /// Unified time format
//        /// </summary>
//        private static void TimeLog()
//        {
//            Log("[{0}]", SystemTime.Now);
//        }

//        /// <summary>
//        /// Current thread log
//        /// </summary>
//        private static void ThreadLog()
//        {
//            Log("[Thread:{0}]", Thread.CurrentThread.GetHashCode());
//        }


//        /// <summary>
//        /// Unindent once
//        /// </summary>
//        private static void Unindent()
//        {
//            using (Cache.BeginCacheLock(LockName, ""))
//            {
//#if NET35 || NET40 || NET45 || NET461
//                System.Diagnostics.Trace.Unindent();
//#elif NETSTANDARD2_0 || NETCOREAPP2_0 || NETCOREAPP2_1
//                System.Diagnostics.Trace.Unindent();
//#endif
//            }
//        }

//        /// <summary>
//        /// Indent once
//        /// </summary>
//        private static void Indent()
//        {
//            using (Cache.BeginCacheLock(LockName, ""))
//            {
//#if NET35 || NET40 || NET45 || NET461
//                System.Diagnostics.Trace.Indent();
//#elif NETSTANDARD2_0 || NETCOREAPP2_0 || NETCOREAPP2_1
//                System.Diagnostics.Trace.Indent();
//#endif
//            }
//        }

//        /// <summary>
//        /// Flush cache to system Trace
//        /// </summary>
//        private static void Flush()
//        {
//            using (Cache.BeginCacheLock(LockName, ""))
//            {
//#if NET35 || NET40 || NET45 || NET461
//                System.Diagnostics.Trace.Flush();
//#elif NETSTANDARD2_0 || NETCOREAPP2_0 || NETCOREAPP2_1
//                System.Diagnostics.Trace.Flush();
//#endif
//            }
//        }

//        /// <summary>
//        /// Begin logging
//        /// </summary>
//        /// <param name="title"></param>
//        protected static void LogBegin(string title = null)
//        {
//            Open();
//            Log("");
//            if (title != null)
//            {
//                Log("[{0}]", title);
//            }
//            TimeLog();//Record time
//            ThreadLog();//Record thread
//            Indent();
//        }

//        /// <summary>
//        /// Log message
//        /// </summary>
//        /// <param name="messageFormat">Log message format</param>
//        /// <param name="args">Log message arguments</param>
//        public static void Log(string messageFormat, params object[] args)
//        {
//            using (Cache.BeginCacheLock(LockName, ""))
//            {
//#if NET35 || NET40 || NET45 || NET461
//                System.Diagnostics.Trace.WriteLine(string.Format(messageFormat, args));
//#elif NETSTANDARD2_0 || NETCOREAPP2_0 || NETCOREAPP2_1
//                System.Diagnostics.Trace.WriteLine(string.Format(messageFormat, args));
//#endif
//            }
//        }

//        /// <summary>
//        /// End logging
//        /// </summary>
//        protected static void LogEnd()
//        {
//            Unindent();
//            Flush();
//            Close();

//            if (OnLogFunc != null)
//            {
//                try
//                {
//                    OnLogFunc();
//                }
//                catch
//                {
//                }
//            }
//        }

//        #endregion

//        #region Logging

//        /// <summary>
//        /// <para>Log message (SendXXLog() methods are recommended for unified logging rules)</para>
//        /// <para>Note: calling this method directly writes to system trace, not the log file</para>
//        /// </summary>
//        /// <param name="message">Log message</param>
//        public static void Log(string message)
//        {
//            using (Cache.BeginCacheLock(LockName, ""))
//            {
//#if NET35 || NET40 || NET45 || NET461
//                System.Diagnostics.Trace.WriteLine(message);
//#elif NETSTANDARD2_0 || NETCOREAPP2_0 || NETCOREAPP2_1
//                System.Diagnostics.Trace.WriteLine(message);
//#endif
//            }
//        }


//        /// <summary>
//        /// Custom log
//        /// </summary>
//        /// <param name="typeName">Log type</param>
//        /// <param name="content">Log content</param>
//        public static void SendCustomLog(string typeName, string content)
//        {
//            if (!Config.IsDebug)
//            {
//                return;
//            }

//            LogBegin(string.Format("[[{0}]]", typeName));
//            Log(content);
//            LogEnd();
//        }

//        /// <summary>
//        /// API request log (response received)
//        /// </summary>
//        /// <param name="url"></param>
//        /// <param name="returnText"></param>
//        public static void SendApiLog(string url, string returnText)
//        {
//            if (!Config.IsDebug)
//            {
//                return;
//            }

//            LogBegin("[[API Call]]");
//            //TODO: add AppId at source
//            Log("URL：{0}", url);
//            Log("Result：\r\n{0}", returnText);
//            LogEnd();
//        }

//        /// <summary>
//        /// API request log (Post message sent)
//        /// </summary>
//        /// <param name="url"></param>
//        /// <param name="data"></param>
//        public static void SendApiPostDataLog(string url, string data)
//        {
//            if (!Config.IsDebug)
//            {
//                return;
//            }

//            LogBegin("[[API Call]]");
//            Log("URL：{0}", url);
//            Log("Post Data：\r\n{0}", data);
//            LogEnd();
//        }


//        #endregion

//        #region BaseException


//        /// <summary>
//        /// BaseException log
//        /// </summary>
//        /// <param name="ex"></param>
//        public static void BaseExceptionLog(Exception ex)
//        {
//            BaseExceptionLog(new BaseException(ex.Message, ex));
//        }

//        /// <summary>
//        /// BaseException log
//        /// </summary>
//        /// <param name="ex"></param>
//        public static void BaseExceptionLog(BaseException ex)
//        {
//            if (!Config.IsDebug)
//            {
//                return;
//            }

//            LogBegin("[[BaseException]]");
//            Log(ex.GetType().Name);
//            Log("Message：{0}", ex.Message);
//            Log("StackTrace：{0}", ex.StackTrace);
//            if (ex.InnerException != null)
//            {
//                Log("InnerException：{0}", ex.InnerException.Message);
//                Log("InnerException.StackTrace：{0}", ex.InnerException.StackTrace);
//            }

//            if (OnBaseExceptionFunc != null)
//            {
//                try
//                {
//                    OnBaseExceptionFunc(ex);
//                }
//                catch
//                {
//                }
//            }

//            LogEnd();
//        }

//        #endregion
//    }
//}
