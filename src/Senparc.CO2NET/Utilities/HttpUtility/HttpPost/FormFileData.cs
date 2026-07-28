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
    Copyright (C) 2026 Senparc

    Filename: FormFileData.cs
    File description: Parsing information for fileDictionary Value in RequestUtility.Post when simulating Form submission with base64 file stream data


    Creation Identifier: Senparc - 20190811
    
----------------------------------------------------------------*/

using Senparc.CO2NET.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Senparc.CO2NET.Utilities.HttpUtility.HttpPost
{
    /// <summary>
    /// Form submission
    /// </summary>
    public class FormFileData
    {
        /// <summary>
        /// In Post, the fileDictionary Value may provide a base64-encoded data stream and must match this format
        /// </summary>
        public const string FILE_DICTIONARY_STREAM_FORMAT = "{0}||{1}";

        /// <summary>
        /// File name marker used for Form submission
        /// </summary>
        public string FileName { get; set; }
        /// <summary>
        /// Base64-encoded file content
        /// </summary>
        public string FileBase64 { get; set; }


        public FormFileData() { }

        public FormFileData(string fileValue)
        {
            FillFromFileValue(fileValue);
        }

        public FormFileData(string fileName,Stream stream)
        {
            FileName = fileName;
            SetFileBase64FromStream(stream);
        }


        /// <summary>
        /// Get the Base64 value for FileDictionary in RequestUtility.Post from a file stream
        /// </summary>
        /// <param name="fileStream"></param>
        /// <returns></returns>
        public void SetFileBase64FromStream(Stream fileStream)
        {
            if (string.IsNullOrWhiteSpace(FileName))
            {
                throw new Exceptions.FileValueException(this, "FileName 不能为 null！");
            }

            fileStream.Seek(0, SeekOrigin.Begin);

            //Method 1
            byte[] fileBytes = new byte[fileStream.Length];
            fileStream.Read(fileBytes, 0, fileBytes.Length);

            //Method 2
            //BinaryReader r = new BinaryReader(fileStream);
            //r.BaseStream.Seek(0, SeekOrigin.Begin);
            //var fileBytes = r.ReadBytes((int)r.BaseStream.Length);//TODO: do not use int to limit long length

            FileBase64 = Convert.ToBase64String(fileBytes);
        }

        /// <summary>
        /// Get FileName, Base64, and other parameters from FileValue
        /// </summary>
        /// <param name="fileValue">Value of FileDictionary</param>
        public void FillFromFileValue(string fileValue)
        {
            if (string.IsNullOrWhiteSpace(fileValue))
            {
                throw new Exceptions.FileValueException(this, "fileValue 不能为 null！");
            }

            var values = fileValue.Split(new[] { "||" }, StringSplitOptions.None);
            if (values.Length>1)
            {
                FileName = values[0];
                FileBase64 = values[1];
            }
            else
            {
                FileBase64 = values[0];
            }

            //TODO: validation can be added
        }

        /// <summary>
        /// Try to load Base64 into stream; performs Base64 validation and returns false on failure
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public async Task<bool> TryLoadStream(Stream stream)
        {
            if (stream == null)
            {
                throw new Exceptions.FileValueException(this, "stream 不能为 null！");
            }

            if (string.IsNullOrWhiteSpace(FileBase64))
            {
                throw new Exceptions.FileValueException(this, "FileBase64 不能为 null！");
            }

            try
            {
                byte[] bytes = Convert.FromBase64String(FileBase64);//If not valid Base64 encoding, an exception is thrown
                stream.Seek(0, SeekOrigin.Begin);
                await stream.WriteAsync(bytes, 0, bytes.Length);
                stream.Seek(0, SeekOrigin.Begin);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Get an available file name
        /// </summary>
        /// <param name="backupName">Fallback name used when FileName is empty</param>
        /// <returns></returns>
        public string GetAvaliableFileName(string backupName)
        {
            return string.IsNullOrWhiteSpace(FileName) ? backupName : FileName;
        }

        /// <summary>
        /// Get the combined Value for use in fileDictionary
        /// </summary>
        public string GetFileValue()
        {
            if (string.IsNullOrWhiteSpace(FileBase64))
            {
                throw new Exceptions.FileValueException(this, "FileBase64 不能为 null！");
            }

            return FILE_DICTIONARY_STREAM_FORMAT.FormatWith(FileName, FileBase64);
        }

        ///// <summary>
        ///// Get the combined key-value object for use in fileDictionary
        ///// </summary>
        ///// <param name="formName">Form submission name, i.e. the Key in fileDictionary</param>
        ///// <returns></returns>
        //public KeyValuePair<string, string> GetFileDictionaryKV(string formName)
        //{
        //    return new KeyValuePair<string, string>(formName, GetFileValue());
        //}
    }
}
