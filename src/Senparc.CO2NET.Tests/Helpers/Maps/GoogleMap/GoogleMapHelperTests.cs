using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Senparc.CO2NET.Helpers;
using Senparc.CO2NET.Helpers.GoogleMap;

namespace Senparc.CO2NET.Tests.Helpers
{
    [TestClass]
    public class GoogleMapHelperTests
    {
        [TestMethod]
        public void GoogleMapHelperTest()
        {
            var markers = new List<GoogleMapMarkers>();
            markers.Add(new GoogleMapMarkers()
            {
                Y = 31.3,
                X = 120.6,
                Color = "#ff0000",
                Label = "S",
                Size = GoogleMapMarkerSize.mid
            });

            var result = GoogleMapHelper.GetGoogleStaticMap(5, markers, size: "800x800");
            Console.WriteLine(result);
            Assert.IsTrue(result.Contains("maps.googleapis.com"));
        }
    }
}
