using Poltergeist.Automations.Components.Hooks;
using Poltergeist.Automations.Processors;

namespace Poltergeist.Tests.UnitTests.MacroServiceTests;

[TestClass]
public class HookServiceTests
{
    private class TestHook : MacroHook
    {
    }

    [TestMethod]
    public void TestRaise()
    {
        var value = false;

        var macro = new TestMacro()
        {
            Execute = processor =>
            {
                var hookService = processor.GetService<HookService>();

                hookService.Register<TestHook>((s, e) => value = true);

                hookService.Raise<TestHook>();
            }
        };
        macro.Test();

        Assert.IsTrue(value);
    }

    [TestMethod]
    public void TestOnce()
    {
        var value = 0;

        var macro = new TestMacro()
        {
            Execute = processor =>
            {
                var hookService = processor.GetService<HookService>();

                hookService.Register<TestHook>((s, e) => value++, once: true);

                hookService.Raise<TestHook>();
                hookService.Raise<TestHook>();
                hookService.Raise<TestHook>();
            }
        };
        macro.Test();

        Assert.AreEqual(1, value);
    }

    [TestMethod]
    public void TestPriority()
    {
        var value = 0;

        var macro = new TestMacro()
        {
            Execute = processor =>
            {
                var hookService = processor.GetService<HookService>();

                hookService.Register<TestHook>((s, e) => value += 1);
                hookService.Register<TestHook>((s, e) => value += 3, priority: 3);
                hookService.Register<TestHook>((s, e) => value *= 2, priority: 2);

                hookService.Raise<TestHook>();
            }
        };
        macro.Test();

        Assert.AreEqual(7, value);
    }

    [TestMethod]
    public void TestUnregister()
    {
        var value = 0;

        void handler(IMacroProcessorShared _1, TestHook _2)
        {
            value++;
        }

        var macro = new TestMacro()
        {
            Execute = processor =>
            {
                var hookService = processor.GetService<HookService>();
                hookService.Register<TestHook>(handler);
                hookService.Raise<TestHook>();
                hookService.Unregister<TestHook>(handler);
                hookService.Raise<TestHook>();
            }
        };
        macro.Test();

        Assert.AreEqual(1, value);
    }

    [TestMethod]
    public void TestKeyedHook()
    {
        var value = false;

        var macro = new TestMacro()
        {
            Execute = processor =>
            {
                var hookService = processor.GetService<HookService>();

                hookService.Register("test_hook", () => value = true);

                hookService.Raise("test_hook");
            }
        };
        macro.Test();

        Assert.IsTrue(value);
    }

    [TestMethod]
    public void TestKeyedHookParameters()
    {
        var value = 0;

        var macro = new TestMacro()
        {
            Execute = processor =>
            {
                var hookService = processor.GetService<HookService>();

                hookService.Register("test_hook", (int foo, int bar) => value = foo + bar);

                hookService.Raise("test_hook", 2, 3);
            }
        };
        macro.Test();

        Assert.AreEqual(5, value);
    }
}
