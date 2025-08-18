using Poltergeist.Modules.Macros;
using Poltergeist.Tests.UnitTests;

namespace Poltergeist.Tests.UITests.MacroInstanceTests;

[TestClass]
public class ReportsTests
{
    [UITestMethod]
    public async Task TestUpdate()
    {
        await UITestHelper.WaitForAppWindowLoaded();

        var macroManager = PoltergeistApplication.GetService<MacroManager>();

        var macro = new TestMacro();
        var instance = new MacroInstance(macro);
        instance.Load();

        var result = macroManager.Launch(instance).GetResult();

        Assert.AreEqual(result.Report["processor_id"], instance.Reports!.Last()["processor_id"]); ;
    }

    [UITestMethod]
    public async Task TestSave()
    {
        await UITestHelper.WaitForAppWindowLoaded();

        var macroManager = PoltergeistApplication.GetService<MacroManager>();

        var macro = new TestMacro("report_test");
        var instance = new MacroInstance(macro)
        {
            IsPersistent = true,
            PrivateFolder = Path.Combine(App.Paths.DocumentDataFolder, "Tests", macro.Key),
        };
        instance.Load();

        var result = macroManager.Launch(instance).GetResult();

        Thread.Sleep(500);

        var reportPath = instance.Reports!.Filepath!;
        var processorId = result.Report.Get<string>("processor_id")!;
        var text = File.ReadAllText(reportPath);
        Assert.IsTrue(text.Contains(processorId));
    }
}
