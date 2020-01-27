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

    文件名：FormFileData.cs
    文件功能描述：模拟 Form 提交时， RequestUtility.Post 中 fileDictionary 的 Value 值在提供 base64 文件流信息状态下的解析信息


    创建标识：Senparc - 20190811
    
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
    /// Form 提交
    /// </summary>
    public class FormFileData
    {
        /// <summary>
        /// Post 方法中 fileDictionary 参数的 Value 可以提供 base64 编码后的数据流，需要符合此格式
        /// </summary>
        public const string FILE_DICTIONARY_STREAM_FORMAT = "{0}||{1}";

        /// <summary>
        /// Form 提交用于标记的文件名
        /// </summary>
        public string FileName { get; set; }
        /// <summary>
        /// 文件 base64 编码
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
        /// 从文件流获取适用于 RequestUtility.Post 中 FileDictionary 的 Base64值
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

            //方法一
            byte[] fileBytes = new byte[fileStream.Length];
            fileStream.Read(fileBytes, 0, fileBytes.Length);

            //方法二
            //BinaryReader r = new BinaryReader(fileStream);
            //r.BaseStream.Seek(0, SeekOrigin.Begin);
            //var fileBytes = r.ReadBytes((int)r.BaseStream.Length);//TODO: 不使用 int 限制 long 的长度

            FileBase64 = Convert.ToBase64String(fileBytes);
        }

        /// <summary>
        /// 从 FileValue 获取 FileName、Base64 等参数
        /// </summary>
        /// <param name="fileValue">FileDictionary 的 Value</param>
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

            //TODO:可以加入校验
        }

        /// <summary>
        /// 尝试将 Base64 加载到 stream 中，会进行 Base64 检测，如果失败则返回 false
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
                byte[] bytes = Convert.FromBase64String(FileBase64);//如果不是有效的 Base64 编码，则会进入异常
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
        /// 获取可用的文件名
        /// </summary>
        /// <param name="backupName">备用名称（当前FileName为空时使用）</param>
        /// <returns></returns>
        public string GetAvaliableFileName(string backupName)
        {
            return string.IsNullOrWhiteSpace(FileName) ? backupName : FileName;
        }

        /// <summary>
        /// 获取整合之后的给 fileDictionary 使用的 Value
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
        ///// 获取整合之后的给 fileDictionary 使用的 K-V 对象
        ///// </summary>
        ///// <param name="formName">表单提交时的 name，即 fileDictionary 中的 Key 值</param>
        ///// <returns></returns>
        //public KeyValuePair<string, string> GetFileDictionaryKV(string formName)
        //{
        //    return new KeyValuePair<string, string>(formName, GetFileValue());
        //}
    }
}
