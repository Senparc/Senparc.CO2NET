/*----------------------------------------------------------------
    Copyright (C) 2018 Senparc

    文件名：ManageProvider.cs
    文件功能描述：Senparc.Common.SDK 管理提供者类


    创建标识：MartyZane - 20181030

----------------------------------------------------------------*/

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Web;

namespace Senparc.Common.SDK
{
    /// <summary>
    /// 管理提供者
    /// </summary>
    public class ManageProvider : IManageProvider
    {
        #region 静态实例

        /// <summary>当前提供者</summary>
        public static IManageProvider Provider
        {
            get { return new ManageProvider(); }
        }

        #endregion

        /// <summary>
        /// 秘钥
        /// </summary>
        private string LoginUserKey = "LoginUserKey";
        /// <summary>
        /// 登陆提供者模式:Session、Cookie 
        /// </summary>
        private string LoginProvider = ConfigHelper.AppSettings("LoginProvider");

        /// <summary>
        /// 写入登录信息
        /// </summary>
        /// <param name="user">成员信息</param>
        public virtual void AddCurrent(IManageUser user)
        {
            try
            {
                if (LoginProvider == "Cookie")
                {
                    CookieHelper.WriteCookie(LoginUserKey, DESEncrypt.Encrypt(JsonConvert.SerializeObject(user)), 1440);
                }
                else
                {
                    SessionHelper.Add(LoginUserKey, DESEncrypt.Encrypt(JsonConvert.SerializeObject(user)));
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// 当前用户
        /// </summary>
        /// <returns></returns>
        public virtual IManageUser Current()
        {
            try
            {
                IManageUser user = new IManageUser();
                if (LoginProvider == "Cookie")
                {
                    user = JsonConvert.DeserializeObject<IManageUser>(DESEncrypt.Decrypt(CookieHelper.GetCookie(LoginUserKey)));
                }
                else
                {
                    user = JsonConvert.DeserializeObject<IManageUser>(DESEncrypt.Decrypt(SessionHelper.Get(LoginUserKey).ToString()));
                }
                if (user == null)
                {
                    throw new Exception("登录信息超时，请重新登录。");
                }
                return user;
            }
            catch
            {
                throw new Exception("登录信息超时，请重新登录。");
            }
        }

        /// <summary>
        /// 删除登录信息
        /// </summary>
        public virtual void EmptyCurrent()
        {
            if (LoginProvider == "Cookie")
            {
                HttpCookie objCookie = new HttpCookie(LoginUserKey.Trim());
                objCookie.Expires = DateTime.Now.AddYears(-5);
                HttpContext.Current.Response.Cookies.Add(objCookie);
            }
            else
            {
                SessionHelper.Remove(LoginUserKey.Trim());
            }
        }

        /// <summary>
        /// 是否过期
        /// </summary>
        /// <returns></returns>
        public virtual bool IsOverdue()
        {
            object str = "";
            if (LoginProvider == "Cookie")
            {
                str = CookieHelper.GetCookie(LoginUserKey);
            }
            else
            {
                str = SessionHelper.Get(LoginUserKey);
            }
            if (str != null && str.ToString() != "")
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
