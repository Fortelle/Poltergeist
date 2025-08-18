using Poltergeist.Automations.Components.Storages;
using Poltergeist.Automations.Processors;

namespace Poltergeist.Tests.UnitTests.MacroServiceTests;

[TestClass]
public class SessionStorageTests
{
    [TestMethod]
    public void TestGetSet()
    {
        var buffer = string.Empty;

        var macro = new TestMacro()
        {
            Execute = processor =>
            {
                processor.GetService<SessionStorageService>().Storage.AddOrUpdate("test_key", "test_value");
                buffer = processor.GetService<SessionStorageService>().Storage.GetValueOrDefault("test_key", string.Empty);
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
                    processor.GetService<SessionStorageService>().Storage.AddOrUpdate("test_key", "test_value");
                }
                else
                {
                    buffer = processor.GetService<SessionStorageService>().Storage.GetValueOrDefault("test_key", string.Empty);
                }
            },
        };

        MacroProcessor.Execute(macro);
        isFirstCall = false;
        MacroProcessor.Execute(macro);

        Assert.AreEqual(string.Empty, buffer);
    }

    [TestMethod]
    public void TestCrossMacroGet()
    {
        var buffer = string.Empty;

        var macro1 = new TestMacro()
        {
            Execute = processor =>
            {
                processor.GetService<SessionStorageService>().Storage.AddOrUpdate("test_key", "test_value");
            },
        };

        var macro2 = new TestMacro()
        {
            Execute = processor =>
            {
                buffer = processor.GetService<SessionStorageService>().Storage.GetValueOrDefault("test_key", string.Empty);
            },
        };

        MacroProcessor.Execute(macro1);
        MacroProcessor.Execute(macro2);

        Assert.AreEqual(string.Empty, buffer);
    }

    [TestMethod]
    public void TestDispose()
    {
        var buffer = string.Empty;

        var stream = new MemoryStream();

        var macro = new TestMacro()
        {
            Execute = processor =>
            {
                processor.GetService<SessionStorageService>().Storage.AddOrUpdate("test_stream", stream);
            },
        };

        Assert.IsTrue(stream.CanSeek);

        MacroProcessor.Execute(macro);

        Assert.IsFalse(stream.CanSeek);
    }
}
