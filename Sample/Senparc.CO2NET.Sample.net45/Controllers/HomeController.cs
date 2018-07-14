using Senparc.CO2NET.Cache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Senparc.CO2NET.Sample.net45.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            var cache = CacheStrategyFactory.GetObjectCacheStrategyInstance();
            var count = cache.Get<int>("IndexTest");
            count++;
            cache.Set("IndexTest", count);

            ViewData["IndexTest"] = count;
            ViewData["CacheType"] = cache.GetType();

            return View();
        }

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