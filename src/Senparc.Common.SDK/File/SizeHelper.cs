/*----------------------------------------------------------------
    Copyright (C) 2018 Senparc

    文件名：SizeHelper.cs
    文件功能描述：Senparc.Common.SDK 获取文件大小类


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
    ///  获取文件大小并以B，KB，GB，TB方式
    /// </summary>
    public class SizeHelper
    {
        /// <summary>
        /// 获取文件大小并以B，KB，GB，TB方式表示
        /// </summary>
        /// <param name="File">文件(FileInfo类型)</param>
        /// <returns></returns>
        public static string GetFileSize(FileInfo File)
        {
            string Result = "";
            long FileSize = File.Length;
            if (FileSize >= 1024 * 1024 * 1024)
            {
                if (FileSize / 1024 * 1024 * 1024 * 1024 >= 1024) Result = string.Format("{0:############0.00} TB", (double)FileSize / 1024 * 1024 * 1024 * 1024);
                else Result = string.Format("{0:####0.00} GB", (double)FileSize / 1024 * 1024 * 1024);
            }
            else if (FileSize >= 1024 * 1024) Result = string.Format("{0:####0.00} MB", (double)FileSize / 1024 * 1024);
            else if (FileSize >= 1024) Result = string.Format("{0:####0.00} KB", (double)FileSize / 1024);
            else Result = string.Format("{0:####0.00} Bytes", FileSize);
            return Result;
        }
        /// <summary>
        /// 获取文件大小并以B，KB，GB，TB方式表示
        /// </summary>
        /// <param name="FilePath">文件的具体路径</param>
        /// <returns></returns>
        public static string GetFileSize(string FilePath)
        {
            string Result = "";
            FileInfo File = new FileInfo(FilePath);
            long FileSize = File.Length;
            if (FileSize >= 1024 * 1024 * 1024)
            {
                if (FileSize / 1024 * 1024 * 1024 * 1024 >= 1024) Result = string.Format("{0:########0.00} TB", (double)FileSize / 1024 * 1024 * 1024 * 1024);
                else Result = string.Format("{0:####0.00} GB", (double)FileSize / 1024 * 1024 * 1024);
            }
            else if (FileSize >= 1024 * 1024) Result = string.Format("{0:####0.00} MB", (double)FileSize / 1024 * 1024);
            else if (FileSize >= 1024) Result = string.Format("{0:####0.00} KB", (double)FileSize / 1024);
            else Result = string.Format("{0:####0.00} Bytes", FileSize);
            return Result;
        }
        /// <summary>
        /// 计算文件大小函数(保留两位小数),Size为字节大小
        /// </summary>
        /// <param name="Size">初始文件大小</param>
        /// <returns></returns>
        public static string CountSize(long Size)
        {
            string m_strSize = "";
            long FactSize = 0;
            FactSize = Size;
            if (FactSize < 1024.00)
                m_strSize = FactSize.ToString("F2") + " 字节";
            else if (FactSize >= 1024.00 && FactSize < 1048576)
                m_strSize = (FactSize / 1024.00).ToString("F2") + " KB";
            else if (FactSize >= 1048576 && FactSize < 1073741824)
                m_strSize = (FactSize / 1024.00 / 1024.00).ToString("F2") + " MB";
            else if (FactSize >= 1073741824)
                m_strSize = (FactSize / 1024.00 / 1024.00 / 1024.00).ToString("F2") + " GB";
            return m_strSize;
        }
    }
}
