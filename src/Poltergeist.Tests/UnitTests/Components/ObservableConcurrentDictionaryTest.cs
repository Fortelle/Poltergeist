using System.Diagnostics;
using Poltergeist.Automations.Structures.Parameters;

namespace Poltergeist.Tests.UnitTests.Components;

[TestClass]
public class ObservableConcurrentDictionaryTest
{
    private class OCD : ObservableConcurrentDictionary<string, string>
    {
    }

    [TestMethod]
    public void TestHasChanged()
    {
        // should be false by default
        {
            var ocd = new OCD();
            Assert.IsFalse(ocd.HasChanged);
        }

        // Add() sets HasChanged to true
        {
#pragma warning disable IDE0028
            var ocd = new OCD();
            ocd.Add("foo", "bar");
#pragma warning restore IDE0028
            Assert.IsTrue(ocd.HasChanged);
        }

        // collection initializer calls Add() so it also sets HasChanged to true
        {
            var ocd = new OCD() {
                { "foo", "bar" }
            };
            Assert.IsTrue(ocd.HasChanged);
        }

        {
            var ocd = new OCD() {
                { "foo", "bar" }
            };
            ocd.HasChanged = false;
            ocd.Add("baz", "qux");
            Assert.IsTrue(ocd.HasChanged);
        }
    }

    [TestMethod]
    public void TestTryAdd()
    {
        // add an element
        {
            var ocd = new OCD();
            Assert.IsTrue(ocd.TryAdd("foo", "bar"));
            Assert.AreEqual("bar", ocd["foo"]);
            Assert.IsTrue(ocd.HasChanged);
        }

        // add an existing key should do nothing
        {
            var ocd = new OCD() {
                { "foo", "bar" }
            };
            ocd.HasChanged = false;
            Assert.IsFalse(ocd.TryAdd("foo", "bar"));
            Assert.IsFalse(ocd.HasChanged);
        }

        // add an element with a factory function
        {
            var ocd = new OCD();
            Assert.IsTrue(ocd.TryAdd("foo", () => "bar"));
            Assert.AreEqual("bar", ocd["foo"]);
            Assert.IsTrue(ocd.HasChanged);
        }

        // add an element with a factory function and out parameter
        {
            var ocd = new OCD();
            Assert.IsTrue(ocd.TryAdd("foo", () => "bar", out var foo));
            Assert.AreEqual("bar", foo);
            Assert.AreEqual("bar", ocd["foo"]);
            Assert.IsTrue(ocd.HasChanged);
        }

        // add an existing key with a function
        {
            var ocd = new OCD
            {
                { "foo", "bar" }
            };
            ocd.HasChanged = false;
            Assert.IsFalse(ocd.TryAdd("foo", () => "bar"));
            Assert.IsFalse(ocd.TryAdd("foo", () => "bar", out var foo));
            Assert.IsNull(foo);
            Assert.IsFalse(ocd.HasChanged);
        }
    }

    [TestMethod]
    public void TestGet()
    {
        // get value of an existing key
        {
            var ocd = new OCD
            {
                { "foo", "bar" }
            };
            Assert.AreEqual("bar", ocd.Get("foo"));
        }

        // get value of an absent key
        {
            var ocd = new OCD
            {
                { "foo", "bar" }
            };
            Assert.ThrowsExactly<KeyNotFoundException>(() => ocd.Get("baz"));
        }

        { // GetValueOrDefault
            var ocd = new OCD
            {
                { "foo", "bar" }
            };
            Assert.AreEqual("bar", ocd.GetValueOrDefault("foo"));
            Assert.IsNull(ocd.GetValueOrDefault("baz"));
        }
    }

    [TestMethod]
    public void TestGetValueOrDefault()
    {
        {
            var ocd = new OCD
            {
                { "foo", "bar" }
            };
            Assert.AreEqual("bar", ocd.GetValueOrDefault("foo"));
            Assert.IsNull(ocd.GetValueOrDefault("baz"));
        }
    }

    [TestMethod]
    public void TestGetOrAdd()
    {
        {
            var ocd = new OCD
            {
                { "foo", "bar" }
            };
            Assert.AreEqual("bar", ocd.GetOrAdd("foo", "qux"));
            Assert.AreEqual("qux", ocd.GetOrAdd("baz", "qux"));
            Assert.AreEqual("bar", ocd.GetOrAdd("foo", () => "qux"));
            Assert.AreEqual("qux", ocd.GetOrAdd("baz", () => "qux"));
            Assert.AreEqual("bar", ocd.GetOrAdd("foo", () => throw new UnreachableException()));
        }
    }

    [TestMethod]
    public void TestAddOrUpdate()
    {
        {
            var ocd = new OCD();
            Assert.AreEqual("bar", ocd.AddOrUpdate("foo", "bar"));
            Assert.AreEqual("qux", ocd.AddOrUpdate("foo", "qux"));
        }

        {
            var ocd = new OCD();
            Assert.AreEqual("bar", ocd.AddOrUpdate("foo", "bar", x => x + "qux"));
            Assert.AreEqual("barqux", ocd.AddOrUpdate("foo", "bar", x => x + "qux"));
        }

        {
            var ocd = new OCD();
            Assert.AreEqual("bar", ocd.AddOrUpdate("foo", () => "bar", x => "qux"));
            Assert.AreEqual("qux", ocd.AddOrUpdate("foo", () => "bar", x => "qux"));
        }

        {
            var ocd = new OCD();
            Assert.AreEqual("bar", ocd.AddOrUpdate("foo", () => "bar", x => throw new UnreachableException()));
            Assert.AreEqual("qux", ocd.AddOrUpdate("foo", () => throw new UnreachableException(), x => "qux"));
        }

    }

    [TestMethod]
    public void TestTryGetValue()
    {
        // get value of an existing key
        {
            var ocd = new OCD
            {
                { "foo", "bar" }
            };
            Assert.IsTrue(ocd.TryGetValue("foo", out var foo));
            Assert.AreEqual("bar", foo);
        }

        // get value of an absent key
        {
            var ocd = new OCD
            {
                { "foo", "bar" }
            };
            Assert.IsFalse(ocd.TryGetValue("baz", out var baz));
            Assert.IsNull(baz);
        }
    }

    [TestMethod]
    public void TestTryUpdate()
    {
        // update with the same value should trigger HasChanged
        {
            var ocd = new OCD() {
                { "foo", "bar" }
            };
            ocd.HasChanged = false;
            Assert.IsTrue(ocd.TryUpdate("foo", "bar"));
            Assert.IsTrue(ocd.HasChanged);
        }

        // update with a new value
        {
            var ocd = new OCD() {
                { "foo", "bar" }
            };
            ocd.HasChanged = false;
            Assert.IsTrue(ocd.TryUpdate("foo", "qux"));
            Assert.AreEqual("qux", ocd["foo"]);
            Assert.IsTrue(ocd.HasChanged);
        }

        // update an absent key
        {
            var ocd = new OCD() {
                { "foo", "bar" }
            };
            ocd.HasChanged = false;
            Assert.IsFalse(ocd.TryUpdate("buz", "qux"));
            Assert.IsFalse(ocd.ContainsKey("buz"));
            Assert.IsFalse(ocd.HasChanged);
        }

        // update with a function
        {
            var ocd = new OCD() {
                { "foo", "bar" }
            };
            ocd.HasChanged = false;
            Assert.IsTrue(ocd.TryUpdate("foo", x => x + "qux", out var foo));
            Assert.AreEqual("barqux", foo);
            Assert.AreEqual("barqux", ocd["foo"]);
            Assert.IsTrue(ocd.HasChanged);
        }

        // update an absent key with a function
        {
            var ocd = new OCD() {
                { "foo", "bar" },
            };
            ocd.HasChanged = false;
            Assert.IsFalse(ocd.TryUpdate("baz", x => x + "qux", out var baz));
            Assert.IsNull(baz);
            Assert.IsFalse(ocd.ContainsKey("baz"));
            Assert.IsFalse(ocd.HasChanged);
        }
    }

    [TestMethod]
    public void TestTryRemove()
    {
        var ocd = new OCD() {
                { "foo", "bar" },
            };
        ocd.HasChanged = false;
        Assert.IsTrue(ocd.TryRemove("foo"));
        Assert.IsTrue(ocd.HasChanged);
        ocd.HasChanged = false;
        Assert.IsFalse(ocd.TryRemove("foo"));
        Assert.IsFalse(ocd.HasChanged);
    }

    [TestMethod]
    public void TestContainsKey()
    {
        var ocd = new OCD() {
                { "foo", "bar" },
            };
        Assert.IsTrue(ocd.ContainsKey("foo"));
        Assert.IsFalse(ocd.ContainsKey("bar"));
    }

}


