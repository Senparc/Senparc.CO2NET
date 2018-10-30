/*----------------------------------------------------------------
    Copyright (C) 2018 Senparc

    文件名：TreeJson.cs
    文件功能描述：Senparc.Common.SDK 构造树形Json


    创建标识：MartyZane - 20181030

----------------------------------------------------------------*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Senparc.Common.SDK
{
    /// <summary>
    /// 构造树形Json
    /// </summary>
    public static class TreeJson
    {

        #region TreeToJson
        /// <summary>
        /// 转换树形Json
        /// </summary>
        /// <returns></returns>
        public static string TreeToJson(this IList list, string ParentId = "0")
        {
            StringBuilder strJson = new StringBuilder();
            List<TreeJsonEntity> item = DataHelper.IListToList<TreeJsonEntity>(list).FindAll(t => t.parentId == ParentId);
            strJson.Append("[");
            if (item.Count > 0)
            {
                foreach (TreeJsonEntity entity in item)
                {
                    strJson.Append("{");
                    strJson.Append("\"id\":\"" + entity.id + "\",");
                    strJson.Append("\"text\":\"" + entity.text.Replace("&nbsp;", "") + "\",");
                    strJson.Append("\"value\":\"" + entity.value + "\",");
                    if (entity.Attribute != null && !string.IsNullOrEmpty(entity.Attribute))
                    {
                        strJson.Append("\"" + entity.Attribute + "\":\"" + entity.AttributeValue + "\",");
                    }
                    if (entity.AttributeA != null && !string.IsNullOrEmpty(entity.AttributeA))
                    {
                        strJson.Append("\"" + entity.AttributeA + "\":\"" + entity.AttributeValueA + "\",");
                    }
                    if (entity.title != null && !string.IsNullOrEmpty(entity.title.Replace("&nbsp;", "")))
                    {
                        strJson.Append("\"title\":\"" + entity.title.Replace("&nbsp;", "") + "\",");
                    }
                    if (entity.img != null && !string.IsNullOrEmpty(entity.img.Replace("&nbsp;", "")))
                    {
                        strJson.Append("\"img\":\"" + entity.img.Replace("&nbsp;", "") + "\",");
                    }
                    if (entity.checkstate != null)
                    {
                        strJson.Append("\"checkstate\":" + entity.checkstate + ",");
                    }
                    if (entity.parentId != null)
                    {
                        strJson.Append("\"parentnodes\":\"" + entity.parentId + "\",");
                    }
                    if (entity.level != null)
                    {
                        strJson.Append("\"level\":\"" + entity.level + "\",");
                    }
                    strJson.Append("\"showcheck\":" + entity.showcheck.ToString().ToLower() + ",");
                    strJson.Append("\"shownode\":" + entity.shownode.ToString().ToLower() + ",");
                    strJson.Append("\"isexpand\":" + entity.isexpand.ToString().ToLower() + ",");
                    if (entity.complete == true)
                    {
                        strJson.Append("\"complete\":" + entity.complete.ToString().ToLower() + ",");
                    }
                    strJson.Append("\"hasChildren\":" + entity.hasChildren.ToString().ToLower() + ",");
                    strJson.Append("\"ChildNodes\":" + TreeToJson(list, entity.id) + "");
                    strJson.Append("},");
                }
                strJson = strJson.Remove(strJson.Length - 1, 1);
            }
            strJson.Append("]");
            return strJson.ToString();
        }
        #endregion

        #region ZTreeToJson
        /// <summary>
        /// 转换树形Json
        /// </summary>
        /// <returns></returns>
        public static string ZTreeToJson(this IList list, string pId = "0")
        {
            StringBuilder strJson = new StringBuilder();
            List<ZTreeJsonEntity> item = DataHelper.IListToList<ZTreeJsonEntity>(list).FindAll(t => t.pId == pId);
            strJson.Append("[");
            if (item.Count > 0)
            {
                foreach (ZTreeJsonEntity entity in item)
                {
                    strJson.Append("{");
                    strJson.Append("\"id\":\"" + entity.id + "\",");
                    strJson.Append("\"pId\":\"" + entity.pId + "\",");
                    strJson.Append("\"name\":\"" + entity.name + "\",");
                    strJson.Append("\"open\":" + entity.open.ToString().ToLower() + ",");
                    strJson.Append("},");
                }
                strJson = strJson.Remove(strJson.Length - 1, 1);
            }
            strJson.Append("]");
            return strJson.ToString();
        }
        #endregion

    }
    public class TreeJsonEntity
    {
        public string parentId { get; set; }
        public string id { get; set; }
        public string text { get; set; }
        public string value { get; set; }
        public int? checkstate { get; set; }
        public bool showcheck { get; set; }
        /// <summary>
        /// 是否显示节点
        /// </summary>
        public bool shownode { get; set; }
        /// <summary>
        /// 是否展开
        /// </summary>
        public bool isexpand { get; set; }
        public bool complete { get; set; }
        /// <summary>
        /// 是否有子节点
        /// </summary>
        public bool hasChildren { get; set; }
        public string img { get; set; }
        public string title { get; set; }
        /// <summary>
        /// 自定义属性
        /// </summary>
        public string Attribute { get; set; }
        /// <summary>
        /// 自定义属性值
        /// </summary>
        public string AttributeValue { get; set; }
        /// <summary>
        /// 自定义属性A
        /// </summary>
        public string AttributeA { get; set; }
        /// <summary>
        /// 自定义属性值A
        /// </summary>
        public string AttributeValueA { get; set; }
        /// <summary>
        /// 自定义属性值
        /// </summary>
        public int? level { get; set; }
    }

    public class ZTreeJsonEntity
    {
        public string id { get; set; }
        public string pId { get; set; }
        public string name { get; set; }
        public bool open { get; set; }
    }
}
