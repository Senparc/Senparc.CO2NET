using Microsoft.VisualStudio.TestTools.UnitTesting;
using Senparc.CO2NET.Cache;
using Senparc.CO2NET.Cache.Redis;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;

namespace Senparc.CO2NET.Tests.Cache
{
    [TestClass]
    public class CacheSerializeExtensionTest
    {
        public class TestClass
        {
            public string ID { get; set; }
            public long Star { get; set; }
            public DateTime AddTime { get; set; }
            public Type Type { get; set; }
        }

        [TestMethod]
        public void CacheWrapperTest()
        {
            var testClass = new TestClass()
            {
                ID = Guid.NewGuid().ToString(),
                Star = SystemTime.Now.DateTime.Ticks,
                AddTime = SystemTime.Now.DateTime
            };

            var json = testClass.SerializeToCache();
            Console.WriteLine(json);

            var obj = json.DeserializeFromCache<TestClass>();
            Assert.AreEqual(obj.ID, testClass.ID);
        }

        [TestMethod]
        public void CacheWrapperEfficiencyTest()
        {
            Console.WriteLine("��ʼCacheWrapper�첽����");
            var threadCount = 10;
            var finishCount = 0;
            List<Thread> threadList = new List<Thread>();
            for (int i = 0; i < threadCount; i++)
            {
                var thread = new Thread(() =>
                {

                    var testClass = new TestClass()
                    {
                        ID = Guid.NewGuid().ToString(),
                        Star = SystemTime.Now.DateTime.Ticks,
                        AddTime = SystemTime.Now.DateTime
                    };

                    var dtx = SystemTime.Now.DateTime;
                    var json = testClass.SerializeToCache();
                    //Console.WriteLine(json);
                    Console.WriteLine($"testClass.SerializeToCache ��ʱ��{SystemTime.DiffTotalMS(dtx)}ms");


                    dtx = SystemTime.Now.DateTime;
                    var obj = json.DeserializeFromCache<TestClass>();
                    Console.WriteLine($"json.DeserializeFromCache<TestClass> ��ʱ��{SystemTime.DiffTotalMS(dtx)}ms");
                    Assert.AreEqual(obj.ID, testClass.ID);
                    Assert.AreEqual(obj.Star, testClass.Star);
                    Assert.AreEqual(obj.AddTime, testClass.AddTime);

                    Console.WriteLine("");

                    finishCount++;
                });
                threadList.Add(thread);
            }

            threadList.ForEach(z => z.Start());

            while (finishCount < threadCount)
            {
                //Waiting
            }

        }

        [TestMethod]
        public void CacheWapper_VS_BinaryTest()
        {
            var count = 50000;
            var dt1 = SystemTime.Now.DateTime;
            for (int i = 0; i < count; i++)
            {
                var testClass = new TestClass()
                {
                    ID = Guid.NewGuid().ToString(),
                    Star = SystemTime.Now.DateTime.Ticks,
                    AddTime = SystemTime.Now.DateTime
                };

                var dtx = SystemTime.Now.DateTime;
                var json = testClass.SerializeToCache();
                //Console.WriteLine(json);
                //Console.WriteLine($"testClass.SerializeToCache took {SystemTime.DiffTotalMS(dtx)}ms");

                dtx = SystemTime.Now.DateTime;
                var obj = json.DeserializeFromCache<TestClass>();
                //Console.WriteLine($"json.DeserializeFromCache<TestClass> took {SystemTime.DiffTotalMS(dtx)}ms");
                Assert.AreEqual(obj.ID, testClass.ID);
                Assert.AreEqual(obj.Star, testClass.Star);
                Assert.AreEqual(obj.AddTime, testClass.AddTime);

            }
            var dt2 = SystemTime.Now.DateTime;
            Console.WriteLine($"CacheWrapper���л� {count} �Σ�ʱ�䣺{(dt2 - dt1).TotalMilliseconds}ms");

            dt1 = SystemTime.Now.DateTime;
            for (int i = 0; i < count; i++)
            {
                var testClass = new TestClass()
                {
                    ID = Guid.NewGuid().ToString(),
                    Star = SystemTime.Now.DateTime.Ticks,
                    AddTime = SystemTime.Now.DateTime
                };

                var dtx = SystemTime.Now.DateTime;
                var serializedObj = StackExchangeRedisExtensions.Serialize(testClass);
                //Note: This issue seems to be related to DateTimeOffset being converted to Object, which cannot be deserialized back to the original type

                //Console.WriteLine($"StackExchangeRedisExtensions.Serialize took {SystemTime.DiffTotalMS(dtx)}ms");

                dtx = SystemTime.Now.DateTime;
                var containerBag = StackExchangeRedisExtensions.Deserialize<TestClass>((RedisValue)serializedObj);//11ms
                //Console.WriteLine($"StackExchangeRedisExtensions.Deserialize took {SystemTime.DiffTotalMS(dtx)}ms");

                Assert.AreEqual(containerBag.AddTime.Ticks, testClass.AddTime.Ticks);
                Assert.AreNotEqual(containerBag.GetHashCode(), testClass.GetHashCode());

            }
            dt2 = SystemTime.Now.DateTime;
            Console.WriteLine($"StackExchangeRedisExtensions���л� {count} �Σ�ʱ�䣺{(dt2 - dt1).TotalMilliseconds}ms");


            dt1 = SystemTime.Now.DateTime;
            for (int i = 0; i < count; i++)
            {
                var testClass = new TestClass()
                {
                    ID = Guid.NewGuid().ToString(),
                    Star = SystemTime.Now.DateTime.Ticks,
                    AddTime = SystemTime.Now.DateTime,
                };

                //Simulating CacheWrapper Type issue to test performance; needs to ensure no performance degradation
                //testClass.Type = testClass.GetType();

                var dtx = SystemTime.Now.DateTime;
                var serializedObj = Newtonsoft.Json.JsonConvert.SerializeObject(testClass);
                //Console.WriteLine($"StackExchangeRedisExtensions.Serialize took {SystemTime.DiffTotalMS(dtx)}ms");

                dtx = SystemTime.Now.DateTime;
                var containerBag = Newtonsoft.Json.JsonConvert.DeserializeObject<TestClass>(serializedObj);//11ms
                                                                                                           //Console.WriteLine($"StackExchangeRedisExtensions.Deserialize took {SystemTime.DiffTotalMS(dtx)}ms");

                Assert.AreEqual(containerBag.AddTime.Ticks, testClass.AddTime.Ticks);
                Assert.AreNotEqual(containerBag.GetHashCode(), testClass.GetHashCode());

            }
            dt2 = SystemTime.Now.DateTime;
            Console.WriteLine($"Newtonsoft ���л����޷��䣩 {count} �Σ�ʱ�䣺{(dt2 - dt1).TotalMilliseconds}ms");


            dt1 = SystemTime.Now.DateTime;
            for (int i = 0; i < count; i++)
            {
                var testClass = new TestClass()
                {
                    ID = Guid.NewGuid().ToString(),
                    Star = SystemTime.Now.DateTime.Ticks,
                    AddTime = SystemTime.Now.DateTime,
                };

                //Simulating CacheWrapper Type issue to test performance; needs to ensure no performance degradation
                testClass.Type = testClass.GetType();

                var dtx = SystemTime.Now.DateTime;
                var serializedObj = Newtonsoft.Json.JsonConvert.SerializeObject(testClass);
                //Console.WriteLine($"StackExchangeRedisExtensions.Serialize took {SystemTime.DiffTotalMS(dtx)}ms");

                dtx = SystemTime.Now.DateTime;
                var containerBag = Newtonsoft.Json.JsonConvert.DeserializeObject<TestClass>(serializedObj);//11ms
                //Console.WriteLine($"StackExchangeRedisExtensions.Deserialize took {SystemTime.DiffTotalMS(dtx)}ms");

                Assert.AreEqual(containerBag.AddTime.Ticks, testClass.AddTime.Ticks);
                Assert.AreNotEqual(containerBag.GetHashCode(), testClass.GetHashCode());

            }
            dt2 = SystemTime.Now.DateTime;
            Console.WriteLine($"Newtonsoft ���л�+���� {count} �Σ�ʱ�䣺{(dt2 - dt1).TotalMilliseconds}ms");


            dt1 = SystemTime.Now.DateTime;
            for (int i = 0; i < count; i++)
            {
                var testClass = new TestClass()
                {
                    ID = Guid.NewGuid().ToString(),
                    Star = SystemTime.Now.DateTime.Ticks,
                    AddTime = SystemTime.Now.DateTime,
                };

                //Simulating CacheWrapper Type issue to test performance; needs to ensure no performance degradation
                Expression<Func<TestClass>> fun = () => testClass;
                //Console.WriteLine(fun.Body.Type);

                testClass.Type = fun.Body.Type;

                var dtx = SystemTime.Now.DateTime;
                var serializedObj = Newtonsoft.Json.JsonConvert.SerializeObject(testClass);
                //Console.WriteLine($"StackExchangeRedisExtensions.Serialize took {SystemTime.DiffTotalMS(dtx)}ms");

                dtx = SystemTime.Now.DateTime;
                var containerBag = Newtonsoft.Json.JsonConvert.DeserializeObject<TestClass>(serializedObj);//11ms
                                                                                                           //Console.WriteLine($"StackExchangeRedisExtensions.Deserialize took {SystemTime.DiffTotalMS(dtx)}ms");
                Assert.AreEqual(typeof(TestClass), containerBag.Type);
                Assert.AreEqual(containerBag.AddTime.Ticks, testClass.AddTime.Ticks);
                Assert.AreNotEqual(containerBag.GetHashCode(), testClass.GetHashCode());

            }
            dt2 = SystemTime.Now.DateTime;
            Console.WriteLine($"Newtonsoft ���л���Lambda�� {count} �Σ�ʱ�䣺{(dt2 - dt1).TotalMilliseconds}ms");

        }

    }
}
