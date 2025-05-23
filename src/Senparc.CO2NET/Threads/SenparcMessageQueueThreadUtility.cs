﻿#region Apache License Version 2.0
/*----------------------------------------------------------------

Copyright 2025 Suzhou Senparc Network Technology Co.,Ltd.

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
    Copyright (C) 2025 Senparc

    FileName：SenparcMessageQueueThreadUtility.cs
    File Function Description：SenparcMessageQueue message queue thread processing


    Creation Identifier：Senparc - 20160210

----------------------------------------------------------------*/


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Senparc.CO2NET.MessageQueue;

namespace Senparc.CO2NET.Threads
{
    /// <summary>
    /// SenparcMessageQueue thread auto-processing
    /// </summary>
    public class SenparcMessageQueueThreadUtility
    {
        private readonly int _sleepMilliSeconds;


        public SenparcMessageQueueThreadUtility(int sleepMilliSeconds = 500)
        {
            _sleepMilliSeconds = sleepMilliSeconds;
        }

        /// <summary>
        /// Destructor, process the unhandled queue
        /// </summary>
        ~SenparcMessageQueueThreadUtility()
        {
            try
            {
                var mq = new SenparcMessageQueue();

#if NET462
                System.Diagnostics.Trace.WriteLine(string.Format("SenparcMessageQueueThreadUtility执行析构函数"));
                System.Diagnostics.Trace.WriteLine(string.Format("当前队列数量：{0}", mq.GetCount()));
#endif

                SenparcMessageQueue.OperateQueue();//Process the queue
            }
            catch (Exception ex)
            {
                //Logs can be added here
#if NET462

                System.Diagnostics.Trace.WriteLine(string.Format("SenparcMessageQueueThreadUtility执行析构函数错误：{0}", ex.Message));
#endif
            }
        }

        /// <summary>
        /// Start thread polling
        /// </summary>
        public void Run()
        {
            do
            {
                SenparcMessageQueue.OperateQueue();
                Thread.Sleep(_sleepMilliSeconds);
            } while (true);
        }
    }
}
