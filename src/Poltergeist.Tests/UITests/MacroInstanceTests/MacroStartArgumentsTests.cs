using Poltergeist.Automations.Structures.Parameters;
using Poltergeist.Modules.Macros;
using Poltergeist.Tests.UnitTests;

namespace Poltergeist.Tests.UITests.MacroInstanceTests;

[TestClass]
public class MacroStartArgumentsTests
{
    [UITestMethod]
    public async Task TestCallback()
    {
        await UITestHelper.WaitForAppWindowLoaded();

        var macro = new TestMacro();
        var instance = new MacroInstance(macro);
        instance.Load();

        var buffer = false;
        var processor = PoltergeistApplication.GetService<MacroManager>().CreateProcessor(instance, new()
        {
            Callback = _ => buffer = true,
        });
        processor.Execute();

        Assert.IsTrue(buffer);
    }

    [UITestMethod]
    public async Task TestOptionOverrides()
    {
        await UITestHelper.WaitForAppWindowLoaded();

        var macro = new TestMacro()
        {
            OptionDefinitions =
            {
                new OptionDefinition<string>("test_key", "test_value")
            }
        };

        var instance = new MacroInstance(macro);
        instance.Load();

        var processor = PoltergeistApplication.GetService<MacroManager>().CreateProcessor(instance, new()
        {
            OptionOverrides = new()
            {
                {"test_key", "overridden_value"},
            },
        });
        processor.Execute();

        Assert.AreEqual("overridden_value", processor.Options.Get<string>("test_key"));
    }

    [UITestMethod]
    public async Task TestEnvironmentOverrides()
    {
        await UITestHelper.WaitForAppWindowLoaded();

        var macro = new TestMacro();

        var instance = new MacroInstance(macro);
        instance.Load();

        var processor = PoltergeistApplication.GetService<MacroManager>().CreateProcessor(instance, new()
        {
            EnvironmentOverrides = new()
            {
                {"application_name", "overridden_value"},
            },
        });
        processor.Execute();

        Assert.AreEqual("overridden_value", processor.Environments.Get<string>("application_name"));
    }

    [UITestMethod]
    public async Task TestIncognitoMode()
    {
        await UITestHelper.WaitForAppWindowLoaded();

        var macro = new TestMacro()
        {
            OptionDefinitions =
            {
                new OptionDefinition<string>("test_key_1", "original_value"),
                new OptionDefinition<string>("test_key_2", "original_value"),
            }
        };

        var instance = new MacroInstance(macro);
        instance.Load();
        instance.Options!.Set("test_key_1", "overridden_value");

        var processor = PoltergeistApplication.GetService<MacroManager>().CreateProcessor(instance, new()
        {
            IncognitoMode = true,
            OptionOverrides = new()
            {
                {"test_key_2", "overridden_value"},
            },
            EnvironmentOverrides = new()
            {
                {"test_key", "test_value"},
            },
            SessionStorage = new()
            {
                {"test_key", "test_value"},
            },
        });
        processor.Execute();

        Assert.AreEqual("original_value", processor.Options.Get<string>("test_key_1"));
        Assert.AreEqual("overridden_value", processor.Options.Get<string>("test_key_2"));
        Assert.AreEqual(PoltergeistApplication.ApplicationName, processor.Environments.Get<string>("application_name"));
        Assert.AreEqual("test_value", processor.Environments.Get<string>("test_key"));
        Assert.AreEqual("test_value", processor.SessionStorage.Get<string>("test_key"));
    }
}
