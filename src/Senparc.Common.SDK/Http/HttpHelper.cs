/*----------------------------------------------------------------
    Copyright (C) 2018 Senparc

    文件名：HttpHelper.cs
    文件功能描述：Senparc.Common.SDK Http帮助类


    创建标识：MartyZane - 20181030

----------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace Senparc.Common.SDK
{
    /// <summary>
    /// Http帮助类
    /// </summary>
    public static class HttpHelper
    {
        static HttpHelper()
        {
            ServicePointManager.DefaultConnectionLimit = 512;
            UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.17 (KHTML, like Gecko) Chrome/24.0.1312.57 Safari/537.17";
            Timeout = 100000;
        }

        class MyWebClient : WebClient
        {
            protected override WebRequest GetWebRequest(Uri address)
            {
                var request = base.GetWebRequest(address) as HttpWebRequest;
                if (request == null) return null;
                request.Timeout = Timeout;
                request.UserAgent = UserAgent;
                return request;
            }
        }

        /// <summary>
        /// 获取或设置 使用的UserAgent信息
        /// </summary>
        /// <remarks>
        /// 可以到<see cref="http://www.sum16.com/resource/user-agent-list.html"/>查看更多User-Agent
        /// </remarks>
        public static String UserAgent { get; set; }
        /// <summary>
        /// 获取或设置 请求超时时间
        /// </summary>
        public static Int32 Timeout { get; set; }

        public static Boolean GetContentString(String url, out String message, Encoding encoding = null)
        {
            try
            {
                if (encoding == null) encoding = Encoding.UTF8;
                using (var wc = new MyWebClient())
                {
                    message = encoding.GetString(wc.DownloadData(url));
                    return true;
                }
            }
            catch (Exception exception)
            {
                message = exception.Message;
                return false;
            }
        }

        /// <summary>
        ///  POST数据
        /// </summary>
        /// <param name="url"></param>
        /// <param name="data"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public static String Post(String url, Byte[] data, Encoding encoding = null)
        {
            if (encoding == null) encoding = Encoding.UTF8;
            String str;
            using (var wc = new MyWebClient())
            {
                wc.Headers["Content-Type"] = "application/x-www-form-urlencoded";
                var ret = wc.UploadData(url, "POST", data);
                str = encoding.GetString(ret);
            }
            return str;
        }

        public static Byte[] DownloadData(String address)
        {
            Byte[] data;
            using (var wc = new MyWebClient())
            {
                data = wc.DownloadData(address);
            }
            return data;
        }
        public static Int64 GetContentLength(String url)
        {
            Int64 length;
            var req = (HttpWebRequest)WebRequest.Create(url);
            req.UserAgent = UserAgent;
            req.Method = "HEAD";
            req.Timeout = 5000;
            var res = (HttpWebResponse)req.GetResponse();
            if (res.StatusCode == HttpStatusCode.OK)
            {
                length = res.ContentLength;
            }
            else
            {
                length = -1;
            }
            res.Close();
            return length;
        }

        /// <summary>
        /// 当前日期时间
        /// </summary>
        public static string ClientDateTime { get; set; }

        /// <summary>
        /// 客户端IP
        /// </summary>
        public static string ClientIPAddress { get; set; }

        /// <summary>
        /// 客户端访问URL
        /// </summary>
        public static string ClientAskURL { get; set; }

        /// <summary>
        /// 客户端IPv6
        /// </summary>
        public static string ClientIPv6 { get; set; }

        /// <summary>
        /// 客户端IPv4
        /// </summary>
        public static string ClientIPv4 { get; set; }

        /// <summary>
        /// 报文头内容
        /// </summary>
        public static string ContentType { get; set; }

        /// <summary>
        /// 获取客户端日期时间
        /// </summary>
        public static DateTime GetClientDateTime()
        {
            return DateTime.Now;
        }

        /// <summary>
        /// 获取客户端IP地址
        /// </summary>
        public static string GetClientIPAddress()
        {
            IPHostEntry IpEntry = Dns.GetHostEntry(Dns.GetHostName());
            //这样,如果没有安装IPV6协议,可以取得IP地址.  但是如果安装了IPV6,就取得的是IPV6的IP地址.
            string m_ClientIPAddress = IpEntry.AddressList[0].ToString();
            if(m_ClientIPAddress == "")
            {
                //这样就在IPV6的情况下取得IPV4的IP地址.
                //但是,如果本机有很多块网卡, 如何得到IpEntry.AddressList[多少]才是本机的局网IPV4地址呢?
                m_ClientIPAddress = IpEntry.AddressList[1].ToString();
            }
            return m_ClientIPAddress;
        }

        /// <summary> 
        /// 取得客户端真实IP。如果有代理则取第一个非内网地址 
        /// </summary> 
        public static string GetRealIPAddress()
        {
            string result = String.Empty;
            if(HttpContext.Current == null)
            {
                return GetClientIPv4();
            }
            result = HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
            if(result != null && result != String.Empty)
            {
                //可能有代理 
                if(result.IndexOf(".") == -1)     //没有“.”肯定是非IPv4格式 
                    result = null;
                else
                {
                    if(result.IndexOf(",") != -1)
                    {
                        //有“,”，估计多个代理。取第一个不是内网的IP。 
                        result = result.Replace(" ", "").Replace("'", "");
                        string[] temparyip = result.Split(",;".ToCharArray());
                        for(int i = 0; i < temparyip.Length; i++)
                        {
                            if(IsIPAddress(temparyip[i])
                                && temparyip[i].Substring(0, 3) != "10."
                                && temparyip[i].Substring(0, 7) != "192.168"
                                && temparyip[i].Substring(0, 7) != "172.16.")
                            {
                                return temparyip[i];     //找到不是内网的地址 
                            }
                        }
                    }
                    else if(IsIPAddress(result)) //代理即是IP格式 
                        return result;
                    else
                        result = null;     //代理中的内容 非IP，取IP 
                }
            }
            string IpAddress = (HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"] != null && HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"] != String.Empty) ? HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"] : HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];


            if(null == result || result == String.Empty)
                result = HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];

            if(result == null || result == String.Empty)
                result = HttpContext.Current.Request.UserHostAddress;
            return result;

        }

        #region bool IsIPAddress(str1) 判断是否是IP格式 
        /**//// <summary>
            /// 判断是否是IP地址格式 0.0.0.0
            /// </summary>
            /// <param name="str1">待判断的IP地址</param>
            /// <returns>true or false</returns>
        public static bool IsIPAddress(string str1)
        {
            if(str1 == null || str1 == string.Empty || str1.Length < 7 || str1.Length > 15) return false;
            string regformat = @"^\d{1,3}[\.]\d{1,3}[\.]\d{1,3}[\.]\d{1,3}$";
            Regex regex = new Regex(regformat, RegexOptions.IgnoreCase);
            return regex.IsMatch(str1);
        }
        #endregion

        /// <summary>
        /// 获取客户端当前访问的URL
        /// </summary>
        public static string GetClientCurrentAskUrl()
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// 获取客户端IPv6
        /// </summary>
        public static string GetClientIPv6()
        {
            IPHostEntry IpEntry = Dns.GetHostEntry(Dns.GetHostName());
            //这样,如果没有安装IPV6协议,可以取得IP地址.  但是如果安装了IPV6,就取得的是IPV6的IP地址.
            string m_ClientIPv6 = IpEntry.AddressList[0].ToString();
            return m_ClientIPv6;
        }

        /// <summary>
        /// 获取客户端IPv4
        /// </summary>
        public static string GetClientIPv4()
        {
            IPHostEntry IpEntry = Dns.GetHostEntry(Dns.GetHostName());
            //这样,如果没有安装IPV6协议,可以取得IP地址.  但是如果安装了IPV6,就取得的是IPV6的IP地址.
            string m_ClientIPv4 = IpEntry.AddressList[1].ToString();
            return m_ClientIPv4;
        }

        /// <summary>
        /// 获取客户端用户名
        /// </summary>
        public static string GetClientUseName()
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// 打开指定网页
        /// </summary>
        /// <param name="m_URL">网页地址</param>
        public static bool OpenURL(string m_URL)
        {
            System.Diagnostics.Process.Start(m_URL);
            return true;
        }

        /// <summary>
        /// 模拟提交数据
        /// </summary>
        /// <param name="url"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public static string HttpPostData(string url, string param, string contentType = "application/x-www-form-urlencoded")
        {
            var result = string.Empty;
            //注意提交的编码 这边是需要改变的 这边默认的是Default：系统当前编码
            byte[] postData = Encoding.UTF8.GetBytes(param);

            // 设置提交的相关参数 
            HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
            Encoding myEncoding = Encoding.UTF8;
            request.Method = "POST";
            request.KeepAlive = false;
            request.AllowAutoRedirect = true;
            request.ContentType = contentType;
            request.UserAgent = "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.1; SV1; .NET CLR 2.0.50727; .NET CLR  3.0.04506.648; .NET CLR 3.5.21022; .NET CLR 3.0.4506.2152; .NET CLR 3.5.30729)";
            request.ContentLength = postData.Length;

            // 提交请求数据 
            System.IO.Stream outputStream = request.GetRequestStream();
            outputStream.Write(postData, 0, postData.Length);
            outputStream.Close();

            HttpWebResponse response;
            Stream responseStream;
            StreamReader reader;
            string srcString;
            response = request.GetResponse() as HttpWebResponse;
            responseStream = response.GetResponseStream();
            reader = new System.IO.StreamReader(responseStream, Encoding.GetEncoding("UTF-8"));
            srcString = reader.ReadToEnd();
            result = srcString;   //返回值赋值
            reader.Close();

            return result;
        }

        /// <summary>
        /// 模拟授权提交数据
        /// </summary>
        /// <param name="url"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public static string HttpPostDataByAuth(string url, string param, string user, string password, string contentType = "application/x-www-form-urlencoded")
        {
            var result = string.Empty;
            //注意提交的编码 这边是需要改变的 这边默认的是Default：系统当前编码
            byte[] postData = Encoding.UTF8.GetBytes(param);

            // 设置提交的相关参数 
            HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
            Encoding myEncoding = Encoding.UTF8;
            request.Method = "POST";
            request.KeepAlive = false;
            request.AllowAutoRedirect = true;
            request.ContentType = contentType;
            request.UserAgent = "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.1; SV1; .NET CLR 2.0.50727; .NET CLR  3.0.04506.648; .NET CLR 3.5.21022; .NET CLR 3.0.4506.2152; .NET CLR 3.5.30729)";
            request.ContentLength = postData.Length;
            //获得用户名密码的Base64编码
            string code = Convert.ToBase64String(Encoding.ASCII.GetBytes(string.Format("{0}:{1}", user, password)));

            //添加Authorization到HTTP头
            request.Headers.Add("Authorization", "Basic " + code);

            // 提交请求数据 
            System.IO.Stream outputStream = request.GetRequestStream();
            outputStream.Write(postData, 0, postData.Length);
            outputStream.Close();

            HttpWebResponse response;
            Stream responseStream;
            StreamReader reader;
            string srcString;
            response = request.GetResponse() as HttpWebResponse;
            responseStream = response.GetResponseStream();
            reader = new System.IO.StreamReader(responseStream, Encoding.GetEncoding("UTF-8"));
            srcString = reader.ReadToEnd();
            result = srcString;   //返回值赋值
            reader.Close();

            return result;
        }

        /// <summary>
        /// 获取不加密网页源码
        /// </summary>
        /// <param name="m_URL">网址</param>
        /// <param name="m_EncodeType">编码类型</param>
        public static string GetWebCode(string m_URL, Encoding m_EncodeType)
        {
            //指定请求
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(m_URL);
            //得到返回
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            //得到流
            Stream recStream = response.GetResponseStream();
            //编码方式
            //Encoding gb2312 = Encoding.GetEncoding("gb2312");
            //指定转换为gb2312编码
            StreamReader sr = new StreamReader(recStream, m_EncodeType);
            //以字符串方式得到网页内容
            String content = sr.ReadToEnd();
            //将网页内容显示在TextBox中
            return content;
        }

        /// <summary>
        /// 获取客户端网页代码
        /// </summary>
        public static string GetWebClient()
        {
            try
            {
                WebClient MyWebClient = new WebClient();
                MyWebClient.Credentials = CredentialCache.DefaultCredentials;//获取或设置用于向Internet资源的请求进行身份验证的网络凭据
                //Byte[] pageData = MyWebClient.DownloadData("http://www.163.com"); //从指定网站下载数据
                Byte[] pageData = MyWebClient.DownloadData("http://test.zh.com/housedeal/interfaceDealTransfers?contract_code=NF1610649"); //从指定网站下载数据
                //string pageHtml = Encoding.Default.GetString(pageData);  //如果获取网站页面采用的是GB2312，则使用这句            

                string pageHtml = Encoding.UTF8.GetString(pageData); //如果获取网站页面采用的是UTF-8，则使用这句
                //Console.WriteLine(pageHtml);//在控制台输入获取的内容
                //using (StreamWriter sw = new StreamWriter("c:\\test\\ouput.html"))//将获取的内容写入文本
                //{
                //    sw.Write(pageHtml);
                //}
                //Console.ReadLine(); //让控制台暂停,否则一闪而过了       
                return pageHtml;
            }
            catch(WebException webEx)
            {
                //Console.WriteLine(webEx.Message.ToString());
                return webEx.Message.ToString();
            }
        }

        ///// <summary>
        ///// 获取指定的内容
        ///// </summary>
        //private string querystring(string m_qs)
        //{
        //    String pageurl;
        //    string m_Result = "";
        //    string m_Temp;
        //    try
        //    {
        //        pageurl = Request.Url.ToString();
        //        if (pageurl.IndexOf("?") != -1)
        //        {
        //            m_Temp = pageurl.Replace("?", "?&");
        //            string[] saurl = m_Temp.Split('&');
        //            for (int i = 1; i < saurl.Count(); i++)
        //            {
        //                if (saurl[i].IndexOf(m_qs + "=") != -1)
        //                {
        //                    m_Result = saurl[i].Replace(m_qs + "=", "");
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        m_Result = ex.Message;
        //    }
        //    return m_Result;
        //}
    }
}
