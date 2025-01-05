#region Apache License Version 2.0
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

    FileName：ThreadUtility.cs
    File Function Description：Thread utility class


    Creation Identifier：Senparc - 20151226

    Modification Identifier：Senparc - 20180222
    Modification Description：Added lock AsynThreadCollectionLock

----------------------------------------------------------------*/



using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Senparc.CO2NET.Threads
{
    /// <summary>
    /// Thread processing class
    /// </summary>
    public static class ThreadUtility
    {
        /// <summary>
        /// Asynchronous thread container
        /// </summary>
        public static Dictionary<string, Thread> AsynThreadCollection = new Dictionary<string, Thread>();//Background running thread

        private static object AsynThreadCollectionLock = new object();

        /// <summary>
        /// Register thread
        /// </summary>
        public static void Register()
        {
            lock (AsynThreadCollectionLock)
            {
                if (AsynThreadCollection.Count == 0)
                {
                    //Queue thread
                    {
                        SenparcMessageQueueThreadUtility senparcMessageQueue = new SenparcMessageQueueThreadUtility();
                        Thread senparcMessageQueueThread = new Thread(senparcMessageQueue.Run) { Name = "SenparcMessageQueue" };
                        AsynThreadCollection.Add(senparcMessageQueueThread.Name, senparcMessageQueueThread);
                    }

                    AsynThreadCollection.Values.ToList().ForEach(z =>
                    {
                        z.IsBackground = true;
                        z.Start();
                    });//Run all
                }
            }
        }
    }
}
