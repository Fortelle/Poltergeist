using Poltergeist.Automations.Components.Hooks;

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

                hookService.Register<TestHook>(_ => value = true);

                hookService.Raise<TestHook>( );
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

                hookService.Register<TestHook>(_ => value++, once: true);

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

                hookService.Register<TestHook>(_ => value += 1);
                hookService.Register<TestHook>(_ => value += 3, priority: 3);
                hookService.Register<TestHook>(_ => value *= 2, priority: 2);

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

        var macro = new TestMacro()
        {
            Execute = processor =>
            {
                var hookService = processor.GetService<HookService>();
                var handler = (TestHook _) =>
                {
                    value++;
                };

                hookService.Register(handler);
                hookService.Raise<TestHook>();
                hookService.Unregister(handler);
                hookService.Raise<TestHook>();
            }
        };
        macro.Test();

        Assert.AreEqual(1, value);
    }
}
