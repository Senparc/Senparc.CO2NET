using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Senparc.CO2NET.Helpers;

namespace Senparc.CO2NET.Tests.Helpers
{
    [TestClass]
    public class FileHelperTests
    {
        [TestMethod]
        public void GetFileStreamTest()
        {
            var fileName = "..\\..\\..\\TestEntities\\Logo.jpg";
            var stream = FileHelper.GetFileStream(fileName);
            Assert.IsNotNull(stream);
            Assert.IsTrue(stream.Length > 0);
            Console.WriteLine(stream.Length);
            Assert.AreEqual(117025, stream.Length);//只对当前Logo.jpg有效
        }
    }
}
