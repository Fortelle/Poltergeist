using System.Text.Json;
using Poltergeist.Automations.Macros;
using Poltergeist.Automations.Structures.Parameters;
using Poltergeist.Modules.Macros;
using Poltergeist.Tests.UnitTests;

namespace Poltergeist.Tests.UITests.MacroInstanceTests;

[TestClass]
public class UserOptionTests
{
    private readonly MacroBase UserOptionTestMacro = new TestMacro("useroption_test")
    {
        OptionDefinitions =
        {
            {"test_key", ""},
        },
        Execute = processor =>
        {
            var value = processor.Options.Get<string>("test_key");
            processor.OutputStorage.Add("test_key", value);
        },
    };

    [UITestMethod]
    public async Task TestSet()
    {
        await UITestHelper.WaitForAppWindowLoaded();

        var instance = new MacroInstance(UserOptionTestMacro);
        instance.Load();

        instance.Options?.Set("test_key", "test_value");

        var processor = PoltergeistApplication.GetService<MacroManager>().CreateProcessor(instance);
        processor.Execute();

        Assert.AreEqual("test_value", processor.OutputStorage.Get<string>("test_key"));
    }

    [UITestMethod]
    public async Task TestSave()
    {
        await UITestHelper.WaitForAppWindowLoaded();

        var instance = new MacroInstance(UserOptionTestMacro)
        {
            IsPersistent = true,
            PrivateFolder = Path.Combine(App.Paths.DocumentDataFolder, "Tests", UserOptionTestMacro.Key),
        };

        var path = Path.Combine(instance.PrivateFolder!, "UserOptions.json");
        if (File.Exists(path))
        {
            File.Delete(path);
        }

        instance.Load();

        instance.Options?.Set("test_key", "test_value");
        instance.Options?.Save();

        var text = File.ReadAllText(path);
        var json = JsonSerializer.Deserialize<Dictionary<string, object>>(text)!;
        Assert.AreEqual("test_value", json["test_key"].ToString());
    }

    [UITestMethod]
    public async Task TestLoad()
    {
        await UITestHelper.WaitForAppWindowLoaded();

        var instance = new MacroInstance(UserOptionTestMacro)
        {
            IsPersistent = true,
            PrivateFolder = Path.Combine(App.Paths.DocumentDataFolder, "Tests", UserOptionTestMacro.Key),
        };

        var path = Path.Combine(instance.PrivateFolder!, "UserOptions.json");
        var options = new Dictionary<string, object>()
        {
            { "test_key", "test_value" },
        };
        var text = JsonSerializer.Serialize(options);
        File.WriteAllText(path, text);

        instance.Load();

        var processor = PoltergeistApplication.GetService<MacroManager>().CreateProcessor(instance);
        processor.Execute();

        Assert.AreEqual("test_value", processor.OutputStorage.Get<string>("test_key"));
    }


    [UITestMethod]
    public async Task TestGlobalOptions()
    {
        await UITestHelper.WaitForAppWindowLoaded();

        var globalKey = "test_global_count";

        var macro = new TestMacro("globaloption_test")
        {
            OptionDefinitions =
            {
                new OptionDefinition<int>(globalKey, 100)
                {
                    IsGlobal = true,
                },
            },
            Execute = processor =>
            {
                var count = processor.Options.Get<int>(globalKey);
                processor.OutputStorage.Add(globalKey, count);
            },
        };

        var globalOptionsService = PoltergeistApplication.GetService<GlobalOptionsService>();
        var templateManager = PoltergeistApplication.GetService<MacroTemplateManager>();
        var macroManager = PoltergeistApplication.GetService<MacroManager>();

        globalOptionsService.GlobalOptions.Remove(globalKey);
        Assert.IsFalse(globalOptionsService.GlobalOptions.ContainsDefinition(globalKey));

        templateManager.Register(macro);
        Assert.IsTrue(globalOptionsService.GlobalOptions.ContainsDefinition(globalKey));
        Assert.AreEqual(100, globalOptionsService.GlobalOptions.Get<int>(globalKey));

        globalOptionsService.GlobalOptions.Set(globalKey, 200);

        var instance = new MacroInstance(macro, macro.Key);
        instance.Load();
        Assert.IsFalse(instance.Options!.ContainsDefinition(globalKey));

        using var processor = macroManager.CreateProcessor(instance);
        macroManager.Launch(processor, instance);
        var result = processor.GetResult();

        Thread.Sleep(500);

        Assert.AreEqual(200, result.Output.Get<int>(globalKey));

        globalOptionsService.GlobalOptions.Remove(globalKey);
    }

}
