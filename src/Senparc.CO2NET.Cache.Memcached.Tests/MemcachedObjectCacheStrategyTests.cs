using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Senparc.CO2NET.Cache.Memcached.Tests
{
    [TestClass]
    public class MemcachedObjectCacheStrategyTests
    {
        [TestMethod]
        public void RegisterServerListTest()
        {
            var str = "localhost:12211;localhost:12345";
            MemcachedObjectCacheStrategy.RegisterServerList(str);

            //由于是private方法，这里只能确认是否有异常，或者进行读写测试
        }
    }
}
