using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Senparc.CO2NET.Helpers;

namespace Senparc.CO2NET.Tests.Helpers
{
    [TestClass]
    public class ASCIISortTests
    {
        [TestMethod]
        public void ASCIISortTest()
        {
            string[] scoure = new[] { "1", "2", "A", "a", "B", "b" };
            scoure.ToList().OrderBy(x => x); //结果 1 2 a A b B
            Console.WriteLine("使用OrderBy排序：");
            foreach (var item in scoure)
            {
                Console.WriteLine(item);
            }
            Assert.AreEqual("1", scoure[0]);
            Assert.AreEqual("2", scoure[1]);
            Assert.AreEqual("A", scoure[2]);
            Assert.AreEqual("a", scoure[3]);//注意这里不同
            Assert.AreEqual("B", scoure[4]);//注意这里不同
            Assert.AreEqual("b", scoure[5]);

            ArrayList arrSource = new ArrayList(new[] { "1", "2", "A", "a", "B", "b" });
            arrSource.Sort(ASCIISort.Create());
            Console.WriteLine("使用ASCIISort排序：");
            foreach (var item in arrSource)
            {
                Console.WriteLine(item);
            }
            Assert.AreEqual("1", arrSource[0]);
            Assert.AreEqual("2", arrSource[1]);
            Assert.AreEqual("A", arrSource[2]);
            Assert.AreEqual("B", arrSource[3]);//注意这里不同
            Assert.AreEqual("a", arrSource[4]);//注意这里不同
            Assert.AreEqual("b", arrSource[5]);
        }

    }
}
