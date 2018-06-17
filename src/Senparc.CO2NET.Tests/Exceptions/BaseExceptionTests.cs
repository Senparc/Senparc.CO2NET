using Microsoft.VisualStudio.TestTools.UnitTesting;
using Senparc.CO2NET.Exceptions;
using Senparc.CO2NET.Tests.TestEntities;
using System;

namespace Senparc.CO2NET.Tests.Exceptions
{
    [TestClass]
    public class BaseExceptionTests
    {
        [TestMethod]
        public void BaseExceptionTest()
        {
            try
            {
                throw new TestException("异常测试", new Exception("内部异常"));
            }
            catch (TestException ex)
            {
                Assert.AreEqual("异常测试", ex.Message);
                Assert.AreEqual("内部异常", ex.InnerException.Message);

                //TODO：测试日记录
            }
            catch (BaseException ex)
            {
                Assert.Fail();
            }

        }
    }
}
