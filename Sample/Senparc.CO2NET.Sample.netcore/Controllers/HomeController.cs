using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Senparc.CO2NET.Cache;
using Senparc.CO2NET.Sample.netcore.Models;

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
