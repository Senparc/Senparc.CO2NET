using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Senparc.CO2NET.Cache;
using Senparc.CO2NET.HttpUtility;
using Senparc.CO2NET.Sample.netcore3.Models;
using Senparc.CO2NET.Trace;
using Senparc.CO2NET.Utilities.HttpUtility.HttpPost;

namespace Senparc.CO2NET.Sample.netcore3.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IServiceProvider _serviceProvider;

        public HomeController(ILogger<HomeController> logger,IServiceProvider serviceProvider /*,IBaseObjectCacheStrategy oCache*/)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

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
            var result = RequestUtility.HttpPost(_serviceProvider,"https://localhost:44351/Home/PostParameter", formData: new Dictionary<string, string>() { { "code", SystemTime.NowTicks.ToString() } });
            return Content(result);
        }


        [HttpPost]
        public IActionResult PostParameter(string code)
        {
            return Content($"已经到达Post目标地址。{SystemTime.Now}, code: {code}");
        }

        #endregion

        #region Post 文件

        /// <summary>
        /// 记录耗时，并返回平均时间
        /// </summary>
        /// <param name="byStream"></param>
        /// <param name="cost"></param>
        /// <returns></returns>
        private async Task<double> RecordTimeCost(string byStream, TimeSpan cost)
        {
            var cache = CacheStrategyFactory.GetObjectCacheStrategyInstance();
            var cacheKey = $"RecordTimeCost-{byStream}";
            dynamic record;
            if (await cache.CheckExistedAsync(cacheKey))
            {
                record = await cache.GetAsync<dynamic>(cacheKey);
                record = new { ViewCount = record.ViewCount + 1, TotalCost = ((TimeSpan)record.TotalCost).Add(cost) };
                //record.ViewCount++;//增加访问量
                //record.TotalCost = ((TimeSpan)record.TotalCost).Add(cost);//增加总耗时
            }
            else
            {
                record = new { ViewCount = 1, TotalCost = cost };
            }

            await cache.SetAsync(cacheKey, record, TimeSpan.FromMinutes(10));//更新信息

            return ((TimeSpan)record.TotalCost).TotalMilliseconds / (int)record.ViewCount;
        }

        public async Task<IActionResult> PostFile(string byStream = null)
        {
            var dt1 = SystemTime.Now;
            var filePath = Path.GetFullPath("App_Data/cover.png");//也可以上传其他任意文件
            var fileDictionary = new Dictionary<string, string>();
            if (byStream != null)
            {
                //使用Stream传入，而不是文件名
                SenparcTrace.SendCustomLog("Post 文件信息", $"使用文件流放入 fileDictionary 中，并将修改文件名。");
                using (var fs = System.IO.File.OpenRead(filePath))
                {
                    var formFileData = new FormFileData(Path.GetFileName(filePath), fs);
                    formFileData.FileName = $"changed-{formFileData.FileName}";//修改文件名
                    fileDictionary["image"] = formFileData.GetFileValue();
                }
            }
            else
            {
                SenparcTrace.SendCustomLog("Post 文件信息", $"使用文件物理地址放入 fileDictionary 中，保留原文件名。");
                fileDictionary["image"] = filePath;
            }

            var url = "https://localhost:44351/Home/PostFile";
            var result = await RequestUtility.HttpPostAsync(_serviceProvider, url, fileDictionary: fileDictionary);//获取图片的base64编码
            var note = byStream != null ? "使用文件流" : "使用文件名";
            var timeCost = SystemTime.NowDiff(dt1);
            var averageCost = await RecordTimeCost(byStream, timeCost);
            var html = $@"<html>
<head>
<meta http-equiv=Content-Type content=""text/html;charset=utf-8"">
<title>CO2NET 文件 Post 测试 - {note}</title>
</head>
<body>
    <p>如果在下方看到《微信开发深度解析》图书封面，表示测试成功（{note}）。可通过 SenparcTrace 日志查看更多过调试信息日志。</p>
    <p>耗时：{timeCost.TotalMilliseconds} ms，平均：{averageCost} ms</p>
       <img src=""data:image/png; base64,{result}"" />
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

                SenparcTrace.SendCustomLog("Post 文件信息", $"Name：{image.Name}，FileName：{image.FileName}，Length：{image.Length}");

                return Content(base64, "text/plain");
            }
        }

        #endregion



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
