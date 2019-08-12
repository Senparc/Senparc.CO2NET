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
    Copyright (C) 2019 Senparc
  
    文件名：SenparcTrace.cs
    文件功能描述：Senparc.CO2NET 日志记录
    
    
    创建标识：Senparc - 20180602
 
    修改标识：Senparc - 20180721
    修改描述：v0.2.1 增加 SenparcTrace.BaseExceptionLog(Exception ex) 重写方法
 
    修改标识：Senparc - 201801118
    修改描述：v0.3.0 升级 SenparcTrace，使用队列

    修改标识：Senparc - 20181227
    修改描述：v0.4.4 提供 SenparcTrace.RecordAPMLog 参数

----------------------------------------------------------------*/


using Senparc.CO2NET.Cache;
using Senparc.CO2NET.Exceptions;
using Senparc.CO2NET.MessageQueue;
using System;
using System.IO;

namespace Senparc.CO2NET.Trace
{
    /// <summary>
    /// Senparc.CO2NET 日志记录
    /// </summary>
    public class SenparcTrace
    {
        /// <summary>
        /// 统一日志锁名称
        /// </summary>
        const string LockName = "SenparcTraceLock";


        /// <summary>
        /// 记录BaseException日志时需要执行的任务
        /// </summary>
        public static Action<BaseException> OnBaseExceptionFunc;

        /// <summary>
        /// 执行所有日志记录操作时执行的任务（发生在记录日志之后）
        /// </summary>
        public static Action OnLogFunc;

        /// <summary>
        /// 是否开放每次 APM 录入的记录，默认为关闭（当 Senparc.CO2ENT.APM 启用时有效）
        /// </summary>
        public static bool RecordAPMLog = false;

        #region 私有方法

        /// <summary>
        /// Senparc.Weixin全局统一的缓存策略
        /// </summary>
        private static IBaseObjectCacheStrategy Cache
        {
            get
            {
                //使用工厂模式或者配置进行动态加载
                return CacheStrategyFactory.GetObjectCacheStrategyInstance();
            }
        }

        /// <summary>
        /// 队列执行逻辑
        /// </summary>
        protected static Action<string> _queue = async (logStr) =>
        {
            using (await Cache.BeginCacheLockAsync(LockName, ""))
            {
                string logDir;
#if NET35
                logDir = Path.Combine(Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "App_Data"), "SenparcTraceLog");
#else

#if NET40 || NET45 || NET461
                logDir = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "App_Data", "SenparcTraceLog");
#else
                //var logDir = Path.Combine(AppContext.BaseDirectory, "App_Data", "SenparcTraceLog");
                logDir = Path.Combine(Senparc.CO2NET.Config.RootDictionaryPath, "App_Data", "SenparcTraceLog");
#endif
#endif

                if (!Directory.Exists(logDir))
                {
                    Directory.CreateDirectory(logDir);
                }

                //TODO：可以进行合并写入

                string logFile = Path.Combine(logDir, string.Format("SenparcTrace-{0}.log", SystemTime.Now.ToString("yyyyMMdd")));
                //TODO:判断文件被占用情况

                using (var fs = new FileStream(logFile, FileMode.OpenOrCreate))
                {
                    using (var sw = new StreamWriter(fs))
                    {
                        fs.Seek(0, SeekOrigin.End);
                        await sw.WriteAsync(logStr);
                        await sw.FlushAsync();
                    }
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
        /// 结束日志记录
        /// </summary>
        protected static Action<SenparcTraceItem> _logEndActon = (traceItem) =>
        {
            var logStr = traceItem.GetFullLog();
            SenparcMessageQueue messageQueue = new SenparcMessageQueue();
            var key = $"{SystemTime.Now.Ticks.ToString()}{traceItem.ThreadId.ToString()}{logStr.Length.ToString()}";//确保全局唯一
            messageQueue.Add(key, () => _queue(logStr));
        };

        #endregion

        #region 日志记录

        /// <summary>
        /// 系统日志
        /// </summary>
        /// <param name="message">日志内容</param>
        public static void Log(string message)
        {
            SendCustomLog("系统日志", message);
        }

        /// <summary>
        /// 自定义日志
        /// </summary>
        /// <param name="typeName">日志类型</param>
        /// <param name="content">日志内容</param>
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
        /// API请求日志（接收结果）
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
                //TODO:从源头加入AppId
                traceItem.Log("URL：{0}", url);
                traceItem.Log("Result：\r\n{0}", returnText);
            }
        }

        /// <summary>
        /// API请求日志（Post发送消息）
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
        /// BaseException 日志
        /// </summary>
        /// <param name="ex"></param>
        public static void BaseExceptionLog(Exception ex)
        {
            BaseExceptionLog(new BaseException(ex.Message, ex));
        }

        /// <summary>
        /// BaseException 日志
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
