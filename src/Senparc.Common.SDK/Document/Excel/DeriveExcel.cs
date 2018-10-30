/*----------------------------------------------------------------
    Copyright (C) 2018 Senparc

    文件名：DeriveExcel.cs
    文件功能描述：Senparc.Common.SDK 导出Excel帮助类


    创建标识：MartyZane - 20181030

----------------------------------------------------------------*/

using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Web;

namespace Senparc.Common.SDK
{
    /// <summary>
    /// 导出Excel帮助类
    /// </summary>
    public class DeriveExcel
    {
        /// <summary>
        /// IList导出Excel
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list">集合</param>
        /// <param name="DataColumn">字段</param>
        /// <param name="fileName"></param>
        public static void ListToExcel<T>(IList list, string[] DataColumn, string fileName)
        {
            HttpContext.Current.Response.ContentType = "application/vnd.ms-excel";
            HttpContext.Current.Response.ContentEncoding = System.Text.Encoding.UTF8;
            HttpContext.Current.Response.Charset = "Utf-8";
            HttpContext.Current.Response.AppendHeader("Content-Disposition", "attachment;filename=" + HttpUtility.UrlEncode(fileName + ".xls", System.Text.Encoding.UTF8));
            StringBuilder sbHtml = new StringBuilder();
            sbHtml.AppendLine("<meta http-equiv=\"Content-Type\" content=\"text/html; charset=utf-8\">");
            sbHtml.AppendLine("<table cellspacing=\"0\" cellpadding=\"5\" rules=\"all\" border=\"1\">");
            //写出列名
            sbHtml.AppendLine("<tr style=\"background-color: #FFE88C;font-weight: bold; white-space: nowrap;\">");
            foreach (string item in DataColumn)
            {
                string[] stritem = item.Split(':');
                sbHtml.AppendLine("<td>" + stritem[0] + "</td>");
            }
            sbHtml.AppendLine("</tr>");
            //写数据
            foreach (T entity in list)
            {
                Hashtable ht = HashtableHelper.GetModelToHashtable<T>(entity);
                sbHtml.Append("<tr>");
                foreach (string item in DataColumn)
                {
                    string[] stritem = item.Split(':');
                    sbHtml.Append("<td>").Append(ht[stritem[1]]).Append("</td>");
                }
                sbHtml.AppendLine("</tr>");
            }
            sbHtml.AppendLine("</table>");
            HttpContext.Current.Response.Write(sbHtml.ToString());
            HttpContext.Current.Response.End();
        }

        /// <summary>
        /// DataTable导出Excel
        /// </summary>
        /// <param name="data">集合</param>
        /// <param name="DataColumn">字段</param>
        /// <param name="fileName">文件名称</param>
        public static void DataTableToExcel(DataTable data, string[] DataColumn, string fileName)
        {
            HttpContext.Current.Response.ContentType = "application/vnd.ms-excel";
            HttpContext.Current.Response.ContentEncoding = System.Text.Encoding.UTF8;
            HttpContext.Current.Response.Charset = "Utf-8";
            HttpContext.Current.Response.AppendHeader("Content-Disposition", "attachment;filename=" + HttpUtility.UrlEncode(fileName + ".xls", System.Text.Encoding.UTF8));
            StringBuilder sbHtml = new StringBuilder();
            sbHtml.AppendLine("<meta http-equiv=\"Content-Type\" content=\"text/html; charset=utf-8\">");
            sbHtml.AppendLine("<table cellspacing=\"0\" cellpadding=\"5\" rules=\"all\" border=\"1\">");
            //写出列名
            sbHtml.AppendLine("<tr style=\"background-color: #FFE88C;font-weight: bold; white-space: nowrap;\">");
            foreach (string item in DataColumn)
            {
                sbHtml.AppendLine("<td>" + item + "</td>");
            }
            sbHtml.AppendLine("</tr>");
            //写数据
            foreach (DataRow row in data.Rows)
            {
                sbHtml.Append("<tr>");
                foreach (string item in DataColumn)
                {
                    sbHtml.Append("<td>").Append(row[item]).Append("</td>");
                }
                sbHtml.AppendLine("</tr>");
            }
            sbHtml.AppendLine("</table>");
            HttpContext.Current.Response.Write(sbHtml.ToString());
            HttpContext.Current.Response.End();
        }

        /// <summary>
        /// Table标签导出Excel
        /// </summary>
        /// <param name="sbHtml">html标签</param>
        /// <param name="fileName">文件名</param>
        public static void HtmlToExcel(StringBuilder sbHtml, string fileName)
        {
            if (sbHtml.Length > 0)
            {
                HttpContext.Current.Response.ContentType = "application/vnd.ms-excel";
                HttpContext.Current.Response.ContentEncoding = System.Text.Encoding.UTF8;
                HttpContext.Current.Response.Charset = "Utf-8";
                HttpContext.Current.Response.AppendHeader("Content-Disposition", "attachment;filename=" + HttpUtility.UrlEncode(fileName + ".xls", System.Text.Encoding.UTF8));
                HttpContext.Current.Response.Write(sbHtml.ToString());
                HttpContext.Current.Response.End();
            }
        }

        /// <summary>
        /// JqGrid导出Excel
        /// </summary>
        /// <param name="JsonColumn">表头</param>
        /// <param name="JsonData">数据</param>
        /// <param name="ExportField">导出字段</param>
        /// <param name="fileName">文件名</param>
        public static void JqGridToExcel(string JsonColumn, string JsonData, string ExportField, string fileName)
        {
            List<JqGridColumn> ListColumn = JsonConvert.DeserializeObject<List<JqGridColumn>>(JsonColumn);
            HttpContext.Current.Response.ContentType = "application/vnd.ms-excel";
            HttpContext.Current.Response.ContentEncoding = System.Text.Encoding.UTF8;
            HttpContext.Current.Response.Charset = "Utf-8";
            HttpContext.Current.Response.AppendHeader("Content-Disposition", "attachment;filename=" + HttpUtility.UrlEncode(fileName + ".xls", System.Text.Encoding.UTF8));
            StringBuilder sbHtml = new StringBuilder();
            sbHtml.AppendLine("<meta http-equiv=\"Content-Type\" content=\"text/html; charset=utf-8\">");
            sbHtml.AppendLine("<table cellspacing=\"0\" cellpadding=\"5\" rules=\"all\" border=\"1\">");
            //写出列名
            sbHtml.AppendLine("<tr style=\"background-color: #FFE88C;font-weight: bold; white-space: nowrap;\">");
            string[] FieldInfo = ExportField.Split(',');
            foreach (string item in FieldInfo)
            {
                IList list = ListColumn.FindAll(t => t.name == item);
                foreach (JqGridColumn jqgridcolumn in list)
                {
                    if (jqgridcolumn.hidden.ToLower() == "false" && jqgridcolumn.label != null)
                    {
                        sbHtml.AppendLine("<td style=\"width:" + jqgridcolumn.width + "px;text-align:" + jqgridcolumn.align + "\">" + jqgridcolumn.label + "</td>");
                    }
                }
            }
            sbHtml.AppendLine("</tr>");
            //写数据
            DataTable dt = JsonData.JsonToDataTable();
            foreach (DataRow row in dt.Rows)
            {
                sbHtml.Append("<tr>");
                foreach (string item in FieldInfo)
                {
                    IList list = ListColumn.FindAll(t => t.name == item);
                    foreach (JqGridColumn jqgridcolumn in list)
                    {
                        if (jqgridcolumn.hidden.ToLower() == "false" && jqgridcolumn.label != null)
                        {
                            object text = row[jqgridcolumn.name];
                            sbHtml.Append("<td style=\"width:" + jqgridcolumn.width + "px;text-align:" + jqgridcolumn.align + "\">").Append(text).Append("</td>");
                        }
                    }
                }
                sbHtml.AppendLine("</tr>");
            }
            sbHtml.AppendLine("</table>");
            HttpContext.Current.Response.Write(sbHtml.ToString());
            HttpContext.Current.Response.End();
        }
    }
}
