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
        private string _rootPath => Path.GetFullPath("..\\..\\..\\");

        private string _logFilePath => Path.Combine(_rootPath, "App_Data", "SenparcTraceLog", $"SenparcTrace-{DateTime.Now.ToString("yyyyMMdd")}.log");

        private bool CheckLog(params string[] keywords)
        {
            using (var fs = new FileStream(_logFilePath, FileMode.Open))
            {
                using (var sr = new StreamReader(fs))
                {
                    var content = sr.ReadToEnd();
                    foreach (var item in keywords)
                    {
                        if (!content.Contains(item))
                        {
                            return false;
                        }
                    }
                    return true;
                }
            }
        }

        public SenparcTraceTests()
        {
            //注册
            var mockEnv = new Mock<IHostingEnvironment>();
            mockEnv.Setup(z => z.ContentRootPath).Returns(() => Path.GetFullPath("..\\..\\..\\"));
            RegisterService.Start(mockEnv.Object, true);

            //删除日志文件
            //File.Delete(_logFilePath);
        }


        [TestMethod]
        public void LogTest()
        {
            //直接调用此方法不会记录到log文件中，而是输出到系统日志中
            var keyword = Guid.NewGuid().ToString();//随机字符串
            SenparcTrace.Log($"添加Log：{keyword}");
            //Assert.IsTrue(CheckLog(keyword));
        }


        [TestMethod]
        public void SendCustomLogTest()
        {
            var keyword = Guid.NewGuid().ToString();//随机字符串
            SenparcTrace.SendCustomLog("标题", $"添加Log：{keyword}");
            Assert.IsTrue(CheckLog("标题", keyword));
        }


        [TestMethod]
        public void SendApiLogTest()
        {
            var url = "http://www.senparc.com";
            var result = Guid.NewGuid().ToString();//随机字符串
            SenparcTrace.SendApiLog(url,result);
            Assert.IsTrue(CheckLog(url,result));
        }


        [TestMethod]
        public void SendApiPostDataLogTest()
        {
            var url = "http://www.senparc.com";
            var data = Guid.NewGuid().ToString();//随机字符串
            SenparcTrace.SendApiLog(url, data);
            Assert.IsTrue(CheckLog(url, data));
        }



        [TestMethod]
        public void BaseExceptionLogTest()
        {
            var keyword = Guid.NewGuid().ToString();//随机字符串
            var ex = new BaseException("测试异常："+ keyword);
            //Log会记录两次，第一次是在BaseException初始化的时候会调用此方法
            SenparcTrace.BaseExceptionLog(ex);
            Assert.IsTrue(CheckLog("测试异常", keyword));
        }

        [TestMethod]
        public void OnLogFuncTest()
        {
            var onlogCount = 0;
            SenparcTrace.OnLogFunc = () => onlogCount++;

            var keyword = Guid.NewGuid().ToString();//随机字符串
            SenparcTrace.SendCustomLog("测试OnLogFuncTest", keyword);
            Assert.IsTrue(CheckLog(keyword));
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
            Assert.IsTrue(CheckLog(keyword));
            Assert.AreEqual(2, onlogCount);
        }

    }
}
