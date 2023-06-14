using Microsoft.VisualStudio.TestTools.UnitTesting;
using Senparc.CO2NET.ApiBind;
using Senparc.NeuChar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Senparc.CO2NET.WebApi.Tests.WebApiEngines
{

    [ApiBind(Ignore = true)]
    public static class ApiBindTestClass_Ignore
    {
        public static void MethodForTest(string accessTokenOrApi, string p1, string p2)
        {
            Console.WriteLine($"accessTokenOrApi:{accessTokenOrApi} , p1:{p1} , p2 {p2}");
        }
    }

    [ApiBind(Ignore = false)]
    public static class ApiBindTestClass_Allow
    {
        public static void MethodForTest(string accessTokenOrApi, string p1, string p2)
        {
            Console.WriteLine($"accessTokenOrApi:{accessTokenOrApi} , p1:{p1} , p2 {p2}");
        }
    }

    [TestClass]
    public class IgnoreTests : BaseTest
    {
        public IgnoreTests() : base(true)
        {

        }

        [TestMethod]
        public void IgnoreApiBindTest()
        {
            var apiData = ApiBindInfoCollection.Instance;
            apiData.Keys.ToList().ForEach(key => { Console.WriteLine(key); });

            Assert.IsFalse(apiData.Keys.Any(z => z.Contains(nameof(ApiBindTestClass_Ignore))));
            Assert.IsTrue(apiData.Keys.Any(z => z.Contains(nameof(ApiBindTestClass_Allow))));
        }
    }
}
