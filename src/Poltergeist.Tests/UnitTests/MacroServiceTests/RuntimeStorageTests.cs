using Poltergeist.Automations.Components.Storages;
using Poltergeist.Automations.Processors;

namespace Poltergeist.Tests.UnitTests.MacroServiceTests;

[TestClass]
public class RuntimeStorageTests
{
    [TestMethod]
    public void TestGetSet()
    {
        var buffer = string.Empty;

        var macro = new TestMacro()
        {
            Execute = processor =>
            {
                processor.GetService<RuntimeStorageService>().Storage.AddOrUpdate("test_key", "test_value");
                buffer = processor.GetService<RuntimeStorageService>().Storage.GetValueOrDefault("test_key", string.Empty);
            },
        };

        MacroProcessor.Execute(macro);

        Assert.AreEqual("test_value", buffer);
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
                    processor.GetService<RuntimeStorageService>().Storage.AddOrUpdate("test_key", "test_value");
                }
                else
                {
                    buffer = processor.GetService<RuntimeStorageService>().Storage.GetValueOrDefault("test_key", string.Empty);
                }
            },
        };

        MacroProcessor.Execute(macro);
        isFirstCall = false;
        MacroProcessor.Execute(macro);

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
                processor.GetService<RuntimeStorageService>().Storage.AddOrUpdate("test_key", "test_value");
            },
        };
        var macro2 = new TestMacro()
        {
            Execute = processor =>
            {
                buffer = processor.GetService<RuntimeStorageService>().Storage.GetValueOrDefault("test_key", string.Empty);
            },
        };

        MacroProcessor.Execute(macro1);
        MacroProcessor.Execute(macro2);

        Assert.AreEqual("test_value", buffer);
    }

    [TestCleanup]
    public void TestCleanup()
    {
        MacroProcessor.Execute(new TestMacro()
        {
            Execute = processor =>
            {
                processor.GetService<RuntimeStorageService>().Storage.TryRemove("test_key");
            },
        });
    }
}
