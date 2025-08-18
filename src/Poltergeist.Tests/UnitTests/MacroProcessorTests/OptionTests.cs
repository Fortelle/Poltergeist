using Poltergeist.Automations.Macros;
using Poltergeist.Automations.Processors;
using Poltergeist.Automations.Structures.Parameters;

namespace Poltergeist.Tests.UnitTests;

[TestClass]
public class OptionTests
{
    [TestMethod]
    public void TestDefaultValue()
    {
        var value = "";

        var macro = new BasicMacro()
        {
            OptionDefinitions =
            {
                new OptionDefinition<string>("test_key", "test_value"),
            },
            Execute = (args) =>
            {
                value = args.Processor.Options.GetValueOrDefault<string>("test_key");
            },
        };

        MacroProcessor.Execute(macro);

        Assert.AreEqual("test_value", value);
    }

    [TestMethod]
    public void TestOverrides()
    {
        var value = "";

        var macro = new BasicMacro()
        {
            OptionDefinitions =
            {
                new OptionDefinition<string>("test_key", "test_value"),
            },
            Execute = (args) =>
            {
                value = args.Processor.Options.GetValueOrDefault<string>("test_key");
            },
        };

        MacroProcessor.Execute(macro, new MacroProcessorArguments()
        {
            Options = new()
            {
                ["test_key"] = "overridden_value",
            },
        });

        Assert.AreEqual("overridden_value", value);
    }
}
