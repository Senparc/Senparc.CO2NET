using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Senparc.CO2NET.WebApi.WebApiEngines;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Senparc.CO2NET.WebApi.Tests.WebApiEngines
{
    [TestClass()]
    public class BaseApiControllerTypeTests : BaseTest
    {
        [TestMethod]
        public void BaseTypeTest()
        {
            base.Init();//初始化

            var appDataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "App_Data");// ServerUtility.ContentRootMapPath("~/App_Data");
            base.ServiceCollection.AddAndInitDynamicApi(MvcCoreBuilder, options =>
            {
                options.DocXmlPath = appDataPath; 
                options.TaskCount = 400;
            });

            {
                var assembly = WebApiEngine.ApiAssemblyCollection.First(z => z.Key == "BaseApiControllerTypeTest").Value;
                var classType = assembly.GetType("BaseApiControllerTypeTestController");
                Assert.IsNotNull(classType);

                //选用Order最大的基类
                Assert.IsTrue(classType.IsSubclassOf(typeof(MyBaseController2)));
            }

            {
                var assembly = WebApiEngine.ApiAssemblyCollection.First(z => z.Key == "BaseApiControllerTypeTest_NoBase").Value;
                var classType = assembly.GetType("BaseApiControllerTypeTest_NoBaseController");
                Assert.IsNotNull(classType);

                //默认使用 ControllerBase
                Assert.IsTrue(classType.IsSubclassOf(typeof(ControllerBase)));
            }
        }
    }

    public class MyBaseController1 : ControllerBase
    {
    }

    public class MyBaseController2 : ControllerBase
    {
    }

    public class MyBaseController3 : ControllerBase
    {
    }

    [ApiBind("BaseApiControllerTypeTest", BaseApiControllerType = typeof(MyBaseController1), BaseApiControllerOrder = 1)]
    public class TestApiClass1
    {
        public string Method1()
        {
            return "TestApiClass1.Method1";
        }

        public string Method2()
        {
            return "TestApiClass1.Method2";
        }
    }

    [ApiBind("BaseApiControllerTypeTest", BaseApiControllerType = typeof(MyBaseController2), BaseApiControllerOrder = 10/*数字最大，MyBaseController2 会被选用*/)]
    public class TestApiClass2
    {
        public string Method1()
        {
            return "TestApiClass2.Method1";
        }

        public string Method2()
        {
            return "TestApiClass2.Method2";
        }
    }

    [ApiBind("BaseApiControllerTypeTest", BaseApiControllerType = typeof(MyBaseController3), BaseApiControllerOrder = 3)]
    public class TestApiClass3
    {
        public string Method1()
        {
            return "TestApiClass3.Method1";
        }

        public string Method2()
        {
            return "TestApiClass3.Method2";
        }
    }


    /// <summary>
    /// 不定义任何基类
    /// </summary>
    [ApiBind("BaseApiControllerTypeTest_NoBase")]
    public class TestApiClass4
    {
        public string Method1()
        {
            return "TestApiClass3.Method1";
        }

        public string Method2()
        {
            return "TestApiClass3.Method2";
        }
    }
}
