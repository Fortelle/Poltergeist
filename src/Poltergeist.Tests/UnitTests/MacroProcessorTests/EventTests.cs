using Poltergeist.Automations.Processors;

namespace Poltergeist.Tests.UnitTests;

[TestClass]
public class EventTests
{
    [TestMethod]
    public void TestLaunched()
    {
        var isLaunched = false;

        var macro = new TestMacro();
        using var processor = new MacroProcessor(macro);
        processor.Launched += (_, _) => isLaunched = true;
        processor.Start();
        Thread.Sleep(1000);

        Assert.IsTrue(isLaunched);
    }

    [TestMethod]
    public void TestCompleted()
    {
        var isCompleted = false;

        var macro = new TestMacro();
        using var processor = new MacroProcessor(macro);
        processor.Completed += (_, _) => isCompleted = true;
        processor.Start();
        Thread.Sleep(1000);

        Assert.IsTrue(isCompleted);
    }
}
