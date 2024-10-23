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
                throw new TestException("Exception Testng", new Exception("Inner Exception"));
            }
            catch (TestException ex)
            {
                Assert.AreEqual("Exception Testng", ex.Message);
                Assert.AreEqual("Inner Exception", ex.InnerException.Message);

                //TODO: Fill in the record
            }
            catch (BaseException ex)
            {
                Assert.Fail();
            }

        }
    }
}
