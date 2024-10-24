#region Apache License Version 2.0
/*----------------------------------------------------------------

Copyright 2024 Suzhou Senparc Network Technology Co.,Ltd.

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
  
    FileName：SenparcTrace.cs
    File Function Description：Senparc.CO2NET Logging
    
    
    Creation Identifier：Senparc - 20180602
 
    Modification Identifier：Senparc - 20180721
    Modification Description：v0.2.1 Added SenparcTrace.BaseExceptionLog(Exception ex) override method
 
    Modification Identifier：Senparc - 201801118
    Modification Description：v0.3.0 Upgraded SenparcTrace to use queue

    Modification Identifier：Senparc - 20181227
    Modification Description：v0.4.4 Provided SenparcTrace.RecordAPMLog parameter

    Modification Identifier：Senparc - 20181227
    Modification Description：v0.8.9 Provided AutoUnlockLogFile parameter, and attempted to auto-unlock in case the log file is occupied

    Modification Identifier：Senparc - 20181227
    Modification Description：v0.8.9 Provided AutoUnlockLogFile parameter, and attempted to auto-unlock in case the log file is occupied

----------------------------------------------------------------*/


using Senparc.CO2NET.Cache;
using Senparc.CO2NET.Exceptions;
using Senparc.CO2NET.Helpers;
using Senparc.CO2NET.MessageQueue;
using System;
using System.IO;
using System.Threading;

namespace Senparc.CO2NET.Trace
{
    /// <summary>
    /// Senparc.CO2NET Logging
    /// </summary>
    public class SenparcTrace
    {
        /// <summary>
        /// Unified log lock name
        /// </summary>
        const string LockName = "SenparcTraceLock";


        /// <summary>
        /// Task to be executed when recording BaseException log
        /// </summary>
        public static Action<BaseException> OnBaseExceptionFunc;

        /// <summary>
        /// Task executed when performing all log recording operations (occurs after logging)
        /// </summary>
        public static Action OnLogFunc;

        /// <summary>
        /// Whether to enable recording for each APM entry, default is off (effective when Senparc.CO2ENT.APM is enabled)
        /// </summary>
        public static bool RecordAPMLog { get; set; } = false;

        /// <summary>
        /// Whether to automatically unlock the log file that may be occupied, if true, it will be enabled (GC operation will be performed immediately upon triggering, consuming some system resources, generally not affecting overall system performance), if false, an exception will be thrown when file usage conflict occurs, abandoning log writing.
        /// (Note: If multiple sites or applications use the same log file directory, be sure to enable this option)
        /// </summary>
        public static bool AutoUnlockLogFile { get; set; } = true;

        #region Private Methods

        /// <summary>
        /// Senparc.Weixin global unified caching strategy
        /// </summary>
        private static IBaseObjectCacheStrategy Cache
        {
            get
            {
                //Use factory pattern or configuration for dynamic loading
                return CacheStrategyFactory.GetObjectCacheStrategyInstance();
            }
        }

        /// <summary>
        /// Queue execution logic
        /// </summary>
        protected static Action<string> _queue = async (logStr) =>
        {
            using (await Cache.BeginCacheLockAsync(LockName, "").ConfigureAwait(false))
            {
                string logDir;
#if NET462
                logDir = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "App_Data", "SenparcTraceLog");
#else
                //var logDir = Path.Combine(AppContext.BaseDirectory, "App_Data", "SenparcTraceLog");
                logDir = Path.Combine(Senparc.CO2NET.Config.RootDirectoryPath, "App_Data", "SenparcTraceLog");
#endif

                if (!Directory.Exists(logDir))
                {
                    Directory.CreateDirectory(logDir);
                }

                //TODO: Can be merged for writing

                string logFile = Path.Combine(logDir, string.Format("SenparcTrace-{0}.log", SystemTime.Now.ToString("yyyyMMdd")));


                //Check file occupation status
                if (AutoUnlockLogFile)
                {
                    const int maxRetryTimes = 3;//Maximum retry count
                    const int retryDelayTimeMillinSeconds = 100;//Wait time after each retry (milliseconds)

                    //Immediate recycling
                    for (int i = 0; i < maxRetryTimes; i++)
                    {
                        if (FileHelper.FileInUse(logFile))
                        {
                            //Description:
                            //1. There are many methods to recycle file occupation, the following two GC commands are used simultaneously as referenced here: https://stackoverflow.com/questions/4128211/system-io-ioexception-the-process-cannot-access-the-file-file-name
                            //2. For .NET Core, if there are better differentiated methods, PRs are welcome: https://github.com/JeffreySu/WeiXinMPSDK.
                            GC.Collect();
                            GC.WaitForPendingFinalizers();

                            var dt = SystemTime.Now;
                            if (i < maxRetryTimes - 1)
                            {
                                while (SystemTime.NowDiff(dt).TotalMilliseconds < retryDelayTimeMillinSeconds)
                                {
                                    //If not the last attempt, wait for a while before proceeding to the next step
                                }
                            }
                        }
                        else
                        {
                            break;
                        }
                    }
                }

                try
                {
                    using (var fs = new FileStream(logFile, FileMode.OpenOrCreate))
                    {
                        using (var sw = new StreamWriter(fs))
                        {
                            fs.Seek(0, SeekOrigin.End);
                            await sw.WriteAsync(logStr);
                            await sw.FlushAsync();
                        }
                    }
                }
                catch (Exception)
                {
                    //Write failed
                    //throw;
                }


                if (OnLogFunc != null)
                {
                    try
                    {
                        OnLogFunc();
                    }
                    catch
                    {
                    }
                }
            }
        };

        /// <summary>
        /// End logging
        /// </summary>
        protected static Action<SenparcTraceItem> _logEndActon = (traceItem) =>
        {
            var logStr = traceItem.GetFullLog();
            SenparcMessageQueue messageQueue = new SenparcMessageQueue();
            var key = $"{SystemTime.Now.Ticks.ToString()}{traceItem.ThreadId.ToString()}{logStr.Length.ToString()}";//Ensure global uniqueness
            messageQueue.Add(key, () => _queue(logStr));
        };

        #endregion

        #region Logging

        /// <summary>
        /// System log
        /// </summary>
        /// <param name="message">Log content</param>
        public static void Log(string message)
        {
            SendCustomLog("系统日志", message);
        }

        /// <summary>
        /// Custom log
        /// </summary>
        /// <param name="typeName">Log type</param>
        /// <param name="content">Log content</param>
        public static void SendCustomLog(string typeName, string content)
        {
            if (!Config.IsDebug)
            {
                return;
            }

            using (var traceItem = new SenparcTraceItem(_logEndActon, typeName, content))
            {
                //traceItem.Log(content);
            }
        }

        /// <summary>
        /// API request log (receive result)
        /// </summary>
        /// <param name="url"></param>
        /// <param name="returnText"></param>
        public static void SendApiLog(string url, string returnText)
        {
            if (!Config.IsDebug)
            {
                return;
            }

            using (var traceItem = new SenparcTraceItem(_logEndActon, "接口调用"))
            {
                //TODO: Add AppId from the source
                traceItem.Log("URL：{0}", url);
                traceItem.Log("Result：\r\n{0}", returnText);
            }
        }

        /// <summary>
        /// API request log (Post send message)
        /// </summary>
        /// <param name="url"></param>
        /// <param name="data"></param>
        public static void SendApiPostDataLog(string url, string data)
        {
            if (!Config.IsDebug)
            {
                return;
            }

            using (var traceItem = new SenparcTraceItem(_logEndActon, "接口调用"))
            {
                traceItem.Log("URL：{0}", url);
                traceItem.Log("Post Data：\r\n{0}", data);
            }
        }


        #endregion

        #region BaseException


        /// <summary>
        /// BaseException log
        /// </summary>
        /// <param name="ex"></param>
        public static void BaseExceptionLog(Exception ex)
        {
            BaseExceptionLog(new BaseException(ex.Message, ex));
        }

        /// <summary>
        /// BaseException log
        /// </summary>
        /// <param name="ex"></param>
        public static void BaseExceptionLog(BaseException ex)
        {
            if (!Config.IsDebug)
            {
                return;
            }


            using (var traceItem = new SenparcTraceItem(_logEndActon, "BaseException"))
            {
                traceItem.Log(ex.GetType().Name);
                traceItem.Log("Message：{0}", ex.Message);
                traceItem.Log("StackTrace：{0}", ex.StackTrace);

                if (ex.InnerException != null)
                {
                    traceItem.Log("InnerException：{0}", ex.InnerException.Message);
                    traceItem.Log("InnerException.StackTrace：{0}", ex.InnerException.StackTrace);
                }

                if (OnBaseExceptionFunc != null)
                {
                    try
                    {
                        OnBaseExceptionFunc(ex);
                    }
                    catch
                    {
                    }
                }
            }
        }

        #endregion
    }
}
