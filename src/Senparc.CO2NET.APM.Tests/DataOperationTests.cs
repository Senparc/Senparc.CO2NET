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
        public const string domainPrefix = "CO2NET_Test_";

        public DataOperationTests()
        {

        }

        private void BuildTestData(DataOperation dataOperation)
        {
            dataOperation.Set("内存", 4567, dateTime: SystemTime.Now.AddDays(-1));//上一天的数据
            dataOperation.Set("内存", 6789, dateTime: SystemTime.Now.AddMinutes(-2));

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
            DataOperation dataOperation = new DataOperation(domainPrefix + "SetAndGetTest");
            BuildTestData(dataOperation);

            var memoryData = dataOperation.GetDataItemList("内存");
            Assert.AreEqual(2, memoryData.Count);

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
            var result = dataOperation.ReadAndCleanDataItems(true, false);//清除所有当前分钟前的过期数据

            Assert.IsNotNull(result);
            Assert.AreEqual(3, result.Count);//内存、CPU、访问量3个分类
            Console.WriteLine(result.ToJson());
            Console.WriteLine("===============");

            //立即获取，检查是否已经清空当前分钟之前的数据
            var memoryData = dataOperation.GetDataItemList("内存");
            Assert.AreEqual(0, memoryData.Count);

            var cpuData = dataOperation.GetDataItemList("CPU");
            Assert.AreEqual(0, cpuData.Count);

            var viewData = dataOperation.GetDataItemList("访问量");
            Assert.AreEqual(1, viewData.Count);//当前分钟的缓存不会被清除

            //模拟当前时间

        }

        [TestMethod]
        public void ReadAndCleanDataItems_KeepTodayDataTest()
        {
            DataOperation dataOperation = new DataOperation(domainPrefix + "ReadAndCleanDataItems_KeepTodayDataTest");
            BuildTestData(dataOperation);
            var result = dataOperation.ReadAndCleanDataItems(true, true);//只清除今天之前的记录

            Assert.IsNotNull(result);
            Assert.AreEqual(3, result.Count);//内存、CPU、访问量3个分类
            Console.WriteLine(result.ToJson());
            Console.WriteLine("===============");

            //立即获取，检查是否已经清空当前分钟之前的数据
            var memoryData = dataOperation.GetDataItemList("内存");
            Assert.AreEqual(1, memoryData.Count);//删除1条昨天的数据

            var cpuData = dataOperation.GetDataItemList("CPU");
            Assert.AreEqual(5, cpuData.Count);//当天数据全部保留

            var viewData = dataOperation.GetDataItemList("访问量");
            Assert.AreEqual(7, viewData.Count);//当天数据全部保留

            //模拟当前时间

        }
    }
}
