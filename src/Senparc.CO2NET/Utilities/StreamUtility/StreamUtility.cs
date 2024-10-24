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
    
    FileName：StreamUtility.cs
    File Function Description：Stream processing utility class
    
    
    Creation Identifier：Senparc - 20150419
    

    ----  CO2NET   ----
    ----  split from Senparc.Weixin/Utilities/StreamUtility/StreamUtility.cs  ----

    Modification Identifier：Senparc - 20180602
    Modification Description：v0.1.0 Ported StreamUtility

----------------------------------------------------------------*/



using System;
using System.IO;
using System.Threading.Tasks;

namespace Senparc.CO2NET.Utilities
{
    /// <summary>
    /// Stream utility class
    /// </summary>
    public static class StreamUtility
    {
        #region 同步方法

        /// <summary>
        /// Get Base64 string of Stream
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static string GetBase64String(Stream stream)
        {
            byte[] arr = new byte[stream.Length];
            stream.Position = 0;
            stream.Read(arr, 0, (int)stream.Length);
#if NET462
            return Convert.ToBase64String(arr, Base64FormattingOptions.None);
#else
            return Convert.ToBase64String(arr);
#endif
        }

        /// <summary>
        /// Deserialize base64String to stream, or save as file
        /// </summary>
        /// <param name="base64String"></param>
        /// <param name="savePath">If null, do not save</param>
        /// <returns></returns>
        public static Stream GetStreamFromBase64String(string base64String, string savePath)
        {
            byte[] bytes = Convert.FromBase64String(base64String);

            var memoryStream = new MemoryStream(bytes, 0, bytes.Length);
            memoryStream.Write(bytes, 0, bytes.Length);

            if (!string.IsNullOrEmpty(savePath))
            {
                SaveFileFromStream(memoryStream, savePath);
            }

            memoryStream.Seek(0, SeekOrigin.Begin);
            return memoryStream;
        }

        /// <summary>
        /// Save memoryStream to file
        /// </summary>
        /// <param name="memoryStream"></param>
        /// <param name="savePath"></param>
        public static void SaveFileFromStream(MemoryStream memoryStream, string savePath)
        {
            memoryStream.Seek(0, SeekOrigin.Begin);
            using (var localFile = new FileStream(savePath, FileMode.OpenOrCreate))
            {
                localFile.Write(memoryStream.ToArray(), 0, (int)memoryStream.Length);
            }
        }

        #endregion

        #region 异步方法

        /// <summary>
        /// [Async method] Get Base64 string of Stream
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static async Task<string> GetBase64StringAsync(Stream stream)
        {
            byte[] arr = new byte[stream.Length];
            stream.Position = 0;
            await stream.ReadAsync(arr, 0, (int)stream.Length).ConfigureAwait(false);
#if NET462
            return Convert.ToBase64String(arr, Base64FormattingOptions.None);
#else
            return Convert.ToBase64String(arr);
#endif
        }

        /// <summary>
        /// [Async method] Deserialize base64String to stream, or save as file
        /// </summary>
        /// <param name="base64String"></param>
        /// <param name="savePath">If null, do not save</param>
        /// <returns></returns>
        public static async Task<Stream> GetStreamFromBase64StringAsync(string base64String, string savePath)
        {
            byte[] bytes = Convert.FromBase64String(base64String);

            var memoryStream = new MemoryStream(bytes, 0, bytes.Length);
            await memoryStream.WriteAsync(bytes, 0, bytes.Length).ConfigureAwait(false);

            if (!string.IsNullOrEmpty(savePath))
            {
                await SaveFileFromStreamAsync(memoryStream, savePath).ConfigureAwait(false);
            }

            memoryStream.Seek(0, SeekOrigin.Begin);
            return memoryStream;
        }

        /// <summary>
        /// [Async method] Save memoryStream to file
        /// </summary>
        /// <param name="memoryStream"></param>
        /// <param name="savePath"></param>
        public static async Task SaveFileFromStreamAsync(MemoryStream memoryStream, string savePath)
        {
            memoryStream.Seek(0, SeekOrigin.Begin);
            using (var localFile = new FileStream(savePath, FileMode.OpenOrCreate))
            {
                await localFile.WriteAsync(memoryStream.ToArray(), 0, (int)memoryStream.Length).ConfigureAwait(false);
            }
        }

        #endregion

    }
}
