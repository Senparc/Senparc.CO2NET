/*----------------------------------------------------------------
    Copyright (C) 2018 Senparc

    文件名：MD5Helper.cs
    文件功能描述：Senparc.Common.SDK MD5加密帮助类


    创建标识：MartyZane - 20181030

----------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Text;

namespace Senparc.Common.SDK
{
    /// <summary>
    /// MD5加密帮助类
    /// </summary>
    public class Md5Helper
    {
        #region "MD5加密"

        /// <summary>
        /// MD5加密
        /// </summary>
        /// <param name="str">加密字符</param>
        /// <param name="code">加密位数16/32</param>
        /// <returns></returns>
        public static string MD5(string str, int code)
        {
            string strEncrypt = string.Empty;
            if (code == 16)
            {
                strEncrypt = System.Web.Security.FormsAuthentication.HashPasswordForStoringInConfigFile(str, "MD5").Substring(8, 16);
            }

            if (code == 32)
            {
                strEncrypt = System.Web.Security.FormsAuthentication.HashPasswordForStoringInConfigFile(str, "MD5");
            }

            return strEncrypt;
        }

        #endregion
    }
}
