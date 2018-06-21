using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Senparc.CO2NET.Cache;
using Senparc.CO2NET.RegisterServices;
using Senparc.CO2NET.Tests.Trace;
using Senparc.CO2NET.Threads;
using Senparc.CO2NET.Trace;

namespace Senparc.CO2NET.Tests
{
    [TestClass]
    public class RegisterTests : BaseTest
    {
        [TestMethod]
        public void ChangeDefaultCacheNamespaceTest()
        {
            var guid = Guid.NewGuid().ToString();

            Config.DefaultCacheNamespace = guid;
            Assert.AreEqual(guid, Config.DefaultCacheNamespace);

            //测试缓存中实际的key
            var cache = CacheStrategyFactory.GetObjectCacheStrategyInstance();
            var cacheKey = cache.GetFinalKey("key");
            Console.WriteLine(cacheKey);
            Assert.IsTrue(cacheKey.Contains($":{guid}:"));

            Config.DefaultCacheNamespace = null;
            Assert.AreEqual("DefaultCache", Config.DefaultCacheNamespace);//返回默认值
        }

        [TestMethod]
        public void RegisterThreads()
        {
            var registerService = new RegisterService();
            Register.RegisterThreads(registerService);

            Assert.IsTrue(ThreadUtility.AsynThreadCollection.Count > 0);
        }

        #region RegisterTraceLogTest

        [TestMethod]
        public void RegisterTraceLogTest()
        {
            var registerService = new RegisterService();
            Register.RegisterTraceLog(registerService, RegisterTraceLogAction);
            Assert.IsTrue(registerTraceLogActionRun);
        }

        bool registerTraceLogActionRun = false;

        private void RegisterTraceLogAction()
        {
            registerTraceLogActionRun = true;

            SenparcTrace.SendCustomLog("Test系统日志", "Test系统启动");//只在Senparc.Weixin.Config.IsDebug = true的情况下生效

            //自定义日志记录回调
            SenparcTrace.OnLogFunc = () =>
            {
                //加入每次触发Log后需要执行的代码
            };
        }

        #endregion

   }
}
