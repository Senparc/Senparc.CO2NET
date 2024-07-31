using Microsoft.VisualStudio.TestTools.UnitTesting;
using Senparc.CO2NET.MagicObject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Senparc.CO2NET.MagicObject.Tests
{
    [TestClass()]
    public class PropertyChangeResultTests
    {
        private Person _person;
        private MO<Person> _mo;

        [TestInitialize]
        public void Setup()
        {
            _person = new Person { Name = "Alice", Age = 30 };
            _mo = new MO<Person>(_person);
        }

        [TestMethod()]
        public void ToStringTest()
        {
            Assert.AreEqual("Alice", _mo.Get(z => z.Name).ToString());

            _mo.Set(z => z.Name, "Bob");

            Assert.AreEqual("Bob", _mo.Get(z => z.Name).NewValue);
            Assert.AreEqual("Alice -> Bob", _mo.Get(z => z.Name).ToString());
        }
    }
}