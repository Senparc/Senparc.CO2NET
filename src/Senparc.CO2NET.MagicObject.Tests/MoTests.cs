using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Xml.Linq;

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
            var dt = SystemTime.Now;

            _mo.Set(p => p.Name, "Bob");

            var dt0 = SystemTime.Now;

            var result = _mo.Get(p => p.Name);

            var dt1 = SystemTime.Now;

            Console.WriteLine("Set 单次耗时(ms)：" + (dt0 - dt).TotalMilliseconds);
            Console.WriteLine("Get 单次耗时(ms)：" + (dt1 - dt0).TotalMilliseconds);

            var dt2 = SystemTime.Now;

            for (var i = 0; i < 1000; i++)
            {
                _mo.Set(p => p.Name, "Bob" + i);
                var resultX = _mo.Get(p => p.Name);
            }
            Console.WriteLine("1000 次 Set+Get 耗时(ms)：" + (SystemTime.DiffTotalMS(dt2)));

            _mo.Set(p => p.Name, "Bob");

            var dtS3 = SystemTime.Now;
            Person person = new Person { Name = "Alice", Age = 30 };
            person.Name = "Bob";
            var result2 = person.Name;
            var dtE3 = SystemTime.Now;
            Console.WriteLine("原始方法 单次 Get+Set 耗时(ms)：" + (dtE3 - dtS3).TotalMilliseconds);


            var dt4 = DateTime.Now;
            for (var i = 0; i < 1000; i++)
            {
                person.Name = "Bob" + i;
                var resultX = person.Name;
            }
            Console.WriteLine("1000 次 原始方法 耗时(ms)：" + SystemTime.DiffTotalMS(dt4));


            Assert.AreEqual("Alice", result.OldValue);
            Assert.AreEqual("Bob", result.NewValue);
            Assert.AreEqual("Bob", _person.Name);
            Assert.IsTrue(result.IsChanged);
        }

        [TestMethod]
        public void Get_SingleProperty_NoChange()
        {
            var result = _mo.Get(p => p.Name);
            Assert.AreEqual("Alice", result.OldValue);
            Assert.AreEqual("Alice", result.NewValue);
            Assert.IsFalse(result.IsChanged);
            Assert.IsFalse(result.HasShapshot);
        }

        [TestMethod]
        public void GetChanges_AfterSettingProperties_ReturnsChanges()
        {
            _mo.Set(p => p.Name, "Bob");
            _mo.Set(p => p.Age, 25);

            var changes = _mo.GetChanges();
            Assert.AreEqual(2, changes.Count());
            Assert.IsTrue(changes.ContainsKey("Name"));
            Assert.IsTrue(changes.ContainsKey("Age"));
            Assert.AreEqual("Bob", _mo.Get(z => z.Name).NewValue);
            Assert.AreEqual(25, _mo.Get(z => z.Age).NewValue);
        }

        [TestMethod]
        public void Reset_ResetsObjectStateAndClearsChanges()
        {
            Assert.AreSame(_mo.Object, _person);

            _mo.Set(p => p.Name, "Bob");
            _mo.Reset();

            Assert.AreNotSame(_mo.Object, _person);

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
            //_person = new Person { Name = "Alice", Age = 30 };

            Assert.IsFalse(_mo.HasChanges());

            _mo.Set(p => p.Name, "Charlie");
            Console.WriteLine(_mo.Get(z => z.Age).SnapshotValue);
            Assert.IsFalse(_mo.Get(z => z.Age).HasShapshot);
            Assert.IsNull(_mo.Get(z => z.Name).SnapshotValue);
            Assert.AreEqual(0, _mo.Get(z => z.Age).SnapshotValue);//仍然会返回默认值
            Assert.IsTrue(_mo.HasChanges());

            _mo.TakeSnapshot();

            Assert.IsTrue(_mo.Get(z => z.Age).HasShapshot);
            Assert.AreEqual(30, _mo.Get(z => z.Age).SnapshotValue);

            _mo.Set(p => p.Name, "Dave");
            var resultBeforeRestore = _mo.Get(p => p.Name);
            Assert.AreEqual("Alice", resultBeforeRestore.OldValue);
            Assert.AreEqual("Charlie", resultBeforeRestore.SnapshotValue);
            Assert.AreEqual("Dave", resultBeforeRestore.NewValue);
            Assert.IsTrue(resultBeforeRestore.IsChanged);

            _mo.RestoreSnapshot();
            Assert.IsTrue(_mo.Get(z => z.Age).HasShapshot);//快照本身不会被清除

            var resultAfterRestore = _mo.Get(p => p.Name);
            Assert.AreEqual("Alice", resultAfterRestore.OldValue);
            Assert.AreEqual("Charlie", resultBeforeRestore.SnapshotValue);
            Assert.AreEqual("Charlie", resultAfterRestore.NewValue);
            Assert.IsTrue(resultAfterRestore.IsChanged);
            Assert.IsFalse(_mo.HasChanges());

            //适用对象覆盖

            //TODO
        }

        [TestMethod]
        public void TestRevertChanges()
        {
            // Act  
            Assert.IsFalse(_mo.HasChanges());

            _mo.Set(x => x.Name, "I'm Jeffrey");
            Assert.IsTrue(_mo.HasChanges());
            Assert.AreEqual("I'm Jeffrey", _mo.Get(z => z.Name).NewValue);
            Assert.AreEqual("I'm Jeffrey", _person.Name);

            _mo.RevertChanges();

            // Assert  
            Assert.AreSame(_mo.Object, _person);

            Assert.AreEqual("Alice", _mo.Get(z => z.Name).NewValue);
            Assert.AreEqual("Alice", _person.Name);
            Assert.IsFalse(_mo.HasChanges());
        }
    }
}
