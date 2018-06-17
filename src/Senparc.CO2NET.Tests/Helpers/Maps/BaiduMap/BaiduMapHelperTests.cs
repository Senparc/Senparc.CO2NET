using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Senparc.CO2NET.Helpers;
using Senparc.CO2NET.Helpers.BaiduMap;

namespace Senparc.CO2NET.Tests.Helpers
{
    [TestClass]
    public class BaiduMapHelperTests
    {
        [TestMethod]
        public void BaiduMapHelperTest()
        {
            var markers = new List<BaiduMarkers>();
            markers.Add(new BaiduMarkers()
            {
                url = "http://www.senparc.com",
                Color = "#ff0000",
                Label = "A",
                Latitude = 31.3,
                Longitude = 120.6,
                Size = BaiduMarkerSize.m
            });

            var result = BaiduMapHelper.GetBaiduStaticMap(120.6, 31.3,2,15, markers, 400, 300);
            Console.WriteLine(result);
            Assert.IsTrue(result.Contains("api.map.baidu.com"));
        }
    }
}
