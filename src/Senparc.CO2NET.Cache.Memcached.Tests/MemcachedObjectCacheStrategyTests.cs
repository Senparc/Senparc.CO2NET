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

            // This method is private and is only used to check for exceptions or perform assertions
        }
    }
}
