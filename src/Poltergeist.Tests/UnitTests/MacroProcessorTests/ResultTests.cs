using Poltergeist.Automations.Processors;

namespace Poltergeist.Tests.UnitTests.MacroProcessorTests;

[TestClass]
public class ResultTests
{
    [TestMethod]
    public void TestIsSucceeded()
    {
        var macro = new TestMacro()
        {
            Execute = (processor) =>
            {
            }
        };

        using var processor = new MacroProcessor(macro);
        var result = processor.Execute();

        Assert.IsTrue(result.IsSucceeded);
    }

    [TestMethod]
    public void TestException()
    {
        var macro = new TestMacro()
        {
            Execute = (processor) =>
            {
                throw new TestException();
            }
        };

        using var processor = new MacroProcessor(macro);
        var result = processor.Execute();

        Assert.AreEqual(EndReason.ErrorOccurred, result.Reason);
        Assert.IsTrue(result.Exception is TestException);
    }

    [TestMethod]
    public void TestReport()
    {
        var macro = new TestMacro()
        {
            Execute = (processor) =>
            {
                processor.Report.Add("test_key", "test_value");
            }
        };
        using var processor = new MacroProcessor(macro);
        var result = processor.Execute();

        Assert.AreEqual("test_value", result.Report.Get<string>("test_key"));
    }

    [TestMethod]
    public void TestOutput()
    {
        var macro = new TestMacro()
        {
            Execute = (processor) =>
            {
                processor.OutputStorage.Add("test_key", "test_value");
            }
        };
        using var processor = new MacroProcessor(macro);
        var result = processor.Execute();

        Assert.AreEqual("test_value", result.Output.Get<string>("test_key"));
    }

}
