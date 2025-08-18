using Poltergeist.Modules.Macros;
using Poltergeist.Tests.UnitTests;

namespace Poltergeist.Tests.UITests.MacroInstanceTests;

[TestClass]
public class EnvironmentsTests
{
    [UITestMethod]
    public async Task TestInstanceEnvironments()
    {
        await UITestHelper.WaitForAppWindowLoaded();

        var macroManager = PoltergeistApplication.GetService<MacroManager>();

        var macro = new TestMacro();
        var instance = new MacroInstance(macro);
        var processor = macroManager.CreateProcessor(instance);
        macroManager.Launch(processor, instance);
        processor.GetResult();

        Assert.AreEqual(instance.InstanceId, processor.Environments.GetValueOrDefault<string>("macro_instance_id"));
    }

    [UITestMethod]
    public async Task TestGlobalEnvironments()
    {
        await UITestHelper.WaitForAppWindowLoaded();

        var macroManager = PoltergeistApplication.GetService<MacroManager>();
        macroManager.GlobalEnvironments["test_key"] = "test_value";

        var macro = new TestMacro();
        var instance = new MacroInstance(macro);
        var processor = macroManager.CreateProcessor(instance);
        macroManager.Launch(processor, instance);
        processor.GetResult();

        Assert.AreEqual(PoltergeistApplication.ApplicationName, processor.Environments.GetValueOrDefault<string>("application_name"));
        Assert.AreEqual("test_value", processor.Environments.GetValueOrDefault<string>("test_key"));

        macroManager.GlobalEnvironments.Remove("test_key");
    }
}
