using System;
using System.Threading;
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
            //debug=true state, record log
            {
                Config.IsDebug = true;
                Assert.AreEqual(true, Config.IsDebug);

                var guid = Guid.NewGuid().ToString();
                SenparcTrace.SendCustomLog("IsDebugTest:Debug", guid);
                Thread.Sleep(1500);//Need to wait for write completion
                Assert.IsTrue(UnitTestHelper.CheckKeywordsExist(SenparcTraceTests.LogFilePath, guid));
            }

            //debug=false state, do not record log
            {
                Config.IsDebug = false;
                Assert.AreEqual(false, Config.IsDebug);

                var guid = Guid.NewGuid().ToString();
                SenparcTrace.SendCustomLog("IsDebugTest:Not Debug", guid);
                Thread.Sleep(1500);//Need to wait for write completion
                Assert.IsFalse(UnitTestHelper.CheckKeywordsExist(SenparcTraceTests.LogFilePath, guid));
            }
        }


    }
}
