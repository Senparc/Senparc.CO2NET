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

    FileName：SenparcTraceItem.cs
    File Function Description：Object information for each trace log


    Creation Identifier：Senparc - 20180602

    Modification Identifier：Senparc - 20181226
    Modification Description：v0.4.3 Changed DateTime to DateTimeOffset

 ----------------------------------------------------------------*/

using Senparc.CO2NET.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Senparc.CO2NET.Trace
{
    /// <summary>
    /// Object information for each trace log
    /// </summary>
    public class SenparcTraceItem : IDisposable
    {
        public string Title { get; set; }
        public string Content { get; set; }
        public DateTimeOffset DateTime { get; set; }

        public int ThreadId { get; set; } = Thread.CurrentThread.GetHashCode();

        private Action<SenparcTraceItem> _logEndAction;

        public SenparcTraceItem(Action<SenparcTraceItem> logEndAction, string title = null, string content = null)
        {
            _logEndAction = logEndAction;
            Title = title;
            Content = content;
            DateTime = SystemTime.Now;
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
        /// Get the complete string information of a single log
        /// </summary>
        public string GetFullLog()
        {
            string logStr = $@"[[[{Title}]]]
[{DateTime.ToString("yyyy/MM/dd HH:mm:ss.ffff")}]
[线程：{ThreadId}]
{Content}

";
            return logStr;
        }

        public void Dispose()
        {
            _logEndAction?.Invoke(this);
        }
    }
}