#region Apache License Version 2.0
/*----------------------------------------------------------------

Copyright 2025 Suzhou Senparc Network Technology Co.,Ltd.

Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file
except in compliance with the License. You may obtain a copy of the License at

http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software distributed under the
License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND,
either express or implied. See the License for the specific language governing permissions
and limitations under the License.

Detail: https://github.com/Senparc/Senparc.CO2NET/blob/master/LICENSE

----------------------------------------------------------------*/
#endregion Apache License Version 2.0

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Senparc.CO2NET.Cache.Redis;
using Senparc.CO2NET.RegisterServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Senparc.CO2NET.Cache.Tests
{
    [TestClass()]
    public class CacheStrategyFactoryTests
    {
        public CacheStrategyFactoryTests()
        {
            //Register
            var serviceCollection = new ServiceCollection();
            var configBuilder = new ConfigurationBuilder();
            var config = configBuilder.Build();
            serviceCollection.AddSenparcGlobalServices(config);
        }

        [TestMethod()]
        public void RegisterObjectCacheStrategyTest()
        {
            //Console.WriteLine("Do not register");


            {
                //Do not register, use default

                //Restore default cache state
                CacheStrategyFactory.RegisterObjectCacheStrategy(null);

                var cache = CacheStrategyFactory.GetObjectCacheStrategyInstance();
                //Default is local cache
                Assert.IsInstanceOfType(cache, typeof(LocalObjectCacheStrategy));
            }

            {
                //Register specified cache

                CacheStrategyFactory.RegisterObjectCacheStrategy(() => RedisObjectCacheStrategy.Instance);

                var cache = CacheStrategyFactory.GetObjectCacheStrategyInstance();
                Assert.IsInstanceOfType(cache, typeof(RedisObjectCacheStrategy));

                CacheStrategyFactory.RegisterObjectCacheStrategy(() => LocalObjectCacheStrategy.Instance);
                cache = CacheStrategyFactory.GetObjectCacheStrategyInstance();
                Assert.IsInstanceOfType(cache, typeof(LocalObjectCacheStrategy));

                CacheStrategyFactory.RegisterObjectCacheStrategy(null);
                cache = CacheStrategyFactory.GetObjectCacheStrategyInstance();
                Assert.IsInstanceOfType(cache, typeof(LocalObjectCacheStrategy));
            }

            //{
            //    //Do not register, use default
            //    var c1 = TestContainer1.GetCollectionList();
            //    Console.WriteLine(c1.Count);//0
            //    var c1Strategy = CacheStrategyFactory.GetContainerCacheStrategyInstance();
            //    Assert.IsNotNull(c1Strategy);

            //    var key = typeof(TestContainer1).ToString();
            //    var data = c1Strategy.Get(key);
            //    Assert.IsNotNull(data);

            //    var newData = new ContainerItemCollection();
            //    newData["A"] = new TestContainerBag1();
            //    c1Strategy.InsertToCache(key, newData);
            //    data = c1Strategy.Get(key);
            //    Assert.AreEqual(1, data.GetCount());
            //    Console.WriteLine(data.GetCount());//1

            //    var collectionList = TestContainer1.GetCollectionList()[key];
            //    collectionList.InsertToCache("ABC", new TestContainerBag1());
            //    data = c1Strategy.Get(key);
            //    Assert.AreEqual(2, data.GetCount());
            //    Console.WriteLine(data.GetCount());//2
            //}
            //Console.WriteLine("Use registration");
            //{
            //    //Register
            //    CacheStrategyFactory.RegisterContainerCacheStrategy(() =>
            //    {
            //        return LocalContainerCacheStrategy.Instance as IContainerCacheStrategy;
            //    });

            //    var key = typeof(TestContainer2).ToString();

            //    var c2 = TestContainer2.GetCollectionList();
            //    Console.WriteLine(c2.Count);//1 (one TestContainer1 was already registered during registration)
            //    var c2Strategy = CacheStrategyFactory.GetContainerCacheStrategyInstance();
            //    Assert.IsNotNull(c2Strategy);


            //    var data = c2Strategy.Get(key);
            //    Assert.IsNotNull(data);

            //    var newData = new ContainerItemCollection();
            //    newData["A"] = new TestContainerBag1();
            //    c2Strategy.InsertToCache(key, newData);
            //    data = c2Strategy.Get(key);
            //    Assert.AreEqual(1, data.GetCount());
            //    Console.WriteLine(data.GetCount());//1

            //    var collectionList = TestContainer2.GetCollectionList()[typeof(TestContainer2).ToString()];
            //    collectionList.InsertToCache("DEF", new TestContainerBag2());
            //    data = c2Strategy.Get(key);
            //    Assert.AreEqual(2, data.GetCount());
            //    Console.WriteLine(data.GetCount());//1
            //}
        }
    }
}