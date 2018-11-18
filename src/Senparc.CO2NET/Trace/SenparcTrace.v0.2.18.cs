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
  
//    文件名：SenparcTrace.cs
//    文件功能描述：Senparc.CO2NET 日志记录
    
    
//    创建标识：Senparc - 20180602
 
//    修改标识：Senparc - 20180721
//    修改描述：v0.2.1 增加 SenparcTrace.BaseExceptionLog(Exception ex) 重写方法
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
//    /// Senparc.CO2NET 日志记录
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
//        /// 统一日志锁名称
//        /// </summary>
//        const string LockName = "SenparcTraceLock";

//        /// <summary>
//        /// Senparc.Weixin全局统一的缓存策略
//        /// </summary>
//        private static IBaseObjectCacheStrategy Cache
//        {
//            get
//            {
//                //使用工厂模式或者配置进行动态加载
//                return CacheStrategyFactory.GetObjectCacheStrategyInstance();
//            }
//        }

//        /// <summary>
//        /// 记录BaseException日志时需要执行的任务
//        /// </summary>
//        public static Action<BaseException> OnBaseExceptionFunc;

//        /// <summary>
//        /// 执行所有日志记录操作时执行的任务（发生在记录日志之后）
//        /// </summary>
//        public static Action OnLogFunc;

//        /// <summary>
//        /// 打开日志开始记录
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
//                //TODO:如果这里不开通，netstandard1.6将无法使用日志记录功能
//                //ILoggerFactory loggerFactory = new LoggerFactory();

//                _traceListener = new TextWriterTraceListener(logWriter);
//                System.Diagnostics.Trace.Listeners.Add(_traceListener);
//                System.Diagnostics.Trace.AutoFlush = true;
//#endif

//            }
//        }

//        /// <summary>
//        /// 关闭日志记录
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

//        #region 私有方法

//        /// <summary>
//        /// 统一时间格式
//        /// </summary>
//        private static void TimeLog()
//        {
//            Log("[{0}]", SystemTime.Now);
//        }

//        /// <summary>
//        /// 当前线程记录
//        /// </summary>
//        private static void ThreadLog()
//        {
//            Log("[线程：{0}]", Thread.CurrentThread.GetHashCode());
//        }


//        /// <summary>
//        /// 退回一次缩进
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
//        /// 缩进一次
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
//        /// 写入缓存到系统Trace
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
//        /// 开始记录日志
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
//            TimeLog();//记录时间
//            ThreadLog();//记录线程
//            Indent();
//        }

//        /// <summary>
//        /// 记录日志
//        /// </summary>
//        /// <param name="messageFormat">日志内容格式</param>
//        /// <param name="args">日志内容参数</param>
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
//        /// 结束日志记录
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

//        #region 日志记录

//        /// <summary>
//        /// <para>记录日志（建议使用SendXXLog()方法，以符合统一的记录规则）</para>
//        /// <para>注意：直接调用此方法不会记录到log文件中，而是输出到系统日志中</para>
//        /// </summary>
//        /// <param name="message">日志内容</param>
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
//        /// 自定义日志
//        /// </summary>
//        /// <param name="typeName">日志类型</param>
//        /// <param name="content">日志内容</param>
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
//        /// API请求日志（接收结果）
//        /// </summary>
//        /// <param name="url"></param>
//        /// <param name="returnText"></param>
//        public static void SendApiLog(string url, string returnText)
//        {
//            if (!Config.IsDebug)
//            {
//                return;
//            }

//            LogBegin("[[接口调用]]");
//            //TODO:从源头加入AppId
//            Log("URL：{0}", url);
//            Log("Result：\r\n{0}", returnText);
//            LogEnd();
//        }

//        /// <summary>
//        /// API请求日志（Post发送消息）
//        /// </summary>
//        /// <param name="url"></param>
//        /// <param name="data"></param>
//        public static void SendApiPostDataLog(string url, string data)
//        {
//            if (!Config.IsDebug)
//            {
//                return;
//            }

//            LogBegin("[[接口调用]]");
//            Log("URL：{0}", url);
//            Log("Post Data：\r\n{0}", data);
//            LogEnd();
//        }


//        #endregion

//        #region BaseException


//        /// <summary>
//        /// BaseException 日志
//        /// </summary>
//        /// <param name="ex"></param>
//        public static void BaseExceptionLog(Exception ex)
//        {
//            BaseExceptionLog(new BaseException(ex.Message, ex));
//        }

//        /// <summary>
//        /// BaseException 日志
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
