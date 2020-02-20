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
    Copyright (C) 2020 Senparc
    
    文件名：FileHelper.cs
    文件功能描述：处理文件
    
    创建标识：Senparc - 20150211
    
    修改标识：Senparc - 20150303
    修改描述：整理接口
    
    修改标识：Senparc - 20161108
    修改描述：将Senparc.Weixin.MP中的FileHelper转移到Senparc.Weixin中，并添加DownLoadFileFromUrl方法

    修改标识：Senparc - 20170204
    修改描述：v4.10.4 优化GetFileStream方法


    ----  CO2NET   ----
    ----  split from Senparc.Weixin/Helpers/FileHelper.cs  ----

    修改标识：Senparc - 20180601
    修改描述：v0.1.0 移植 FileHelper

    修改标识：Senparc - 20190811
    修改描述：v0.8.6 添加 FileHelper.FileInUse() 方法，用于判断文件是否正在被占用

----------------------------------------------------------------*/



using System;
using System.IO;

namespace Senparc.CO2NET.Helpers
{
    /// <summary>
    /// 文件帮助类
    /// </summary>
    public class FileHelper
    {
        /// <summary>
        /// 根据完整文件路径获取FileStream
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static FileStream GetFileStream(string fileName)
        {
            FileStream fileStream = null;
            if (!string.IsNullOrEmpty(fileName) && File.Exists(fileName))
            {
                fileStream = new FileStream(fileName, FileMode.Open,FileAccess.Read, FileShare.ReadWrite);
            }
            return fileStream;
        }

        /// <summary>
        /// 从Url下载文件
        /// </summary>
        /// <param name="url"></param>
        /// <param name="fullFilePathAndName"></param>
        public static void DownLoadFileFromUrl(IServiceProvider serviceProvider,string url, string fullFilePathAndName)
        {
            using (FileStream fs = new FileStream(fullFilePathAndName, FileMode.OpenOrCreate))
            {
                HttpUtility.Get.Download(
                    serviceProvider,
                    url, fs);
#if NET35
                fs.Flush();
#else
                fs.Flush(true);
#endif
            }
        }

        /// <summary>
        /// 判断文件是否正在被使用
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <returns></returns>
        public static bool FileInUse(string filePath)
        {
            try
            {
                if (!System.IO.File.Exists(filePath)) // The path might also be invalid.
                {
                    return false;
                }

                using (System.IO.FileStream stream = new System.IO.FileStream(filePath, System.IO.FileMode.Open))
                {
                    return false;
                }
            }
            catch
            {
                return true;
            }
        }
    }
}
