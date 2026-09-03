using Poltergeist.Automations.Components.Storages;
using Poltergeist.Automations.Processors;

namespace Poltergeist.Tests.UnitTests.MacroServiceTests;

[TestClass]
public class GlobalStorageTests
{
    private static readonly MacroProcessorArguments ProcessorArguments = new()
    {
        Environments = new()
        {
            {"document_data_folder", App.Paths.DocumentDataFolder}
        }
    };

    [TestMethod]
    public void TestNoEnvironments()
    {
        var macro = new TestMacro()
        {
            Execute = processor =>
            {
                processor.GetService<GlobalStorageService>().Storage.AddOrUpdate("test_key", "test_value");
            },
        };

        var result = MacroProcessor.Execute(macro);
        Assert.AreNotEqual(ProcessorConclusion.Success, result.Conclusion);
    }

    [TestMethod]
    public void TestGetSet()
    {
        var buffer = string.Empty;

        var macro = new TestMacro()
        {
            Execute = processor =>
            {
                processor.GetService<GlobalStorageService>().Storage.AddOrUpdate("test_key", "test_value");
                buffer = processor.GetService<GlobalStorageService>().Storage.GetValueOrDefault("test_key", string.Empty);
            },
        };

        var processor = new MacroProcessor(macro, ProcessorArguments);
        processor.Execute();
        Assert.AreEqual("test_value", buffer);

        var path = Path.Combine(App.Paths.DocumentDataFolder, "LocalStorage.json");
        Assert.Contains("test_value", File.ReadAllText(path));
    }

    [TestMethod]
    public void TestCrossProcessorGet()
    {
        var buffer = string.Empty;
        var isFirstCall = true;

        var macro = new TestMacro()
        {
            Execute = processor =>
            {
                if (isFirstCall)
                {
                    processor.GetService<GlobalStorageService>().Storage.AddOrUpdate("test_key", "test_value");
                }
                else
                {
                    buffer = processor.GetService<GlobalStorageService>().Storage.GetValueOrDefault("test_key", string.Empty);
                }
            },
        };

        MacroProcessor.Execute(macro, ProcessorArguments);
        isFirstCall = false;
        MacroProcessor.Execute(macro, ProcessorArguments);

        Assert.AreEqual("test_value", buffer);
    }

    [TestMethod]
    public void TestCrossMacroGet()
    {
        var buffer = string.Empty;
        var macro1 = new TestMacro()
        {
            Execute = processor =>
            {
                processor.GetService<GlobalStorageService>().Storage.AddOrUpdate("test_key", "test_value");
            },
        };
        var macro2 = new TestMacro()
        {
            Execute = processor =>
            {
                buffer = processor.GetService<GlobalStorageService>().Storage.GetValueOrDefault("test_key", string.Empty);
            },
        };

        MacroProcessor.Execute(macro1, ProcessorArguments);
        MacroProcessor.Execute(macro2, ProcessorArguments);

        Assert.AreEqual("test_value", buffer);
    }
}
