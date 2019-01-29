using Microsoft.VisualStudio.TestTools.UnitTesting;
using Senparc.CO2NET.Utilities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Senparc.CO2NET.Tests.Utilities
{
    [TestClass]
    public class ServerUtilityTests : BaseTest
    {
        [TestMethod]
        public void DllMapPathTest()
        {
            var path = "~/App_Data/log.log";
            var result = ServerUtility.DllMapPath(path);
            Console.WriteLine(result);
            Assert.IsTrue(result.EndsWith(@"\bin\Test\netcoreapp2.2\App_Data\log.log") || 
                          result.EndsWith(@"\bin\Release\netcoreapp2.2\App_Data\log.log"));
        }

        [TestMethod]
        public void ContentRootMapPathTest()
        {
            var path = "~/App_Data/log.log";
            var result = ServerUtility.ContentRootMapPath(path);
            Console.WriteLine(result);
            Assert.IsTrue(result.EndsWith(@"src\Senparc.CO2NET.Tests\App_Data\log.log"));
        }

    }
}
