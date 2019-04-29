using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using Senparc.CO2NET.Cache;
using Senparc.CO2NET.HttpUtility;
using Senparc.CO2NET.Sample.netcore.Models;

namespace Senparc.CO2NET.Sample.netcore.Controllers
{
    /// <summary>
    /// 提供给 Senparc.WeixinTests/Utilities/HttpUtility/PostTests.cs使用
    /// </summary>
    public class ForTestController : Controller
    {
        [HttpPost]
        public ActionResult PostTest()
        {
            string data;

            using (var sr = new StreamReader(Request.GetRequestMemoryStream()))
            {
                data = sr.ReadToEnd();
            }

            var isAjax = Request.IsAjaxRequest();

            var testCount = 0;
            if (Request.Cookies["TestCount"] != null)
            {
                testCount = int.Parse(Request.Cookies["TestCount"]);
            }
            testCount++;

            Response.Headers.Add(HeaderNames.SetCookie,
                new Microsoft.Extensions.Primitives.StringValues(
                    new[] {
                        $"TestCount={testCount}; path=/; domain=localhost; Expires=Tue, 19 Jan 2038 03:14:07 GMT;",
                        "TestCookie=20190429 20:27:45; path=/; domain=localhost; Expires=Tue, 19 Jan 2038 03:14:07 GMT;"
                    }));

            //Response.Cookies.Append("TestCookie", SystemTime.Now.ToString());
            //Response.Cookies.Append("TestCount", testCount.ToString());

            return Content($"data：{data}  Ajax:{isAjax} Server Time:{SystemTime.Now} TestCount：{testCount}");
        }
    }
}
