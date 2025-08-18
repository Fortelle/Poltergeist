using Poltergeist.Modules.Macros;
using Poltergeist.Tests.UnitTests;

namespace Poltergeist.Tests.UITests.MacroInstanceTests;

[TestClass]
public class PrivateFolderTests
{
    [UITestMethod]
    public async Task TestNotPersistent()
    {
        await UITestHelper.WaitForAppWindowLoaded();

        var macro = new TestMacro();

        var instance = new MacroInstance(macro);

        var macroInstanceManager = PoltergeistApplication.GetService<MacroInstanceManager>();
        macroInstanceManager.AddInstance(instance);

        instance.Load();

        Assert.IsNull(instance.PrivateFolder);
    }

    [UITestMethod]
    public async Task TestPersistent()
    {
        await UITestHelper.WaitForAppWindowLoaded();

        var macro = new TestMacro();

        var instance = new MacroInstance(macro)
        {
            IsPersistent = true
        };

        var macroInstanceManager = PoltergeistApplication.GetService<MacroInstanceManager>();
        macroInstanceManager.AddInstance(instance);

        instance.Load();

        Assert.IsNotNull(instance.PrivateFolder);
    }

    [UITestMethod]
    public async Task TestCustomPath()
    {
        await UITestHelper.WaitForAppWindowLoaded();

        var macro = new TestMacro();

        var tempPath = Path.Combine(PoltergeistApplication.Paths.DocumentDataFolder, "Tests", macro.Key);

        var instance = new MacroInstance(macro)
        {
            PrivateFolder = tempPath,
            IsPersistent = true
        };

        var macroInstanceManager = PoltergeistApplication.GetService<MacroInstanceManager>();
        macroInstanceManager.AddInstance(instance);

        instance.Load();

        Assert.AreEqual(tempPath, instance.PrivateFolder);
    }

}
