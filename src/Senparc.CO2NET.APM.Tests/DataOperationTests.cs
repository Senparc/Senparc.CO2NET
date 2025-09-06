using Microsoft.VisualStudio.TestTools.UnitTesting;
using Senparc.CO2NET.Extensions;
using Senparc.CO2NET.Tests;
using System;
using Moq;
using System.Threading;
using System.Collections.Generic;
using System.Threading.Tasks;

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

        private async Task BuildTestDataAsync(DataOperation dataOperation)
        {
           _= dataOperation.SetAsync("Memory", 4567, dateTime: SystemTime.Now.AddDays(-1));//A simple example
           _= dataOperation.SetAsync("Memory", 6789, dateTime: SystemTime.Now.AddMinutes(-2));
          
           _= dataOperation.SetAsync("CPU", .65, dateTime: SystemTime.Now.AddMinutes(-2));
           _= dataOperation.SetAsync("CPU", .78, dateTime: SystemTime.Now.AddMinutes(-2));
           _= dataOperation.SetAsync("CPU", .75, dateTime: SystemTime.Now.AddMinutes(-2));
           _= dataOperation.SetAsync("CPU", .92, dateTime: SystemTime.Now.AddMinutes(-1));
           _= dataOperation.SetAsync("CPU", .48, dateTime: SystemTime.Now.AddMinutes(-1));
        
           _= dataOperation.SetAsync("Accessor", 1, dateTime: SystemTime.Now.AddMinutes(-3));
           _= dataOperation.SetAsync("Accessor", 1, dateTime: SystemTime.Now.AddMinutes(-3));
           _= dataOperation.SetAsync("Accessor", 1, dateTime: SystemTime.Now.AddMinutes(-2));
           _= dataOperation.SetAsync("Accessor", 1, dateTime: SystemTime.Now.AddMinutes(-2));
           _= dataOperation.SetAsync("Accessor", 1, dateTime: SystemTime.Now.AddMinutes(-1));
           _= dataOperation.SetAsync("Accessor", 1, dateTime: SystemTime.Now.AddMinutes(-1));
          
           _= dataOperation.SetAsync("Accessor", 1, dateTime: SystemTime.Now);
          
            Thread.Sleep(1000); // wait for all items be cached
        }

        [TestMethod]
        public async Task SetAndGetTest()
        {
            DataOperation dataOperation = new DataOperation(domainPrefix + "SetAndGetTest");
            await BuildTestDataAsync(dataOperation);

            var memoryData = await dataOperation.GetDataItemListAsync("Memory");
            Assert.AreEqual(2, memoryData.Count);

            var cpuData = await dataOperation.GetDataItemListAsync("CPU");
            Assert.AreEqual(5, cpuData.Count);

            //var viewData = dataOperation.GetDataItemListAsync("Visitor Volume").Result;
            //Assert.AreEqual(7, viewData.Count);
        }


        [TestMethod]
        public async Task ReadAndCleanDataItemsTest()
        {
            DataOperation dataOperation = new DataOperation(domainPrefix + "ReadAndCleanDataItemsTest");
            await BuildTestDataAsync(dataOperation);
            var result = await dataOperation.ReadAndCleanDataItemsAsync(true, false);//Processing the current task before the previous task

            Assert.IsNotNull(result);
            Assert.AreEqual(3, result.Count);//Memory and CPU resources, limited to 3 units
            Console.WriteLine(result.ToJson());
            Console.WriteLine("===============");

            //Check if the current task has received the previous task
            var memoryData = await dataOperation.GetDataItemListAsync("内存");
            Assert.AreEqual(0, memoryData.Count);

            var cpuData = await dataOperation.GetDataItemListAsync("CPU");
            Assert.AreEqual(0, cpuData.Count);

            //var viewData = dataOperation.GetDataItemListAsync("Visitor Volume").Result;
            //Assert.AreEqual(1, viewData.Count);//The current task will not be interrupted

            //Simulate current time

        }

        [TestMethod]
        public async Task ReadAndCleanDataItems_KeepTodayDataTest()
        {
            DataOperation dataOperation = new DataOperation(domainPrefix + "ReadAndCleanDataItems_KeepTodayDataTest");
            await BuildTestDataAsync(dataOperation);
            var result = await dataOperation.ReadAndCleanDataItemsAsync(true, true);//Only record before the previous task

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
