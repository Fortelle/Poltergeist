using Poltergeist.Automations.Processors;

namespace Poltergeist.Tests.UnitTests;

[TestClass]
public class VariableTests
{
    [TestMethod]
    public void TestGetEnvironmentVariable()
    {
        var macro = new TestMacro()
        {
            Execute = (processor) =>
            {
                var value = processor.GetVariable<int>("test_key");

                Assert.AreEqual(100, value);
            },
        };
        var args = new MacroProcessorArguments()
        {
            Environments = new()
            {
                { "test_key", 100 }
            }
        };

        macro.Test(args).AssertSuccess();
    }

    [TestMethod]
    public void TestGetOptionVariable()
    {
        var macro = new TestMacro()
        {
            Execute = (processor) =>
            {
                var value = processor.GetVariable<int>("test_key");

                Assert.AreEqual(100, value);
            },
        };
        var args = new MacroProcessorArguments()
        {
            Options = new()
            {
                { "test_key", 100 }
            }
        };

        macro.Test(args).AssertSuccess();
    }

    [TestMethod]
    public void TestGetSessionVariable()
    {
        var macro = new TestMacro()
        {
            Execute = (processor) =>
            {
                processor.SessionStorage.Add("test_key", 100);
                var value = processor.GetVariable<int>("test_key");

                Assert.AreEqual(100, value);
            },
        };

        macro.Test().AssertSuccess();
    }

    [TestMethod]
    public void TestOverrides()
    {
        var macro = new TestMacro()
        {
            Execute = (processor) =>
            {
                var value = processor.GetVariable<int>("test_key");
                Assert.AreEqual(200, value);

                processor.SessionStorage.Add("test_key", 300);
                value = processor.GetVariable<int>("test_key");
                Assert.AreEqual(300, value);
            },
        };
        var args = new MacroProcessorArguments()
        {
            Environments = new()
            {
                { "test_key", 100 }
            },
            Options = new()
            {
                { "test_key", 200 }
            },
        };

        macro.Test(args).AssertSuccess();
    }
}
