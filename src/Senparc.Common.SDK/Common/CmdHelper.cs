/*----------------------------------------------------------------
    Copyright (C) 2018 Senparc

    文件名：CmdHelper.cs
    文件功能描述：Senparc.Common.SDK 命令操作帮助类


    创建标识：MartyZane - 20181030

----------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace Senparc.Common.SDK
{
    /// <summary>
    /// 执行命令
    /// </summary>
    public class CmdHelper
    {
        /// <summary>
        /// 执行cmd.exe命令
        /// </summary>
        /// <param name="commandText">命令文本</param>
        /// <returns></returns>
        public static string ExeCommand(string commandText)
        {
            return ExeCommand(new string[] { commandText });
        }

        /// <summary>
        /// 执行多条cmd.exe命令
        /// </summary>
        /// <param name="commandTexts">命令文本数组</param>
        /// <returns></returns>
        public static string ExeCommand(string[] commandTexts)
        {
            Process p = new Process();
            p.StartInfo.FileName = "cmd.exe";
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardInput = true;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.RedirectStandardError = true;
            p.StartInfo.CreateNoWindow = true;
            string strOutput = null;
            try
            {
                p.Start();
                foreach (string item in commandTexts)
                {
                    p.StandardInput.WriteLine(item);
                }
                p.StandardInput.WriteLine("exit");
                strOutput = p.StandardOutput.ReadToEnd();
                //strOutput = Encoding.UTF8.GetString(Encoding.Default.GetBytes(strOutput));
                p.WaitForExit();
                p.Close();
            }
            catch (Exception e)
            {
                strOutput = e.Message;
            }
            return strOutput;
        }

        /// <summary>
        /// 启动外部Windows应用程序，隐藏程序界面
        /// </summary>
        /// <param name="appName">应用程序路径名称</param>
        /// <returns>true表示成功，false表示失败</returns>
        public static bool StartApp(string appName)
        {
            return StartApp(appName, ProcessWindowStyle.Hidden);
        }

        /// <summary>
        /// 启动外部应用程序
        /// </summary>
        /// <param name="appName">应用程序路径名称</param>
        /// <param name="style">进程窗口模式</param>
        /// <returns>true表示成功，false表示失败</returns>
        public static bool StartApp(string appName, ProcessWindowStyle style)
        {
            return StartApp(appName, null, style);
        }

        /// <summary>
        /// 启动外部应用程序，隐藏程序界面
        /// </summary>
        /// <param name="appName">应用程序路径名称</param>
        /// <param name="arguments">启动参数</param>
        /// <returns>true表示成功，false表示失败</returns>
        public static bool StartApp(string appName, string arguments)
        {
            return StartApp(appName, arguments, ProcessWindowStyle.Hidden);
        }

        /// <summary>
        /// 启动外部应用程序
        /// </summary>
        /// <param name="appName">应用程序路径名称</param>
        /// <param name="arguments">启动参数</param>
        /// <param name="style">进程窗口模式</param>
        /// <returns>true表示成功，false表示失败</returns>
        public static bool StartApp(string appName, string arguments, ProcessWindowStyle style)
        {
            bool blnRst = false;
            Process p = new Process();
            p.StartInfo.FileName = appName;//exe,bat and so on
            p.StartInfo.WindowStyle = style;
            p.StartInfo.Arguments = arguments;
            try
            {
                p.Start();
                p.WaitForExit();
                p.Close();
                blnRst = true;
            }
            catch
            {
            }
            return blnRst;
        }

        /// <summary>
        /// 实现压缩，需要rar.exe上传到网站根目录
        /// </summary>
        /// <param name="s"></param>
        /// <param name="d"></param>
        /// <example>rar("e:/www.svnhost.cn/", "e:/www.svnhost.cn.rar");</example>
        public static void Rar(string s, string d)
        {
            ExeCommand(System.Web.HttpContext.Current.Server.MapPath("~/rar.exe") + " a \"" + d + "\" \"" + s + "\" -ep1");
        }

        /// <summary>
        /// 实现解压缩，需要rar.exe上传到网站根目录
        /// </summary>
        /// <param name="s"></param>
        /// <param name="d"></param>
        /// <example>unrar("e:/www.svnhost.cn.rar", "e:/");</example>
        public static void UnRar(string s, string d)
        {
            ExeCommand(System.Web.HttpContext.Current.Server.MapPath("~/rar.exe") + " x \"" + s + "\" \"" + d + "\" -o+");
        }

    }

}
