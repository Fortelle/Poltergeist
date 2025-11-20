using Poltergeist.Automations.Macros;
using Poltergeist.Automations.Processors;

namespace Poltergeist.Tests.UnitTests.MacroTests;

[TestClass]
public class MacroModuleTests
{
    private class TestModule : MacroModule
    {
        public override void OnMacroInitialize(IInitializableMacro macro)
        {
            macro.OptionDefinitions.Add("test_initialize", true);
        }

        public override void OnProcessorConfigure(IConfigurableProcessor processor)
        {
            processor.Options.Add("test_configure", true);
        }

        public override void OnProcessorPrepare(IPreparableProcessor processor)
        {
            processor.OutputStorage.Add("test_initialize", processor.Options.Get<bool>("test_initialize"));
            processor.OutputStorage.Add("test_configure", processor.Options.Get<bool>("test_configure"));
            processor.OutputStorage.Add("test_prepare", true);
        }
    }

    [TestMethod]
    public void TestInject()
    {
        var macro = new TestMacro()
        {
            Modules =
            {
                new TestModule(),
            },
        };

        Assert.IsFalse(macro.OptionDefinitions.Contains("test_initialize"));

        ((IFrontBackMacro)macro).Initialize();

        Assert.IsTrue(macro.OptionDefinitions.TryGetValue("test_initialize", out var value) && (bool)value.DefaultValue!);

        var result = macro.Test();

        Assert.IsTrue(result.Output.Get<bool>("test_initialize"));
        Assert.IsTrue(result.Output.Get<bool>("test_configure"));
        Assert.IsTrue(result.Output.Get<bool>("test_prepare"));
    }
}
