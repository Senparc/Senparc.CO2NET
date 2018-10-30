/*----------------------------------------------------------------
    Copyright (C) 2018 Senparc

    文件名：INIFileHelper.cs
    文件功能描述：Senparc.Common.SDK INI文件读写类


    创建标识：MartyZane - 20181030

----------------------------------------------------------------*/

using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Web;
using System.Web.SessionState;

namespace Senparc.Common.SDK
{
    /// <summary>
    /// INI文件读写类
    /// </summary>
    public class INIFileHelper
    {
        public static string path;

        [DllImport("kernel32")]
        private static extern long WritePrivateProfileString(string section, string key, string val, string filePath);

        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);


        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section, string key, string defVal, Byte[] retVal, int size, string filePath);


        /// <summary> 
        /// 写INI文件 
        /// </summary> 
        /// <param name="Section"></param> 
        /// <param name="Key"></param> 
        /// <param name="Value"></param> 
        public static void IniWriteValue(string Section, string Key, string Value)
        {
            WritePrivateProfileString(Section, Key, Value, path);
        }

        /// <summary> 
        /// 读取INI文件 
        /// </summary> 
        /// <param name="Section"></param> 
        /// <param name="Key"></param> 
        /// <returns></returns> 
        public static string IniReadValue(string Section, string Key)
        {
            //Cache.Insert("Items1", list, new System.Web.Caching.CacheDependency(path)); 
            HttpSessionState Session = HttpContext.Current.Session;
            if (path == null) path = Session["path"].ToString();
            StringBuilder temp = new StringBuilder(255);
            int i = GetPrivateProfileString(Section, Key, "", temp, 255, path);
            return temp.ToString();
        }
        public static byte[] IniReadValues(string section, string key)
        {
            byte[] temp = new byte[255];
            int i = GetPrivateProfileString(section, key, "", temp, 255, path);
            return temp;

        }


        /// <summary> 
        /// 删除ini文件下所有段落 
        /// </summary> 
        public static void ClearAllSection()
        {
            IniWriteValue(null, null, null);
        }
        /// <summary> 
        /// 删除ini文件下personal段落下的所有键 
        /// </summary> 
        /// <param name="Section"></param> 
        public static void ClearSection(string Section)
        {
            IniWriteValue(Section, null, null);
        }
    }
}
