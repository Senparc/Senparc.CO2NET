/*----------------------------------------------------------------
    Copyright (C) 2018 Senparc

    文件名：HashtableHelper.cs
    文件功能描述：Senparc.Common.SDK HashTable帮助类


    创建标识：MartyZane - 20181030

----------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Reflection;
using Newtonsoft.Json;
using System.IO;
using System.Data;
using System.Text.RegularExpressions;

namespace Senparc.Common.SDK
{
    /// <summary>
    /// Hashtable帮助类
    /// </summary>
    public class HashtableHelper
    {
        /// <summary>
        /// Json转Hashtable:
        /// </summary>
        /// <param name="strJson">Json格式</param>
        /// <returns></returns>
        public static Hashtable JsonToHashtable(string strJson)
        {
            Hashtable ht = new Hashtable();
            if (strJson == null)
            {
                return ht;
            }
            //取出表名   
            var rg = new Regex(@"(?<={)[^:]+(?=:\[)", RegexOptions.IgnoreCase);
            string strName = rg.Match(strJson).Value;
            DataTable tb = new DataTable();
            //获取数据   
            rg = new Regex(@"(?<={)[^}]+(?=})");
            MatchCollection mc = rg.Matches(strJson);
            for (int i = 0; i < mc.Count; i++)
            {
                string strRow = mc[i].Value;
                string[] strRows = strRow.Split(',');
                //创建表   
                if (tb.Columns.Count == 0)
                {
                    tb = new DataTable();
                    tb.TableName = strName;
                    foreach (string str in strRows)
                    {
                        var dc = new DataColumn();
                        string[] strCell = str.Split(':');
                        dc.ColumnName = strCell[0].Replace(@"""", "");
                        tb.Columns.Add(dc);
                    }
                    tb.AcceptChanges();
                }
                //增加内容   
                DataRow dr = tb.NewRow();
                for (int r = 0; r < strRows.Length; r++)
                {
                    dr[r] = strRows[r].Split(':')[1].Trim().Replace("，", ",").Replace("：", ":").Replace("\"", "");
                }
                tb.Rows.Add(dr);
                tb.AcceptChanges();
            }
            return DataHelper.DataTableToHashtable(tb);
        }
        /// <summary>
        /// 实体类Model转Hashtable(反射)
        /// </summary>
        public static Hashtable GetModelToHashtable<T>(T model)
        {
            Hashtable ht = new Hashtable();
            PropertyInfo[] properties = model.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);
            foreach (PropertyInfo item in properties)
            {
                string key = item.Name;
                ht[key] = item.GetValue(model, null);
            }
            return ht;
        }
        /// <summary>
        /// Hashtable转换实体类
        /// </summary>
        public static T HashtableToModel<T>(Hashtable ht)
        {
            T model = Activator.CreateInstance<T>();
            Type type = model.GetType();
            //遍历每一个属性
            foreach (PropertyInfo prop in type.GetProperties())
            {
                object value = ht[prop.Name];
                if (prop.PropertyType.ToString() == "System.Nullable`1[System.DateTime]")
                {
                    value = CommonHelper.ToDateTime(value);
                }
                prop.SetValue(model, HackType(value, prop.PropertyType), null);
            }
            return model;
        }
        //这个类对可空类型进行判断转换，要不然会报错
        public static object HackType(object value, Type conversionType)
        {
            if (conversionType.IsGenericType && conversionType.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
            {
                if (value == null)
                    return null;
                System.ComponentModel.NullableConverter nullableConverter = new System.ComponentModel.NullableConverter(conversionType);
                conversionType = nullableConverter.UnderlyingType;
            }
            return Convert.ChangeType(value, conversionType);
        }

        public static bool IsNullOrDBNull(object obj)
        {
            return ((obj is DBNull) || string.IsNullOrEmpty(obj.ToString())) ? true : false;
        }
        /// <summary>
        /// 字符串 分割转换 Hashtable   ≌; ☻
        /// </summary>
        public static Hashtable ParameterToHashtable(string str)
        {
            Hashtable ht = new Hashtable();
            if (!string.IsNullOrEmpty(str))
            {
                string[] arrayParm_Key_Value = str.Split('≌');
                foreach (string item in arrayParm_Key_Value)
                {
                    if (item.Length > 0)
                    {
                        string[] Key_Value = item.Split('☻');
                        ht[Key_Value[0]] = Key_Value[1];
                    }
                }
            }
            return ht;
        }
        /// <summary>
        /// 自定义格式字符串转换 Hashtable
        /// </summary>
        /// <param name="item">自定义字符串</param>
        /// <returns></returns>
        public static Hashtable List_Key_ValueToHashtable(string item)
        {
            Hashtable ht = new Hashtable();
            foreach (string itemwithin in item.Split('☺'))
            {
                if (itemwithin.Length > 0)
                {
                    string[] str_item = itemwithin.Split('☻');
                    ht[str_item[0]] = str_item[1];
                }
            }
            return ht;
        }
        /// <summary>
        /// 指示指定的字符串是 null 还是 System.String.Empty 字符串。
        /// </summary>
        /// <param name="item">要测试的值</param>
        /// <returns>如果 value 参数为 null 或空字符串 ("")，则为 true；否则为 false</returns>
        public static bool IsNullOrEmpty(object item)
        {
            if (item != null && item.ToString() != "null" && !string.IsNullOrEmpty(item.ToString()))
            {

                return false;
            }
            else
            {
                return true;
            }
        }
    }
}