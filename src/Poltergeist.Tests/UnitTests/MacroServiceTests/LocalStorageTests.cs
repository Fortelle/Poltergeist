using Poltergeist.Automations.Components.Storages;
using Poltergeist.Automations.Processors;

namespace Poltergeist.Tests.UnitTests.MacroServiceTests;

[TestClass]
public class LocalStorageTests
{
    private static readonly string PrivateFolder = Path.Combine(App.Paths.DocumentDataFolder, "Tests", "LocalStorageTests");
    private static readonly string LocalStoragePath = Path.Combine(PrivateFolder, "LocalStorage.json");
    private static readonly MacroProcessorArguments PrivateFolderArguments = new()
    {
        Environments = new()
        {
            {"private_folder", PrivateFolder}
        }
    };

    [TestInitialize]
    public void TestInitialize()
    {
        if (File.Exists(LocalStoragePath))
        {
            File.Delete(LocalStoragePath);
        }
    }

    [TestCleanup]
    public void TestCleanup()
    {
        if (File.Exists(LocalStoragePath))
        {
            File.Delete(LocalStoragePath);
        }
    }

    [TestMethod]
    public void TestNoPrivateFolder()
    {
        var macro = new TestMacro()
        {
            Execute = processor =>
            {
                processor.GetService<LocalStorageService>().Storage.AddOrUpdate("test_key", "test_value");
            },
        };

        var result = MacroProcessor.Execute(macro);
        Assert.IsFalse(result.IsSucceeded);
    }

    [TestMethod]
    public void TestGetSet()
    {
        var buffer = string.Empty;

        var macro = new TestMacro()
        {
            Execute = processor =>
            {
                processor.GetService<LocalStorageService>().Storage.AddOrUpdate("test_key", "test_value");
                buffer = processor.GetService<LocalStorageService>().Storage.GetValueOrDefault("test_key", string.Empty);
            },
        };

        MacroProcessor.Execute(macro, PrivateFolderArguments);
        Assert.AreEqual("test_value", buffer);
        Assert.IsTrue(File.ReadAllText(LocalStoragePath).Contains("test_value"));
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
                    processor.GetService<LocalStorageService>().Storage.AddOrUpdate("test_key", "test_value");
                }
                else
                {
                    buffer = processor.GetService<LocalStorageService>().Storage.GetValueOrDefault("test_key", string.Empty);
                }
            },
        };

        MacroProcessor.Execute(macro, PrivateFolderArguments);
        isFirstCall = false;
        MacroProcessor.Execute(macro, PrivateFolderArguments);

        Assert.AreEqual("test_value", buffer);
    }
}
