#region Apache License Version 2.0
/*----------------------------------------------------------------

Copyright 2019 Suzhou Senparc Network Technology Co.,Ltd.

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
            //注册
            var serviceCollection = new ServiceCollection();
            var configBuilder = new ConfigurationBuilder();
            var config = configBuilder.Build();
            serviceCollection.AddSenparcGlobalServices(config);
        }

        [TestMethod()]
        public void RegisterObjectCacheStrategyTest()
        {
            //Console.WriteLine("不注册");


            {
                //不注册，使用默认

                //还原默认缓存状态
                CacheStrategyFactory.RegisterObjectCacheStrategy(null);

                var cache = CacheStrategyFactory.GetObjectCacheStrategyInstance();
                //默认为本地缓存
                Assert.IsInstanceOfType(cache, typeof(LocalObjectCacheStrategy));
            }

            {
                //注册指定缓存

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
            //    //不注册，使用默认
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
            //Console.WriteLine("使用注册");
            //{
            //    //进行注册
            //    CacheStrategyFactory.RegisterContainerCacheStrategy(() =>
            //    {
            //        return LocalContainerCacheStrategy.Instance as IContainerCacheStrategy;
            //    });

            //    var key = typeof(TestContainer2).ToString();

            //    var c2 = TestContainer2.GetCollectionList();
            //    Console.WriteLine(c2.Count);//1（位注册的时候已经注册过一个TestContainer1）
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