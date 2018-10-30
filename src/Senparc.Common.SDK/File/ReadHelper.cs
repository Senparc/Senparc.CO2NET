/*----------------------------------------------------------------
    Copyright (C) 2018 Senparc

    文件名：ReadHelper.cs
    文件功能描述：Senparc.Common.SDK 以只读方式读取文本文件


    创建标识：MartyZane - 20181030

----------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Senparc.Common.SDK
{
    /// <summary>
    /// 以只读方式读取文本文件
    /// </summary>
    public class ReadHelper
    {
        #region 以只读方式读取文本文件
        /// <summary>
        /// 以只读方式读取文本文件
        /// </summary>
        /// <param name="FilePath">文件路径及文件名</param>
        /// <returns></returns>
        public static string ReadTxtFile(string FilePath)
        {
            string content = "";//返回的字符串
            using (FileStream fs = new FileStream(FilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                using (StreamReader reader = new StreamReader(fs, Encoding.UTF8))
                {
                    string text = string.Empty;
                    while (!reader.EndOfStream)
                    {
                        text += reader.ReadLine() + "\r\n";
                        content = text;
                    }
                }
            }
            return content;
        }
        public static string ReadFile(string FilePath)
        {
            string text = string.Empty;
            System.Text.Encoding code = System.Text.Encoding.GetEncoding("gb2312");
            using (var sr = new StreamReader(FilePath, code))
            {
                try
                {
                    text = sr.ReadToEnd(); // 读取文件
                    sr.Close();
                }
                catch { }
            }
            return text;
        }
        #endregion
    }
}
