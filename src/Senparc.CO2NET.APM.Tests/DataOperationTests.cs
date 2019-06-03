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
            dataOperation.SetAsync("�ڴ�", 4567, dateTime: SystemTime.Now.AddDays(-1)).Wait();//��һ�������
            dataOperation.SetAsync("�ڴ�", 6789, dateTime: SystemTime.Now.AddMinutes(-2)).Wait();

            dataOperation.SetAsync("CPU", .65, dateTime: SystemTime.Now.AddMinutes(-2)).Wait();
            dataOperation.SetAsync("CPU", .78, dateTime: SystemTime.Now.AddMinutes(-2)).Wait();
            dataOperation.SetAsync("CPU", .75, dateTime: SystemTime.Now.AddMinutes(-2)).Wait();
            dataOperation.SetAsync("CPU", .92, dateTime: SystemTime.Now.AddMinutes(-1)).Wait();
            dataOperation.SetAsync("CPU", .48, dateTime: SystemTime.Now.AddMinutes(-1)).Wait();

            dataOperation.SetAsync("������", 1, dateTime: SystemTime.Now.AddMinutes(-3)).Wait();
            dataOperation.SetAsync("������", 1, dateTime: SystemTime.Now.AddMinutes(-3)).Wait();
            dataOperation.SetAsync("������", 1, dateTime: SystemTime.Now.AddMinutes(-2)).Wait();
            dataOperation.SetAsync("������", 1, dateTime: SystemTime.Now.AddMinutes(-2)).Wait();
            dataOperation.SetAsync("������", 1, dateTime: SystemTime.Now.AddMinutes(-1)).Wait();
            dataOperation.SetAsync("������", 1, dateTime: SystemTime.Now.AddMinutes(-1)).Wait();

            dataOperation.SetAsync("������", 1, dateTime: SystemTime.Now);//��ǰ���ӣ��������ռ�


        }

        [TestMethod]
        public void SetAndGetTest()
        {
            DataOperation dataOperation = new DataOperation(domainPrefix + "SetAndGetTest");
            BuildTestData(dataOperation);

            var memoryData = dataOperation.GetDataItemListAsync("�ڴ�").Result;
            Assert.AreEqual(2, memoryData.Count);

            var cpuData = dataOperation.GetDataItemListAsync("CPU").Result;
            Assert.AreEqual(5, cpuData.Count);

            var viewData = dataOperation.GetDataItemListAsync("������").Result;
            Assert.AreEqual(7, viewData.Count);
        }


        [TestMethod]
        public void ReadAndCleanDataItemsTest()
        {
            DataOperation dataOperation = new DataOperation(domainPrefix + "ReadAndCleanDataItemsTest");
            BuildTestData(dataOperation);
            var result = dataOperation.ReadAndCleanDataItemsAsync(true, false).Result;//������е�ǰ����ǰ�Ĺ�������

            Assert.IsNotNull(result);
            Assert.AreEqual(3, result.Count);//�ڴ桢CPU��������3������
            Console.WriteLine(result.ToJson());
            Console.WriteLine("===============");

            //������ȡ������Ƿ��Ѿ���յ�ǰ����֮ǰ������
            var memoryData = dataOperation.GetDataItemListAsync("�ڴ�").Result;
            Assert.AreEqual(0, memoryData.Count);

            var cpuData = dataOperation.GetDataItemListAsync("CPU").Result;
            Assert.AreEqual(0, cpuData.Count);

            var viewData = dataOperation.GetDataItemListAsync("������").Result;
            Assert.AreEqual(1, viewData.Count);//��ǰ���ӵĻ��治�ᱻ���

            //ģ�⵱ǰʱ��

        }

        [TestMethod]
        public void ReadAndCleanDataItems_KeepTodayDataTest()
        {
            DataOperation dataOperation = new DataOperation(domainPrefix + "ReadAndCleanDataItems_KeepTodayDataTest");
            BuildTestData(dataOperation);
            var result = dataOperation.ReadAndCleanDataItemsAsync(true, true).Result;//ֻ�������֮ǰ�ļ�¼

            Assert.IsNotNull(result);
            Assert.AreEqual(3, result.Count);//�ڴ桢CPU��������3������
            Console.WriteLine(result.ToJson());
            Console.WriteLine("===============");

            //������ȡ������Ƿ��Ѿ���յ�ǰ����֮ǰ������
            var memoryData = dataOperation.GetDataItemListAsync("�ڴ�").Result;
            Assert.AreEqual(1, memoryData.Count);//ɾ��1�����������

            var cpuData = dataOperation.GetDataItemListAsync("CPU").Result;
            Assert.AreEqual(5, cpuData.Count);//��������ȫ������

            var viewData = dataOperation.GetDataItemListAsync("������").Result;
            Assert.AreEqual(7, viewData.Count);//��������ȫ������

            //ģ�⵱ǰʱ��

        }
    }
}
