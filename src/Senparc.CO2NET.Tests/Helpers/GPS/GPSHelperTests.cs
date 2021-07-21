using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Senparc.CO2NET.Helpers;

namespace Senparc.CO2NET.Tests.Helpers
{
    [TestClass]
    public class GPSHelperTests
    {
        [TestMethod]
        public void Distance2Test()
        {
            var result = GpsHelper.Distance2(38.50225, 118.1186, 38.66333, 118.457);
            Console.WriteLine(result);
            Assert.AreEqual(34440, (int)result);//近似算到证书
        }

        [TestMethod]
        public void DistanceTest()
        {
            var result = GpsHelper.Distance(38.50225, 118.1186, 38.66333, 118.457);
            Console.WriteLine(result);
            Assert.AreEqual(34477, (int)result);//近似算到证书
        }


        [TestMethod]
        public void GetLatitudeDifferenceTest()
        {
            var result = GpsHelper.GetLatitudeDifference(100);
            Console.WriteLine(result);
            //Assert.AreEqual("0.900900900900901", result.ToString());
            Assert.AreEqual("0.9009009009009009", result.ToString());
        }


        [TestMethod]
        public void GetLongitudeDifferenceTest()
        {
            var result = GpsHelper.GetLongitudeDifference(100);
            Console.WriteLine(result);
            //Assert.AreEqual("0.909090909090909", result.ToString());
            Assert.AreEqual("0.9090909090909091", result.ToString());
        }

    }
}
