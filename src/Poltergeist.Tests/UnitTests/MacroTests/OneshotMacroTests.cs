using Poltergeist.Automations.Macros.Oneshots;
using Poltergeist.Automations.Processors;

namespace Poltergeist.Tests.UnitTests.MacroTests;

[TestClass]
public class OneshotMacroTests
{
    [TestMethod]
    public void TestDefaultMacro()
    {
        var macro = new OneshotMacro();

        var processor = new MacroProcessor(macro);

        var result = processor.Execute();
        Assert.AreEqual(ProcessorConclusion.Success, result.Conclusion);
    }

    [TestMethod]
    public void TestExecute()
    {
        var flag = false;

        var macro = new OneshotMacro()
        {
            Execute = _ =>
            {
                flag = true;
            }
        };

        var processor = new MacroProcessor(macro);

        var result = processor.Execute();
        Assert.IsTrue(flag);
    }

    [TestMethod]
    public async Task TestExecuteAsync()
    {
        var flag = false;

        var macro = new OneshotMacro()
        {
            ExecuteAsync = async _ =>
            {
                flag = true;
            }
        };

        var processor = new MacroProcessor(macro);

        var result = await processor.ExecuteAsync();

        Assert.IsTrue(flag);
    }
}
