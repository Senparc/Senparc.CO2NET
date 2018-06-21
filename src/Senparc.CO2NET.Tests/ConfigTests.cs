using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Senparc.CO2NET.Cache;
using Senparc.CO2NET.Tests.Trace;
using Senparc.CO2NET.Trace;

namespace Senparc.CO2NET.Tests
{
    [TestClass]
    public class ConfigTests : BaseTest
    {

        [TestMethod]
        public void IsDebugTest()
        {
            //debug=true 状态下会记录日志
            {
                Config.IsDebug = true;
                Assert.AreEqual(true, Config.IsDebug);

                var guid = Guid.NewGuid().ToString();
                SenparcTrace.SendCustomLog("IsDebugTest:Debug", guid);
                Assert.IsTrue(UnitTestHelper.CheckKeywordsExist(SenparcTraceTests.LogFilePath, guid));
            }

            //debug=false 状态下不会记录日志
            {
                Config.IsDebug = false;
                Assert.AreEqual(false, Config.IsDebug);

                var guid = Guid.NewGuid().ToString();
                SenparcTrace.SendCustomLog("IsDebugTest:Not Debug", guid);
                Assert.IsFalse(UnitTestHelper.CheckKeywordsExist(SenparcTraceTests.LogFilePath, guid));
            }
        }


    }
}
