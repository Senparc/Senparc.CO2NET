using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Senparc.CO2NET.MessageQueue;
using Senparc.CO2NET.Threads;

namespace Senparc.CO2NET.Tests.MessageQueue
{
    [TestClass]
    public class SenparcMessageQueueTests
    {
        [TestMethod]
        public void SenparcMessageQueueTest()
        {


            var smq = new SenparcMessageQueue();
            var keyPrefix = "TestMQ_";
            var count = smq.GetCount();

            for (int i = 0; i < 3; i++)
            {
                var key = keyPrefix + i;
                //测试Add
                smq.Add(key, () =>
                  {
                      Console.WriteLine("执行队列：" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffffff"));
                  });


                Console.WriteLine("添加队列项：" + key);
                Console.WriteLine("当前数量：" + smq.GetCount());
                Console.WriteLine("CurrentKey：" + smq.GetCurrentKey());
                Assert.AreEqual(count + 1, smq.GetCount());
                count = smq.GetCount();

                //测试GetItem
                var item = smq.GetItem(key);
                Console.WriteLine("item.AddTime：" + item.AddTime);
                Assert.AreEqual(key, item.Key);

            }

            //测试Remove
            smq.Add("ToRemove", () =>
            {
                Console.WriteLine("如果看到这一条，说明没有清楚成功");
            });
            smq.Remove("ToRemove");

            //启动线程
            ThreadUtility.Register();

            while (smq.GetCount() > 0)
            {
                //等待队列处理完
            }

            Console.WriteLine("队列处理完毕，当前队列数量：" + smq.GetCount());
        }

        [TestMethod]
        public void TestAll()
        {
            var mq = new SenparcMessageQueue();
            var count = mq.GetCount();
            var key = DateTime.Now.Ticks.ToString();

            //Test Add()
            var item = mq.Add(key, () => Console.WriteLine("测试SenparcMessageQueue写入Key=A"));
            Assert.AreEqual(count + 1, mq.GetCount());
            //var hashCode = item.GetHashCode();

            //Test GetCurrentKey()
            var currentKey = mq.GetCurrentKey();
            Assert.AreEqual(key, currentKey);

            //Test GetItem
            var currentItem = mq.GetItem(currentKey);
            Assert.AreEqual(currentItem.Key, item.Key);
            Assert.AreEqual(currentItem.AddTime, item.AddTime);

            //Test Remove
            mq.Remove(key);
            Assert.AreEqual(count, mq.GetCount());
        }
    }
}
