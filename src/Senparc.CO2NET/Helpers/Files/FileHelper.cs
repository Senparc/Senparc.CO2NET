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
    
    FileName：FileHelper.cs
    File Function Description：Handle files
    
    Creation Identifier：Senparc - 20150211
    
    Modification Identifier：Senparc - 20150303
    Modification Description：Organize interface
    
    Modification Identifier：Senparc - 20161108
    Modification Description：Moved FileHelper from Senparc.Weixin.MP to Senparc.Weixin and added DownLoadFileFromUrl method

    Modification Identifier：Senparc - 20170204
    Modification Description：v4.10.4 Optimized GetFileStream method


    ----  CO2NET   ----
    ----  split from Senparc.Weixin/Helpers/FileHelper.cs  ----

    Modification Identifier：Senparc - 20180601
    Modification Description：v0.1.0 Ported FileHelper

    Modification Identifier：Senparc - 20190811
    Modification Description：v0.8.6 Added FileHelper.FileInUse() method to determine if a file is in use

    Modification Identifier：Senparc - 20200530
    Modification Description：v1.3.110 Added FileHelper.TryCreateDirectory() method

    Modification Identifier：Senparc - 20210831
    Modification Description：v1.5.1 Added GetFileHash method in FileHelper

    Modification Identifier：qideqian - 20220721
    Modification Description：v2.1.2 Fixed bug in FileHelper.GetFileHash()

----------------------------------------------------------------*/


using System;
using System.IO;
using System.Text;

namespace Senparc.CO2NET.Helpers
{
    /// <summary>
    /// File helper class
    /// </summary>
    public class FileHelper
    {
        /// <summary>
        /// Get FileStream based on full file path
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static FileStream GetFileStream(string fileName)
        {
            FileStream fileStream = null;
            if (!string.IsNullOrEmpty(fileName) && File.Exists(fileName))
            {
                fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            }
            return fileStream;
        }

        /// <summary>
        /// Download file from URL
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <param name="url"></param>
        /// <param name="fullFilePathAndName"></param>
        public static void DownLoadFileFromUrl(IServiceProvider serviceProvider, string url, string fullFilePathAndName)
        {
            using (FileStream fs = new FileStream(fullFilePathAndName, FileMode.OpenOrCreate))
            {
                HttpUtility.Get.Download(
                    serviceProvider,
                    url, fs);
                fs.Flush(true);
            }
        }

        /// <summary>
        /// Determine if the file is in use
        /// </summary>
        /// <param name="filePath">File path</param>
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

        /// <summary>
        /// Create directory if it does not exist
        /// </summary>
        /// <param name="dir">Absolute directory path</param>
        public static void TryCreateDirectory(string dir)
        {
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
        }

        #region 文件指纹


        /// <summary>
        /// Get the HASH value of the file
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="type">SHA1 or MD5, must be uppercase</param>
        /// <param name="toUpper">Return uppercase result, true: uppercase, false: lowercase</param>
        public static string GetFileHash(string filePath, string type = "SHA1", bool toUpper = true)
        {
            var stream = GetFileStream(filePath);
            try
            {
                return GetFileHash(stream, type, toUpper);
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                stream.Close();
            }
        }

        /// <summary>
        /// Get the HASH value of the file
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="type">SHA1 or MD5 or CRC32, must be uppercase</param>
        /// <param name="toUpper">Return uppercase result, true: uppercase, false: lowercase</param>
        public static string GetFileHash(Stream stream, string type = "SHA1", bool toUpper = true)
        {
            switch (type)
            {
                case "SHA1":
                    {
                        return EncryptHelper.GetSha1(stream, toUpper);
                    }
                case "MD5":
                    {
                        return EncryptHelper.GetMD5(stream, toUpper);
                    }
                case "CRC32":
                    {
                        return EncryptHelper.GetCrc32(stream, toUpper);
                    }
                default:
                    throw new ArgumentOutOfRangeException(nameof(type));
            }
        }

        #endregion
    }
}
