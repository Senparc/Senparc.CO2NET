/*----------------------------------------------------------------
    Copyright (C) 2018 Senparc

    文件名：SqlFilterHelper.cs
    文件功能描述：Senparc.Common.SDK 防注入过滤函数帮助类


    创建标识：MartyZane - 20181030

----------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Senparc.Common.SDK
{
    /// <summary>
    /// 防注入过滤函数
    /// </summary>
    public class SqlFilterHelper
    {
        ///<summary>
        /// 防注入过滤函数
        ///</summary>
        ///<param name="inputString">需要过滤字符串</param>
        ///<returns>过滤后的字符串</returns>
        public static string Filter(string inputString)
        {
            if (inputString != "")
            {
                string sql = SqlFilters(inputString);
                if (sql == "")
                {
                    sql = "敏感字符";
                }
                return sql;
            }
            else
            {
                return inputString;
            }
        }
        ///<summary>
        /// 过滤字符串中注入SQL脚本的方法
        ///</summary>
        ///<param name="source">传入的字符串</param>
        ///<returns>过滤后的字符串</returns>
        private static string SqlFilters(string source)
        {
            //半角括号替换为全角括号
            source = source.Replace("'", "'''").Replace(";", "；").Replace("(", "（").Replace(")", "）");
            //去除执行SQL语句的命令关键字
            source = Regex.Replace(source, "select", "", RegexOptions.IgnoreCase);
            source = Regex.Replace(source, "insert", "", RegexOptions.IgnoreCase);
            source = Regex.Replace(source, "update", "", RegexOptions.IgnoreCase);
            source = Regex.Replace(source, "delete", "", RegexOptions.IgnoreCase);
            source = Regex.Replace(source, "drop", "", RegexOptions.IgnoreCase);
            source = Regex.Replace(source, "truncate", "", RegexOptions.IgnoreCase);
            source = Regex.Replace(source, "declare", "", RegexOptions.IgnoreCase);
            source = Regex.Replace(source, "xp_cmdshell", "", RegexOptions.IgnoreCase);
            source = Regex.Replace(source, "/add", "", RegexOptions.IgnoreCase);
            source = Regex.Replace(source, "net user", "", RegexOptions.IgnoreCase);
            //去除执行存储过程的命令关键字 
            source = Regex.Replace(source, "exec", "", RegexOptions.IgnoreCase);
            source = Regex.Replace(source, "execute", "", RegexOptions.IgnoreCase);
            //去除系统存储过程或扩展存储过程关键字
            source = Regex.Replace(source, "xp_", "x p_", RegexOptions.IgnoreCase);
            source = Regex.Replace(source, "sp_", "s p_", RegexOptions.IgnoreCase);
            //防止16进制注入
            source = Regex.Replace(source, "0x", "0 x", RegexOptions.IgnoreCase);
            return source;
        }
    }
}
