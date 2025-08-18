using System.Diagnostics;
using Poltergeist.Automations.Structures.Parameters;

namespace Poltergeist.Tests.UnitTests.Components;

[TestClass]
public class ParameterValueCollectionTests
{
    private class PVC : ParameterValueCollection
    {
    }

    [TestMethod]
    public void TestAddOrUpdate()
    {
        {
            var pvc = new PVC();
            Assert.AreEqual("bar", pvc.AddOrUpdate("foo", "bar", x => x + "qux")); // add a new value
            Assert.AreEqual("barqux", pvc.AddOrUpdate("foo", "bar", x => x + "qux")); // update the existing value
            Assert.ThrowsExactly<InvalidCastException>(() => pvc.AddOrUpdate("foo", 0, x => x + 1)); // wrong type
        }
    }

    [TestMethod]
    public void TestGet()
    {
        {
            var pvc = new PVC()
            {
                { "foo", "bar" }
            };
            Assert.AreEqual("bar", pvc.Get("foo"));
            Assert.ThrowsExactly<KeyNotFoundException>(() => pvc.Get<string>("baz")); // absent key
            Assert.ThrowsExactly<InvalidCastException>(() => pvc.Get<int>("foo")); // wrong type
            Assert.ThrowsExactly<KeyNotFoundException>(() => pvc.Get<int>("baz")); // absent key with wrong type
        }
    }

    [TestMethod]
    public void TestGetOrAdd()
    {
        {
            var pvc = new PVC()
            {
                { "foo", "bar" }
            };
            Assert.AreEqual("bar", pvc.GetOrAdd("foo", "qux")); // successfully gets the existing value
            Assert.AreEqual("bar", pvc.GetOrAdd("foo", () => "qux")); // existing key, returns the existing value
            Assert.AreEqual("bar", pvc.GetOrAdd<string>("foo", () => throw new UnreachableException())); // should not call the factory
            Assert.ThrowsExactly<InvalidCastException>(() => pvc.GetOrAdd("foo", 0)); // existing key, wrong type
        }

        {
            var pvc = new PVC()
            {
                { "foo", "bar" }
            };
            Assert.AreEqual("qux", pvc.GetOrAdd("baz", "qux")); // adds a new value
            Assert.AreEqual("bar", pvc["foo"]);
            Assert.AreEqual("qux", pvc["baz"]);
        }

        {
            var pvc = new PVC()
            {
                { "foo", "bar" }
            };
            Assert.AreEqual("qux", pvc.GetOrAdd("baz", () => "qux")); // absent key, calls the factory and adds the value
            Assert.AreEqual("bar", pvc["foo"]);
            Assert.AreEqual("qux", pvc["baz"]);
        }
    }

    [TestMethod]
    public void TestGetValueOrDefault()
    {
        {
            var pvc = new PVC()
            {
                { "foo", "bar" }
            };
            Assert.AreEqual("bar", pvc.GetValueOrDefault<string>("foo")); // existing key, returns the value
            Assert.IsNull(pvc.GetValueOrDefault<string>("baz")); // absent key, returns default(T)
            Assert.AreEqual("qux", pvc.GetValueOrDefault("baz", "qux")); // absent key, returns the second parameter
            Assert.ThrowsExactly<InvalidCastException>(() => pvc.GetValueOrDefault<int>("foo")); // existing key, wrong type
        }

        {
            var pvc = new PVC()
            {
                { "foo", "bar" }
            };
            var pd = new ParameterDefinition<string>("foo", "qux");
            Assert.AreEqual("bar", pvc.GetValueOrDefault(pd)); // existing key, returns the value
        }

        {
            var pvc = new PVC();
            var pd = new ParameterDefinition<string>("baz", "qux");
            Assert.AreEqual("qux", pvc.GetValueOrDefault(pd)); // absent key, returns pd.DefaultValue
            Assert.AreEqual("bar", pvc.GetValueOrDefault(pd, "bar")); // absent key, returns the second parameter
        }
    }


    [TestMethod]
    public void TestTryAdd()
    {
        // add an element
        {
            var pvc = new PVC();
            Assert.IsTrue(pvc.TryAdd("foo", () => "bar"));
            Assert.AreEqual("bar", pvc["foo"]);
            Assert.IsTrue(pvc.HasChanged);
        }

        // add an element with a factory function and out parameter
        {
            var pvc = new PVC();
            Assert.IsTrue(pvc.TryAdd("foo", () => "bar", out var foo));
            Assert.AreEqual("bar", foo);
            Assert.AreEqual("bar", pvc["foo"]);
            Assert.IsTrue(pvc.HasChanged);
        }
    }

    [TestMethod]
    public void TestTryGetValue()
    {
        // get value of an existing key
        {
            var pvc = new PVC()
            {
                { "foo", "bar" }
            };
            Assert.IsTrue(pvc.TryGetValue<string>("foo", out var foo));
            Assert.AreEqual("bar", foo);
        }

        // get value of an absent key
        {
            var ocd = new PVC()
            {
                { "foo", "bar" }
            };
            Assert.IsFalse(ocd.TryGetValue<string>("baz", out var baz));
            Assert.IsNull(baz);
        }
    }


    [TestMethod]
    public void TestTryUpdate()
    {
        // update with a function
        {
            var ocd = new PVC()
            {
                { "foo", "bar" }
            };
            ocd.HasChanged = false;
            Assert.IsTrue(ocd.TryUpdate<string>("foo", x => x + "qux", out var foo));
            Assert.AreEqual("barqux", foo);
            Assert.AreEqual("barqux", ocd["foo"]);
            Assert.IsTrue(ocd.HasChanged);
        }

        // update an absent key with a function
        {
            var ocd = new PVC()
            {
                { "foo", "bar" },
            };
            ocd.HasChanged = false;
            Assert.IsFalse(ocd.TryUpdate<string>("baz", x => x + "qux", out var baz));
            Assert.IsNull(baz);
            Assert.IsFalse(ocd.ContainsKey("baz"));
            Assert.IsFalse(ocd.HasChanged);
        }

    }

    private class Child { }
    private class Parent : Child { }

    [TestMethod]
    public void TestTypeCasting()
    {
        {
            var ocd = new PVC()
            {
                { "foo", "bar" },
            };

            Assert.ThrowsExactly<InvalidCastException>(() => ocd.Get<int>("foo"));
        }

        {
            var ocd = new PVC()
            {
                { "baz", null },
            };

            Assert.ThrowsExactly<NullReferenceException>(() => ocd.Get<int>("baz"));
            Assert.IsNull(ocd.Get<int?>("baz"));
            Assert.IsNull(ocd.Get<Child>("baz"));
            Assert.AreSame(ocd.Get<Child>("baz"), ocd.Get<Child?>("baz"));
        }

        {
            var ocd = new PVC()
            {
                { "child", new Child() },
            };

            Assert.ThrowsExactly<InvalidCastException>(() => ocd.Get<Parent>("child"));
        }

        {
            var ocd = new PVC()
            {
                { "parent", new Parent() },
            };

            Assert.IsInstanceOfType<Child>(ocd.Get<Child>("parent"));
        }

        {
            var ocd = new PVC()
            {
                { "parent", new Parent() },
            };

            Assert.AreSame(ocd.Get<Parent>("parent"), ocd.Get<Child>("parent"));
        }
    }
}
