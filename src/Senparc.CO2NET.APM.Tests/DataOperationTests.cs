using Microsoft.VisualStudio.TestTools.UnitTesting;
using Senparc.CO2NET.Extensions;
using Senparc.CO2NET.Tests;
using System;
using Moq;
using System.Threading;
using System.Collections.Generic;

namespace Senparc.CO2NET.APM.Tests
{
    [TestClass]
    public class DataOperationTests : BaseTest
    {
        public const string domainPrefix = "CO2NET_Test_";

        public DataOperationTests()
        {
            Senparc.CO2NET.APM.Config.EnableAPM = true;
        }

        private void BuildTestData(DataOperation dataOperation)
        {
            dataOperation.SetAsync("Memory", 4567, dateTime: SystemTime.Now.AddDays(-1)).Wait();//A simple example
            dataOperation.SetAsync("Memory", 6789, dateTime: SystemTime.Now.AddMinutes(-2)).Wait();

            dataOperation.SetAsync("CPU", .65, dateTime: SystemTime.Now.AddMinutes(-2)).Wait();
            dataOperation.SetAsync("CPU", .78, dateTime: SystemTime.Now.AddMinutes(-2)).Wait();
            dataOperation.SetAsync("CPU", .75, dateTime: SystemTime.Now.AddMinutes(-2)).Wait();
            dataOperation.SetAsync("CPU", .92, dateTime: SystemTime.Now.AddMinutes(-1)).Wait();
            dataOperation.SetAsync("CPU", .48, dateTime: SystemTime.Now.AddMinutes(-1)).Wait();

            dataOperation.SetAsync("Accessor", 1, dateTime: SystemTime.Now.AddMinutes(-3)).Wait();
            dataOperation.SetAsync("Accessor", 1, dateTime: SystemTime.Now.AddMinutes(-3)).Wait();
            dataOperation.SetAsync("Accessor", 1, dateTime: SystemTime.Now.AddMinutes(-2)).Wait();
            dataOperation.SetAsync("Accessor", 1, dateTime: SystemTime.Now.AddMinutes(-2)).Wait();
            dataOperation.SetAsync("Accessor", 1, dateTime: SystemTime.Now.AddMinutes(-1)).Wait();
            dataOperation.SetAsync("Accessor", 1, dateTime: SystemTime.Now.AddMinutes(-1)).Wait();

            dataOperation.SetAsync("Accessor", 1, dateTime: SystemTime.Now).Wait();
        }

        [TestMethod]
        public void SetAndGetTest()
        {
            DataOperation dataOperation = new DataOperation(domainPrefix + "SetAndGetTest");
            BuildTestData(dataOperation);

            var memoryData = dataOperation.GetDataItemListAsync("Memory").Result;
            Assert.AreEqual(2, memoryData.Count);

            var cpuData = dataOperation.GetDataItemListAsync("CPU").Result;
            Assert.AreEqual(5, cpuData.Count);

            //var viewData = dataOperation.GetDataItemListAsync("Visitor Volume").Result;
            //Assert.AreEqual(7, viewData.Count);
        }


        [TestMethod]
        public void ReadAndCleanDataItemsTest()
        {
            DataOperation dataOperation = new DataOperation(domainPrefix + "ReadAndCleanDataItemsTest");
            BuildTestData(dataOperation);
            var result = dataOperation.ReadAndCleanDataItemsAsync(true, false).Result;//Processing the current task before the previous task

            Assert.IsNotNull(result);
            Assert.AreEqual(3, result.Count);//Memory and CPU resources, limited to 3 units
            Console.WriteLine(result.ToJson());
            Console.WriteLine("===============");

            //Check if the current task has received the previous task
            var memoryData = dataOperation.GetDataItemListAsync("内存").Result;
            Assert.AreEqual(0, memoryData.Count);

            var cpuData = dataOperation.GetDataItemListAsync("CPU").Result;
            Assert.AreEqual(0, cpuData.Count);

            //var viewData = dataOperation.GetDataItemListAsync("Visitor Volume").Result;
            //Assert.AreEqual(1, viewData.Count);//The current task will not be interrupted

            //Simulate current time

        }

        [TestMethod]
        public void ReadAndCleanDataItems_KeepTodayDataTest()
        {
            DataOperation dataOperation = new DataOperation(domainPrefix + "ReadAndCleanDataItems_KeepTodayDataTest");
            BuildTestData(dataOperation);
            var result = dataOperation.ReadAndCleanDataItemsAsync(true, true).Result;//Only record before the previous task

            Assert.IsNotNull(result);
            Assert.AreEqual(3, result.Count);//Memory and CPU resources, limited to 3 units
            Console.WriteLine(result.ToJson());
            Console.WriteLine("===============");

            //Check if the current task has received the previous task
            var memoryData = dataOperation.GetDataItemListAsync("Memory").Result;
            Assert.AreEqual(1, memoryData.Count);//Delete 1 element from the list

            var cpuData = dataOperation.GetDataItemListAsync("CPU").Result;
            Assert.AreEqual(5, cpuData.Count);//Clear all elements in the list

            //var viewData = dataOperation.GetDataItemListAsync("Visitor Volumn").Result;
            //Assert.AreEqual(7, viewData.Count);//Clear all elements in the list

            //Simulate current time

        }
    }
}
