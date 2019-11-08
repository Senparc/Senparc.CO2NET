using Microsoft.AspNetCore.Mvc;
using Senparc.CO2NET.HttpUtility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Senparc.CO2NET.Sample.FW.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            //return View();
            return RedirectToAction("PostFile");
        }

        #region Post 文件

        public async Task<ActionResult> PostFile()
        {
            var filePath =Server.MapPath("~/App_Data/cover.png");//也可以上传其他任意文件
            var file = new Dictionary<string, string>() { { "image", filePath } };
            var url = "https://localhost:44344//Home/PostFile";
            var result = await RequestUtility.HttpPostAsync(url, fileDictionary: file);//获取图片的base64编码
            var html = $@"<html>
<head>
<meta http-equiv=Content-Type content=""text/html;charset=utf-8"">
<title>CO2NET 文件 Post 测试</title>
</head>
<body>
<p>如果在下方看到《微信开发深度解析》图书封面，表示测试成功</p>
<img src=""data:image/png;base64,{result}"" />
</body>
</html>";
            return Content(html, "text/html");
        }

        [HttpPost]
        public async Task<ActionResult> PostFile(HttpPostedFileBase image)
        {
            using (var ms = new MemoryStream())
            {
                await image.InputStream.CopyToAsync(ms);
                ms.Seek(0, SeekOrigin.Begin);

                byte[] bytes = new byte[ms.Length];
                await ms.ReadAsync(bytes, 0, bytes.Length);

                var base64 = Convert.ToBase64String(bytes);
                return Content(base64, "text/plain");
            }
        }

        #endregion


        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}