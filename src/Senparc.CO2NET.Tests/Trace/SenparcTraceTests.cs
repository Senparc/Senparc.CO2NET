using System;
using System.IO;
using System.Threading;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Senparc.CO2NET.Exceptions;
using Senparc.CO2NET.MessageQueue;
using Senparc.CO2NET.RegisterServices;
using Senparc.CO2NET.Threads;
using Senparc.CO2NET.Trace;

namespace Senparc.CO2NET.Tests.Trace
{
    [TestClass]
    public class SenparcTraceTests
    {
        public static string LogFilePath => Path.Combine(UnitTestHelper.RootPath, "App_Data", "SenparcTraceLog", $"SenparcTrace-{SystemTime.Now.ToString("yyyyMMdd")}.log");


        public SenparcTraceTests()
        {
            //注册
            var mockEnv = new Mock<Microsoft.Extensions.Hosting.IHostEnvironment/*IHostingEnvironment*/>();
            mockEnv.Setup(z => z.ContentRootPath).Returns(() => UnitTestHelper.RootPath);
            var register = Senparc.CO2NET.AspNet.RegisterServices.RegisterService.Start(mockEnv.Object, new SenparcSetting() { IsDebug = true });

            IServiceCollection services = new ServiceCollection();
            services.AddMemoryCache();//使用内存缓存

            SenparcDI.GlobalServiceCollection = services;//本地缓存需要用到

            //var mockRegisterService = new Mock<RegisterService>();
            //mockRegisterService.Setup(z => z.ServiceCollection).Returns(() => services);

            //删除日志文件
            //File.Delete(_logFilePath);
        }


        [TestMethod]
        public void LogTest()
        {
            //直接调用此方法不会记录到log文件中，而是输出到系统日志中
            var keyword = Guid.NewGuid().ToString();//随机字符串
            SenparcTrace.Log($"添加Log：{keyword}");

            var dt1 = SystemTime.Now;
            while (SystemTime.DiffTotalMS(dt1) < 600)
            {
                //等待队列执行
            }

            SenparcMessageQueue.OperateQueue();

            Console.WriteLine(SenparcMessageQueue.MessageQueueDictionary.Count);

            Console.WriteLine(ThreadUtility.AsynThreadCollection.Count);
            //Assert.IsTrue(UnitTestHelper.CheckKeywordsExist(_logFilePath,keyword));
        }


        [TestMethod]
        public void SendCustomLogTest()
        {
            var keyword = Guid.NewGuid().ToString();//随机字符串
            SenparcTrace.SendCustomLog("标题", $"添加Log：{keyword}");

            var dt1 = SystemTime.Now;
            while (SystemTime.DiffTotalMS(dt1) < 800)
            {
                //等待队列执行
            }

            Assert.IsTrue(UnitTestHelper.CheckKeywordsExist(LogFilePath, "标题", keyword));
        }


        [TestMethod]
        public void SendApiLogTest()
        {
            var url = "http://www.senparc.com";
            var result = Guid.NewGuid().ToString();//随机字符串
            SenparcTrace.SendApiLog(url, result);

            var dt1 = SystemTime.Now;
            while (SystemTime.DiffTotalMS(dt1) < 800)
            {
                //等待队列执行
            }

            Assert.IsTrue(UnitTestHelper.CheckKeywordsExist(LogFilePath, url, result));
        }


        [TestMethod]
        public void SendApiPostDataLogTest()
        {
            var url = "http://www.senparc.com";
            var data = Guid.NewGuid().ToString();//随机字符串
            SenparcTrace.SendApiLog(url, data);

            var dt1 = SystemTime.Now;
            while (SystemTime.DiffTotalMS(dt1) < 800)
            {
                //等待队列执行
            }

            Assert.IsTrue(UnitTestHelper.CheckKeywordsExist(LogFilePath, url, data));
        }



        [TestMethod]
        public void BaseExceptionLogTest()
        {
            var keyword = Guid.NewGuid().ToString();//随机字符串
            var ex = new BaseException("测试异常：" + keyword);
            //Log会记录两次，第一次是在BaseException初始化的时候会调用此方法
            SenparcTrace.BaseExceptionLog(ex);

            var dt1 = SystemTime.Now;
            while (SystemTime.DiffTotalMS(dt1) < 800)
            {
                //等待队列执行
            }

            Assert.IsTrue(UnitTestHelper.CheckKeywordsExist(LogFilePath, "测试异常", keyword));
        }

        [TestMethod]
        public void OnLogFuncTest()
        {
            var onlogCount = 0;
            SenparcTrace.OnLogFunc = () => onlogCount++;

            var keyword = Guid.NewGuid().ToString();//随机字符串
            SenparcTrace.SendCustomLog("测试OnLogFuncTest", keyword);

            var dt1 = SystemTime.Now;
            while (SystemTime.DiffTotalMS(dt1) < 800)
            {
                //等待队列执行
            }

            Assert.IsTrue(UnitTestHelper.CheckKeywordsExist(LogFilePath, keyword));
            Assert.AreEqual(1, onlogCount);
        }


        [TestMethod]
        public void OnBaseExceptionFuncTest()
        {
            var onlogCount = 0;
            SenparcTrace.OnLogFunc = () => onlogCount++;

            var keyword = Guid.NewGuid().ToString();//随机字符串
            var ex = new BaseException("测试异常：" + keyword);
            //Log会记录两次，第一次是在BaseException初始化的时候会调用此方法
            SenparcTrace.BaseExceptionLog(ex);

            var dt1 = SystemTime.Now;
            while (SystemTime.DiffTotalMS(dt1) < 800)
            {
                //等待队列执行
            }

            Assert.IsTrue(UnitTestHelper.CheckKeywordsExist(LogFilePath, keyword));
            Assert.AreEqual(2, onlogCount);
        }

    }
}
