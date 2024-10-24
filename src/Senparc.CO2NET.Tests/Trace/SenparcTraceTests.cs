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
            //Comment
            var mockEnv = new Mock<Microsoft.Extensions.Hosting.IHostEnvironment/*IHostingEnvironment*/>();
            mockEnv.Setup(z => z.ContentRootPath).Returns(() => UnitTestHelper.RootPath);
            var register = Senparc.CO2NET.AspNet.RegisterServices.RegisterService.Start(mockEnv.Object, new SenparcSetting() { IsDebug = true });

            IServiceCollection services = new ServiceCollection();
            services.AddMemoryCache();//Used for caching

            SenparcDI.GlobalServiceCollection = services;//Required for registration

            //var mockRegisterService = new Mock<RegisterService>();
            //mockRegisterService.Setup(z => z.ServiceCollection).Returns(() => services);

            //Delete log file
            //File.Delete(_logFilePath);
        }


        [TestMethod]
        public void LogTest()
        {
            //Directly call this method to record in the log file, instead of the system log
            var keyword = Guid.NewGuid().ToString();//Test string
            SenparcTrace.Log($"Add Log: {keyword}");

            var dt1 = SystemTime.Now;
            while (SystemTime.DiffTotalMS(dt1) < 600)
            {
                //Wait for execution
            }

            SenparcMessageQueue.OperateQueue();

            Console.WriteLine(SenparcMessageQueue.MessageQueueDictionary.Count);

            Console.WriteLine(ThreadUtility.AsynThreadCollection.Count);
            //Assert.IsTrue(UnitTestHelper.CheckKeywordsExist(_logFilePath, keyword));
        }


        [TestMethod]
        public void SendCustomLogTest()
        {
            var keyword = Guid.NewGuid().ToString();//Test string
            SenparcTrace.SendCustomLog("Title", $"Add Log: {keyword}");

            var dt1 = SystemTime.Now;
            while (SystemTime.DiffTotalMS(dt1) < 800)
            {
                //Wait for execution
            }

            Assert.IsTrue(UnitTestHelper.CheckKeywordsExist(LogFilePath, "Title", keyword));
        }


        [TestMethod]
        public void SendApiLogTest()
        {
            var url = "http://www.senparc.com";
            var result = Guid.NewGuid().ToString();//Test string
            SenparcTrace.SendApiLog(url, result);

            var dt1 = SystemTime.Now;
            while (SystemTime.DiffTotalMS(dt1) < 800)
            {
                //Wait for execution
            }

            Assert.IsTrue(UnitTestHelper.CheckKeywordsExist(LogFilePath, url, result));
        }


        [TestMethod]
        public void SendApiPostDataLogTest()
        {
            var url = "http://www.senparc.com";
            var data = Guid.NewGuid().ToString();//Test string
            SenparcTrace.SendApiLog(url, data);

            var dt1 = SystemTime.Now;
            while (SystemTime.DiffTotalMS(dt1) < 800)
            {
                //Wait for execution
            }

            Assert.IsTrue(UnitTestHelper.CheckKeywordsExist(LogFilePath, url, data));
        }



        [TestMethod]
        public void BaseExceptionLogTest()
        {
            var keyword = Guid.NewGuid().ToString();//Test string
            var ex = new BaseException("Test Excepption" + keyword);
            //Log recording method, call this method when initializing a BaseException
            SenparcTrace.BaseExceptionLog(ex);

            var dt1 = SystemTime.Now;
            while (SystemTime.DiffTotalMS(dt1) < 800)
            {
                //Wait for execution
            }

            Assert.IsTrue(UnitTestHelper.CheckKeywordsExist(LogFilePath, "�����쳣", keyword));
        }

        [TestMethod]
        public void OnLogFuncTest()
        {
            var onlogCount = 0;
            SenparcTrace.OnLogFunc = () => onlogCount++;

            var keyword = Guid.NewGuid().ToString();//Test string
            SenparcTrace.SendCustomLog("Test OnLogFuncTest", keyword);

            var dt1 = SystemTime.Now;
            while (SystemTime.DiffTotalMS(dt1) < 800)
            {
                //Wait for execution
            }

            Assert.IsTrue(UnitTestHelper.CheckKeywordsExist(LogFilePath, keyword));
            Assert.AreEqual(1, onlogCount);
        }


        [TestMethod]
        public void OnBaseExceptionFuncTest()
        {
            var onlogCount = 0;
            SenparcTrace.OnLogFunc = () => onlogCount++;

            var keyword = Guid.NewGuid().ToString();//Test string
            var ex = new BaseException("Test Exception" + keyword);
            //Log recording method, call this method when initializing a BaseException
            SenparcTrace.BaseExceptionLog(ex);

            var dt1 = SystemTime.Now;
            while (SystemTime.DiffTotalMS(dt1) < 1000)
            {
                //Wait for execution
            }

            Assert.IsTrue(UnitTestHelper.CheckKeywordsExist(LogFilePath, keyword));
            Assert.AreEqual(2, onlogCount);
        }

    }
}
