using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Senparc.CO2NET.Cache;
using Senparc.CO2NET.AspNet.RegisterServices;
using Senparc.CO2NET.Tests.TestEntities;
using Senparc.CO2NET.Tests.Trace;
using Senparc.CO2NET.Threads;
using Senparc.CO2NET.Trace;
using Senparc.CO2NET.RegisterServices;

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
        public void RegisterThreadsTest()
        {
            var registerService = Senparc.CO2NET.AspNet.RegisterServices.RegisterService.Start(null, new SenparcSetting(true));
            Register.RegisterThreads(registerService);

            Assert.IsTrue(ThreadUtility.AsynThreadCollection.Count > 0);
        }

        #region RegisterTraceLogTest

        [TestMethod]
        public void RegisterTraceLogTest()
        {
            var registerService = Senparc.CO2NET.AspNet.RegisterServices.RegisterService.Start(null, new SenparcSetting(true));
            Register.RegisterTraceLog(registerService, RegisterTraceLogAction);
            Assert.IsTrue(registerTraceLogActionRun);
        }

        bool registerTraceLogActionRun = false;

        private void RegisterTraceLogAction()
        {
            registerTraceLogActionRun = true;

            SenparcTrace.SendCustomLog("Test系统日志", "Test系统启动");//只在Senparc.CO2NET.Config.IsDebug = true的情况下生效

            //自定义日志记录回调
            SenparcTrace.OnLogFunc = () =>
            {
                //加入每次触发Log后需要执行的代码
            };
        }

        #endregion


        [TestMethod]
        public void UseSenparcGlobalTest()
        {
            IRegisterService registerService = Senparc.CO2NET.AspNet.RegisterServices.RegisterService.Start(null, new SenparcSetting(true));
            registerService.UseSenparcGlobal(true, null);

            Assert.IsNotNull(Config.SenparcSetting);
            Assert.AreEqual(true, Config.SenparcSetting.IsDebug);
            Assert.AreEqual(true, Config.IsDebug);

            //测试缓存注册过程

            Func<IList<IDomainExtensionCacheStrategy>> func = () =>
            {
                var list = new List<IDomainExtensionCacheStrategy>();
                list.Add(TestExtensionCacheStrategy.Instance);
                return list;
            };

            registerService.UseSenparcGlobal(false, func);

        }
    }
}
