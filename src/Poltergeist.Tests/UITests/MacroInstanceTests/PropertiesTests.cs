using Poltergeist.Modules.Macros;
using Poltergeist.Tests.UnitTests;

namespace Poltergeist.Tests.UITests.MacroInstanceTests;

[TestClass]
public class PropertiesTests
{
    [UITestMethod]
    public async Task TestRunCount()
    {
        await UITestHelper.WaitForAppWindowLoaded();

        var instance = new MacroInstance(new TestMacro())
        {
            Properties = new(),
        };

        var macroManager = PoltergeistApplication.GetService<MacroManager>();

        for (var i = 0; i < 2; i++)
        {
            macroManager.Launch(instance).GetResult();

            Assert.AreEqual(i + 1, instance.Properties.RunCount);
        }
    }

    [UITestMethod]
    public async Task TestLastRunTime()
    {
        await UITestHelper.WaitForAppWindowLoaded();

        var instance = new MacroInstance(new TestMacro())
        {
            Properties = new(),
        };

        var macroManager = PoltergeistApplication.GetService<MacroManager>();

        for (var i = 0; i < 2; i++)
        {
            var startTime = DateTime.Now;

            macroManager.Launch(instance).GetResult();

            Assert.IsTrue(startTime < instance.Properties.LastRunTime);
        }
    }

    [UITestMethod]
    public async Task TestInstanceKey()
    {
        await UITestHelper.WaitForAppWindowLoaded();

        var instanceManager = PoltergeistApplication.GetService<MacroInstanceManager>();

        var instance = new MacroInstance(new TestMacro(), "test_id")
        {
            IsLocked = true,
            Properties = new()
            {
                Key = "test_key",
            },
        };
        PoltergeistApplication.GetService<MacroInstanceManager>().AddInstance(instance);

        var instanceFromId = instanceManager.GetInstance("test_id");
        var instanceFromKey = instanceManager.GetInstance("test_key");

        Assert.AreEqual(instance, instanceFromId);
        Assert.AreEqual(instance, instanceFromKey);

        PoltergeistApplication.GetService<MacroInstanceManager>().RemoveInstance(instance);

        Assert.IsNull(instanceManager.GetInstance("test_key"));
    }
}
