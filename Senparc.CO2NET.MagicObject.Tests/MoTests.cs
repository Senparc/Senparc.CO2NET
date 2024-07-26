using System.Linq.Expressions;

namespace Senparc.CO2NET.MagicObject.Tests
{
    public class Person
    {
        public string Name { get; set; }
        public int Age { get; set; }
    }

    [TestClass]
    public class MOTests
    {
        private Person _person;
        private MO<Person> _mo;

        [TestInitialize]
        public void Setup()
        {
            _person = new Person { Name = "Alice", Age = 30 };
            _mo = new MO<Person>(_person);
        }

        [TestMethod]
        public void Set_SingleProperty_ChangesTracked()
        {
            _mo.Set(p => p.Name, "Bob");

            var result = _mo.Get(p => p.Name);
            Assert.AreEqual("Alice", result.OldValue);
            Assert.AreEqual("Bob", result.NewValue);
            Assert.IsTrue(result.IsChanged);
        }

        [TestMethod]
        public void Get_SingleProperty_NoChange()
        {
            var result = _mo.Get(p => p.Name);
            Assert.AreEqual("Alice", result.OldValue);
            Assert.AreEqual("Alice", result.NewValue);
            Assert.IsFalse(result.IsChanged);
        }

        [TestMethod]
        public void GetChanges_AfterSettingProperties_ReturnsChanges()
        {
            _mo.Set(p => p.Name, "Bob");
            _mo.Set(p => p.Age, 25);

            var changes = _mo.GetChanges();
            Assert.AreEqual(2, changes.Count);
            Assert.IsTrue(changes.ContainsKey("Name"));
            Assert.IsTrue(changes.ContainsKey("Age"));
        }

        [TestMethod]
        public void Reset_ResetsObjectStateAndClearsChanges()
        {
            _mo.Set(p => p.Name, "Bob");
            _mo.Reset();

            var result = _mo.Get(p => p.Name);
            Assert.AreEqual("Alice", result.NewValue);
            Assert.IsFalse(result.IsChanged);

            Assert.IsFalse(_mo.HasChanges());
        }

        [TestMethod]
        public void HasChanges_DetectsChanges()
        {
            Assert.IsFalse(_mo.HasChanges());

            _mo.Set(p => p.Name, "Bob");
            Assert.IsTrue(_mo.HasChanges());
        }

        [TestMethod]
        public void SetProperties_BatchSetProperties()
        {
            var properties = new Dictionary<Expression<Func<Person, object>>, object>
            {
                { p => p.Name, "Charlie" },
                { p => p.Age, 35 }
            };

            _mo.SetProperties(properties);

            var nameResult = _mo.Get(p => p.Name);
            var ageResult = _mo.Get(p => p.Age);

            Assert.AreEqual("Alice", nameResult.OldValue);
            Assert.AreEqual("Charlie", nameResult.NewValue);
            Assert.IsTrue(nameResult.IsChanged);

            Assert.AreEqual(30, ageResult.OldValue);
            Assert.AreEqual(35, ageResult.NewValue);
            Assert.IsTrue(ageResult.IsChanged);
        }

        [TestMethod]
        public void TakeSnapshot_RestoreSnapshot()
        {
            _mo.Set(p => p.Name, "Charlie");
            _mo.TakeSnapshot();

            _mo.Set(p => p.Name, "Dave");
            var resultBeforeRestore = _mo.Get(p => p.Name);
            Assert.AreEqual("Charlie", resultBeforeRestore.OldValue);
            Assert.AreEqual("Dave", resultBeforeRestore.NewValue);
            Assert.IsTrue(resultBeforeRestore.IsChanged);

            _mo.RestoreSnapshot();
            var resultAfterRestore = _mo.Get(p => p.Name);
            Assert.AreEqual("Charlie", resultAfterRestore.OldValue);
            Assert.AreEqual("Charlie", resultAfterRestore.NewValue);
            Assert.IsFalse(resultAfterRestore.IsChanged);
        }
    }
}