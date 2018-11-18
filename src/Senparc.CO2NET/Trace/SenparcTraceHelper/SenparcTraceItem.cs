using Senparc.CO2NET.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Senparc.CO2NET.Trace
{
    /// <summary>
    /// 每一次跟踪日志的对象信息
    /// </summary>
    public class SenparcTraceItem : IDisposable
    {
        public string Title { get; set; }
        public DateTime DateTime { get; set; }
        public string Content { get; set; }

        public int ThreadId { get; set; } = Thread.CurrentThread.GetHashCode();

        private Action<SenparcTraceItem> _logEndAction;

        public SenparcTraceItem(Action<SenparcTraceItem> logEndAction, string title = null, string content = null)
        {
            _logEndAction = logEndAction;
            Title = title;
            Content = content;
        }

        public void Log(string messageFormat, params object[] param)
        {
            Log(messageFormat.FormatWith(param));
        }

        public void Log(string message)
        {
            if (Content != null)
            {
                Content += System.Environment.NewLine;
            }
            Content += $"\t{message}";
        }

        /// <summary>
        /// 获取完整单条日志的字符串信息
        /// </summary>
        public string GetFullLog()
        {
            string logStr = $@"[[[{Title}]]]
[{DateTime.ToString("yyyy/MM/dd HH:mm:ss.ffff")}]
[线程：{ThreadId}]
{Content}";
            return logStr;
        }

        public void Dispose()
        {
            _logEndAction?.Invoke(this);
        }
    }
}
