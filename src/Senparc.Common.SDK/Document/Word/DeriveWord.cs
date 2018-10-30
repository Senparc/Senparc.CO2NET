/*----------------------------------------------------------------
    Copyright (C) 2018 Senparc

    文件名：DeriveWord.cs
    文件功能描述：Senparc.Common.SDK 导出Word文件


    创建标识：MartyZane - 20181030

----------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;

namespace Senparc.Common.SDK
{
    /// <summary>
    /// 导出Word文件
    /// </summary>
    public class DeriveWord
    {
        /// <summary>
        /// 把DataTable导出为Word文件
        /// </summary>
        /// <param name="page">Page</param>
        /// <param name="fileName">Word文件名(不包括后缀*.doc)</param>
        /// <param name="dtbl">将要被导出的DataTable对象</param>
        /// <returns></returns>
        public static bool DataTableToWord(System.Web.HttpResponse response, string fileName, DataTable dtbl)
        {
            response.Clear();
            response.Buffer = true;
            response.Charset = "UTF-8";
            response.AppendHeader("Content-Disposition", "attachment;filename=" + fileName + ".doc");
            response.ContentEncoding = System.Text.Encoding.GetEncoding("UTF-8");
            response.ContentType = "application/ms-word";
            //page.EnableViewState = false;
            response.Write(DataTableToHtmlTable(dtbl));
            response.End();
            return true;
        }

        /// <summary>
        /// 把DataTable转换成Html的Table
        /// </summary>
        /// <param name="dataTable">DataTable对象</param>
        /// <returns></returns>
        public static string DataTableToHtmlTable(DataTable dataTable)
        {
            StringBuilder sBuilder = new StringBuilder();

            sBuilder.Append("<table cellspacing=\"0\" rules=\"all\" border=\"1\" style=\"border-collapse:collapse;\">");
            foreach (DataRow dr in dataTable.Rows)
            {
                sBuilder.Append("<tr>");
                foreach (DataColumn dc in dataTable.Columns)
                {
                    if (dc.ColumnName.Equals(""))
                    {
                        sBuilder.Append(string.Format("<td>{0}</td>", dr[dc].ToString()));
                    }
                    else
                    {
                        sBuilder.Append(string.Format("<td>{0}</td>", dr[dc].ToString()));// style='vnd.ms-excel.numberformat:@'
                    }
                }
                sBuilder.Append("</tr>");
            }
            sBuilder.Append("</table");
            return sBuilder.ToString();
        }
    }
}
