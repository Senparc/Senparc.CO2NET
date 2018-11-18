using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Senparc.CO2NET.Cache;
using Senparc.CO2NET.HttpUtility;
using Senparc.CO2NET.Sample.netcore.Models;
using Senparc.CO2NET.Trace;

namespace Senparc.CO2NET.Sample.netcore.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            var cache = CacheStrategyFactory.GetObjectCacheStrategyInstance();
            var count = cache.Get<int>("IndexTest");
            count++;
            cache.Set("IndexTest", count);

            ViewData["IndexTest"] = count;
            ViewData["CacheType"] = cache.GetType();

            return View();
        }


        public IActionResult LogTest()
        {
            SenparcTrace.SendCustomLog("日志记录测试", "加入到队列");
            return Content("OK");
        }


        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        #region Post 方法测试

        /// <summary>
        /// Post方法测试
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult PostTest()
        {
         var result =   RequestUtility.HttpPost("https://localhost:44335/Home/PostTest", formData: new Dictionary<string, string>() { { "code", "12335aaa" } });
            return Content(result);
        }


        [HttpPost]
        public IActionResult PostTest(string code)
        {
            return Content(SystemTime.Now + "," + code);
        }
        #endregion

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
