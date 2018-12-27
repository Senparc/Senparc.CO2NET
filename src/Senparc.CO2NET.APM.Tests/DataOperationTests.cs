using Microsoft.VisualStudio.TestTools.UnitTesting;
using Senparc.CO2NET.Extensions;
using Senparc.CO2NET.Tests;
using System;
using Moq;

namespace Senparc.CO2NET.APM.Tests
{
    [TestClass]
    public class DataOperationTests : BaseTest
    {
    public    const string domainPrefix = "CO2NET_Test_";

        public DataOperationTests()
        {

        }

        private void BuildTestData(DataOperation dataOperation)
        {
            dataOperation.Set("CPU", .65, dateTime: SystemTime.Now.AddMinutes(-2));
            dataOperation.Set("CPU", .78, dateTime: SystemTime.Now.AddMinutes(-2));
            dataOperation.Set("CPU", .75, dateTime: SystemTime.Now.AddMinutes(-2));
            dataOperation.Set("CPU", .92, dateTime: SystemTime.Now.AddMinutes(-1));
            dataOperation.Set("CPU", .48, dateTime: SystemTime.Now.AddMinutes(-1));

            dataOperation.Set("访问量", 1, dateTime: SystemTime.Now.AddMinutes(-3));
            dataOperation.Set("访问量", 1, dateTime: SystemTime.Now.AddMinutes(-3));
            dataOperation.Set("访问量", 1, dateTime: SystemTime.Now.AddMinutes(-2));
            dataOperation.Set("访问量", 1, dateTime: SystemTime.Now.AddMinutes(-2));
            dataOperation.Set("访问量", 1, dateTime: SystemTime.Now.AddMinutes(-1));
            dataOperation.Set("访问量", 1, dateTime: SystemTime.Now.AddMinutes(-1));

            dataOperation.Set("访问量", 1, dateTime: SystemTime.Now);//当前分钟，将不被收集

        }

        [TestMethod]
        public void SetAndGetTest()
        {
            DataOperation dataOperation = new DataOperation(domainPrefix+ "SetAndGetTest");
            BuildTestData(dataOperation);

            var cpuData = dataOperation.GetDataItemList("CPU");
            Assert.AreEqual(5, cpuData.Count);

            var viewData = dataOperation.GetDataItemList("访问量");
            Assert.AreEqual(7, viewData.Count);
        }

     
        [TestMethod]
        public void ReadAndCleanDataItemsTest()
        {
            DataOperation dataOperation = new DataOperation(domainPrefix + "ReadAndCleanDataItemsTest");
            BuildTestData(dataOperation);
            var result = dataOperation.ReadAndCleanDataItems();

            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count);//CPU、访问量两个分类
            Console.WriteLine(result.ToJson());
            Console.WriteLine("===============");

            //立即获取，检查是否已经清空当前分钟之前的数据
            var cpuData = dataOperation.GetDataItemList("CPU");
            Assert.AreEqual(0, cpuData.Count);

            var viewData = dataOperation.GetDataItemList("访问量");
            Assert.AreEqual(1, viewData.Count);//当前分钟的缓存不会被清除

            //模拟当前时间

        }
    }
}
