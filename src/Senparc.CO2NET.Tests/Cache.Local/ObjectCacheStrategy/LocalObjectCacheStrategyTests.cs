using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Senparc.CO2NET.Cache;
using Senparc.CO2NET.Tests.TestEntities;
using System;

namespace Senparc.CO2NET.Tests.Cache.Local
{
    [TestClass]
    public class LocalObjectCacheStrategyTests :BaseTest
    {
        public LocalObjectCacheStrategyTests()
        {
            //BaseTest.RegisterServiceCollection();
        }

        [TestMethod]
        public void SingletonTest()
        {
            var cache1 = LocalObjectCacheStrategy.Instance;
            var cache2 = LocalObjectCacheStrategy.Instance;

            Assert.AreEqual(cache1.GetHashCode(), cache2.GetHashCode());//单例，实例相同
        }

        [TestMethod]
        public void CacheLockTest()
        {
            var cache = LocalObjectCacheStrategy.Instance;
            using (var cacheLock = cache.BeginCacheLock("SenparcTest", "CacheLockTest", 1, TimeSpan.FromMilliseconds(10)))
            {
                //查找内存中的对象


            }
        }

        /// <summary>
        /// 所有 BaseObjectCacheStrategy 接口测试
        /// </summary>
        [TestMethod]
        public void InterfaceTest()
        {
            BaseTest.RegisterServiceCollection();

            var cache = LocalObjectCacheStrategy.Instance;
            var key = "LocalObjectCacheStrategyInterfaceTest";
            var value = SystemTime.Now.ToString();

            //Set
            cache.Set(key, value);

            //Get
            var getResult = cache.Get(key);
            Assert.AreEqual(value, getResult);

            //Get<T>
            var objKey = "LocalObjectCacheStrategyInterfaceTestObjKey";
            var objValue = new TestCustomObject();//定义复杂类型
            cache.Set(objKey, objValue);
            var getObjResult = cache.Get<TestCustomObject>(objKey);
            Assert.IsInstanceOfType(objValue, typeof(TestCustomObject));
            Assert.AreEqual(objValue.GetHashCode(), getObjResult.GetHashCode());//不同的策略可能会不同

            Assert.AreEqual(objValue.Id, getObjResult.Id);
            Assert.AreEqual(objValue.Name, getObjResult.Name);
            Assert.AreEqual(objValue.AddTime, getObjResult.AddTime);

            //GetAll
            var allObjects = cache.GetAll();
            Assert.IsTrue(allObjects.Count > 0);

            Console.WriteLine($"GetAll：");
            foreach (var item in allObjects)
            {
                Console.WriteLine($"Key：{item.Key}，Value：{item.Value}");
            }

            //CheckExisted
            Assert.IsTrue(cache.CheckExisted(key));
            Assert.IsTrue(cache.CheckExisted(objKey));
            Assert.IsFalse(cache.CheckExisted(key + objKey));

            //GetCount
            var count = cache.GetCount();
            Assert.AreEqual(allObjects.Count, count);

            //Update
            objValue.Id = 666;
            objValue.Name = "NewDomainName";
            objValue.AddTime = SystemTime.Now;

            cache.Update(objKey, objValue);
            var updatedRessult = cache.Get<TestCustomObject>(objKey);
            Assert.AreEqual(objValue.GetHashCode(), updatedRessult.GetHashCode());//不同的策略可能会不同
            Assert.AreEqual(objValue.Id, updatedRessult.Id);
            Assert.AreEqual(objValue.Name, updatedRessult.Name);
            Assert.AreEqual(objValue.AddTime, updatedRessult.AddTime);

            //Remove
            cache.RemoveFromCache(key);
            getResult = cache.Get(key);
            Assert.IsNull(getResult);

            cache.RemoveFromCache(objKey);
            var removedRessult = cache.Get<TestCustomObject>(objKey);
            Assert.IsNull(removedRessult);

            var newCount = cache.GetCount();
            Assert.AreEqual(count - 2, newCount);//移除两项的计数
        }
    }
}
