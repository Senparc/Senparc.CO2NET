using System;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Senparc.CO2NET.Exceptions;
using Senparc.CO2NET.RegisterServices;
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
            var mockEnv = new Mock<IHostingEnvironment>();
            mockEnv.Setup(z => z.ContentRootPath).Returns(() => UnitTestHelper.RootPath);
            RegisterService.Start(mockEnv.Object, new SenparcSetting() { IsDebug = true });

            //删除日志文件
            //File.Delete(_logFilePath);
        }


        [TestMethod]
        public void LogTest()
        {
            //直接调用此方法不会记录到log文件中，而是输出到系统日志中
            var keyword = Guid.NewGuid().ToString();//随机字符串
            SenparcTrace.Log($"添加Log：{keyword}");
            //Assert.IsTrue(UnitTestHelper.CheckKeywordsExist(_logFilePath,keyword));
        }


        [TestMethod]
        public void SendCustomLogTest()
        {
            var keyword = Guid.NewGuid().ToString();//随机字符串
            SenparcTrace.SendCustomLog("标题", $"添加Log：{keyword}");
            Assert.IsTrue(UnitTestHelper.CheckKeywordsExist(LogFilePath, "标题", keyword));
        }


        [TestMethod]
        public void SendApiLogTest()
        {
            var url = "http://www.senparc.com";
            var result = Guid.NewGuid().ToString();//随机字符串
            SenparcTrace.SendApiLog(url, result);
            Assert.IsTrue(UnitTestHelper.CheckKeywordsExist(LogFilePath, url, result));
        }


        [TestMethod]
        public void SendApiPostDataLogTest()
        {
            var url = "http://www.senparc.com";
            var data = Guid.NewGuid().ToString();//随机字符串
            SenparcTrace.SendApiLog(url, data);
            Assert.IsTrue(UnitTestHelper.CheckKeywordsExist(LogFilePath, url, data));
        }



        [TestMethod]
        public void BaseExceptionLogTest()
        {
            var keyword = Guid.NewGuid().ToString();//随机字符串
            var ex = new BaseException("测试异常：" + keyword);
            //Log会记录两次，第一次是在BaseException初始化的时候会调用此方法
            SenparcTrace.BaseExceptionLog(ex);
            Assert.IsTrue(UnitTestHelper.CheckKeywordsExist(LogFilePath, "测试异常", keyword));
        }

        [TestMethod]
        public void OnLogFuncTest()
        {
            var onlogCount = 0;
            SenparcTrace.OnLogFunc = () => onlogCount++;

            var keyword = Guid.NewGuid().ToString();//随机字符串
            SenparcTrace.SendCustomLog("测试OnLogFuncTest", keyword);
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
            Assert.IsTrue(UnitTestHelper.CheckKeywordsExist(LogFilePath, keyword));
            Assert.AreEqual(2, onlogCount);
        }

    }
}
