using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Senparc.CO2NET.Tests.TestEntities;
using Senparc.CO2NET.Utilities;

namespace Senparc.CO2NET.Tests.Utilities
{
    [TestClass]
    public class EntityUtilityTests
    {
        [TestMethod]
        public void ConvertToTest()
        {
            var boolResult = EntityUtility.ConvertTo<bool>("True");
            Assert.AreEqual(true, boolResult);

            boolResult = EntityUtility.ConvertTo<bool>("true");// Write support
            Assert.AreEqual(true, boolResult);


            boolResult = EntityUtility.ConvertTo<bool>("False");
            Assert.AreEqual(false, boolResult);

            try
            {
                boolResult = EntityUtility.ConvertTo<bool>("0");
                Assert.Fail();// During execution, an exception is thrown
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);//System.FormatException: String was not recognized as a valid Boolean.
            }


            var intResult = EntityUtility.ConvertTo<int>("123456");
            Assert.AreEqual(123456, intResult);

            intResult = EntityUtility.ConvertTo<int>("-123456");
            Assert.AreEqual(-123456, intResult);

            var longResult = EntityUtility.ConvertTo<long>("1234567890123456");
            Assert.AreEqual(1234567890123456, longResult);

            var doubleResult = EntityUtility.ConvertTo<double>("1234567890123456.123456");
            Assert.AreEqual(1234567890123456.123456, doubleResult);
        }

        [TestMethod]
        public void FillSystemTypeTest()
        {
            var obj = new TestCustomObject();
            var prop = typeof(TestCustomObject).GetProperties().FirstOrDefault(z => z.Name == "Id");
            EntityUtility.FillSystemType(obj, prop, "666");
            Assert.AreEqual(666, obj.Id);

            obj.Markers = 100;
            prop = typeof(TestCustomObject).GetProperties().FirstOrDefault(z => z.Name == "Markers");
            EntityUtility.FillSystemType(obj, prop, "");
            Assert.AreEqual(null, obj.Markers);

     
            obj.ArrInt = new[] { 987, 543 };
            prop = typeof(TestCustomObject).GetProperties().FirstOrDefault(z => z.Name == "ArrInt");
            EntityUtility.FillSystemType(obj, prop, "123,456,789");
            Assert.AreEqual(3, obj.ArrInt.Length);
            Assert.AreEqual(123, obj.ArrInt[0]);
            Assert.AreEqual(456, obj.ArrInt[1]);
            Assert.AreEqual(789, obj.ArrInt[2]);


            obj.ArrLong = new[] { (long)987, (long)543 };
            prop = typeof(TestCustomObject).GetProperties().FirstOrDefault(z => z.Name == "ArrLong");
            EntityUtility.FillSystemType(obj, prop, "123,456,789");
            Assert.AreEqual(3, obj.ArrLong.Length);
            Assert.AreEqual(123, obj.ArrLong[0]);
            Assert.AreEqual(456, obj.ArrLong[1]);
            Assert.AreEqual(789, obj.ArrLong[2]);
        }
    }
}
