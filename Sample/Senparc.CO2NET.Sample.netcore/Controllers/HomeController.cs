using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
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

        /// <summary>
        /// 测试日志记录
        /// </summary>
        /// <returns></returns>
        public IActionResult LogTest()
        {
            var logMsg = $"加入到队列，通过 {Request.PathAndQuery()} 访问，{SystemTime.Now.ToString()}";
            SenparcTrace.SendCustomLog("日志记录测试", logMsg);
            return Content($"记录完成，请到【App_Data/SenparcTraceLog/SenparcTrace-{SystemTime.Now.ToString("yyyyMMdd")}.log】文件下查看记录，记录内容：{logMsg}");
        }


        #region Post 方法测试

        #region Post 参数

        /// <summary>
        /// Post方法测试
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult PostParameter()
        {
            var result = RequestUtility.HttpPost("https://localhost:44335/Home/PostParameter", formData: new Dictionary<string, string>() { { "code", SystemTime.NowTicks.ToString() } });
            return Content(result);
        }


        [HttpPost]
        public IActionResult PostParameter(string code)
        {
            return Content($"已经到达Post目标地址。{SystemTime.Now}, code: {code}");
        }

        #endregion

        #region Post 文件

        public async Task<IActionResult> PostFile(string byStream = null)
        {
            var filePath = Path.GetFullPath("App_Data/cover.png");//也可以上传其他任意文件

            if (byStream != null)
            {
                //使用Stream传入，而不是文件名
                using (var fs = System.IO.File.OpenRead(filePath))
                {
                    BinaryReader r = new BinaryReader(fs);
                    r.BaseStream.Seek(0, SeekOrigin.Begin);    //将文件指针设置到文件开
                    byte[] bytes = r.ReadBytes((int)r.BaseStream.Length);
                    filePath = Convert.ToBase64String(bytes);
                }
            }

            var file = new Dictionary<string, string>() { { "image", filePath } };
            var url = "https://localhost:44335/Home/PostFile";
            var result = await RequestUtility.HttpPostAsync(url, fileDictionary: file);//获取图片的base64编码
            var note = byStream != null ? "使用文件流" : "使用文件名";
            var html = $@"<html>
<head>
<meta http-equiv=Content-Type content=""text/html;charset=utf-8"">
<title>CO2NET 文件 Post 测试 - {note}</title>
    </head>
    <body>
    <p> 如果在下方看到《微信开发深度解析》图书封面，表示测试成功（{note}） </p>
       <img src=""data:image/png; base64,{ result}"" />
</body>
</html>";
            return Content(html, "text/html");
        }

        [HttpPost]
        public async Task<IActionResult> PostFile(IFormFile image)
        {
            using (var ms = new MemoryStream())
            {
                await image.CopyToAsync(ms);
                ms.Seek(0, SeekOrigin.Begin);

                byte[] bytes = new byte[ms.Length];
                await ms.ReadAsync(bytes, 0, bytes.Length);

                var base64 = Convert.ToBase64String(bytes);
                return Content(base64, "text/plain");
            }
        }

        #endregion



        #endregion


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
