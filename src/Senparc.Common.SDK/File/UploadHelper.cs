/*----------------------------------------------------------------
    Copyright (C) 2018 Senparc

    文件名：UploadHelper.cs
    文件功能描述：Senparc.Common.SDK 文件上传帮助类


    创建标识：MartyZane - 20181030

----------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Text;
using System.Web.UI.WebControls;
using System.IO;
using System.Web;

namespace Senparc.Common.SDK
{
    /// <summary>
    /// 文件上传帮助类
    /// </summary>
    public class UploadHelper
    {
        /// <summary>
        /// 附件上传 成功：succeed、失败：error、文件太大：-1、
        /// </summary>
        /// <param name="file">单独文件的访问</param>
        /// <param name="path">存储路径</param>
        /// <param name="filename">输出文件名</param>
        /// <returns></returns>
        public static string FileUpload(HttpPostedFileBase file, string path, string FileName)
        {
            if (Directory.Exists(path) == false)//如果不存在就创建file文件夹
            {
                Directory.CreateDirectory(path);
            }
            //取得文件的扩展名,并转换成小写
            string Extension = System.IO.Path.GetExtension(file.FileName).ToLower();
            //取得文件大小
            string filesize = SizeHelper.CountSize(file.ContentLength);
            try
            {
                int Size = file.ContentLength / 1024 / 1024;
                if (Size > 10)
                {
                    return "-1";
                }
                else
                {
                    file.SaveAs(path + FileName);
                    return "succeed";
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
    }
}
