using Poltergeist.Automations.Components.Hooks;
using Poltergeist.Automations.Macros;
using Poltergeist.Automations.Modules;
using Poltergeist.Automations.Processors;

namespace Poltergeist.Tests.UnitTests.MacroTests;

[TestClass]
public class MacroModuleTests
{
    private class OnMacroInitializedModule : MacroModule
    {
        public override void OnMacroInitialized(IMacroInformation macro)
        {
            macro.OptionDefinitions.Add("is_OnMacroInitialized_called", true);
        }
    }

    [TestMethod]
    public void TestModule_OnMacroInitialized()
    {
        var macro = new TestMacro()
        {
            Modules =
            {
                new OnMacroInitializedModule(),
            },
        };

        Assert.IsFalse(macro.OptionDefinitions.Contains("is_OnMacroInitialized_called"));

        ((IMacroBase)macro).Initialize();

        Assert.IsTrue(macro.OptionDefinitions.Contains("is_OnMacroInitialized_called"));
    }



    private class HookModule : MacroModule
    {
        [MacroHook]
        private static void OnProcessorStartup(IMacroProcessorShared processor, ProcessorStartupHook hook)
        {
            processor.SessionStorage.Add("is_OnProcessorStartup_called", true);
        }
    }

    [TestMethod]
    public void TestModule_Hook()
    {
        var macro = new TestMacro()
        {
            Modules =
            {
                new HookModule(),
            },
        };

        var processor = new MacroProcessor(macro);

        Assert.IsFalse(processor.SessionStorage.ContainsKey("is_OnProcessorStartup_called"));

        processor.Execute();

        Assert.IsTrue(processor.SessionStorage.ContainsKey("is_OnProcessorStartup_called"));
    }




    private class ValidateModule : MacroModule
    {
        public override bool Validate(IMacroProcessorInformation processor)
        {
            return false;
        }

        public override void OnMacroInitialized(IMacroInformation macro)
        {
            macro.OptionDefinitions.Add("is_OnMacroInitialized_called", true);
        }

        [MacroHook]
        private static void OnProcessorStartup(IMacroProcessorShared processor, ProcessorStartupHook hook)
        {
            processor.SessionStorage.Add("is_OnProcessorStartup_called", true);
        }
    }

    [TestMethod]
    public void TestModule_Validate()
    {
        var macro = new TestMacro()
        {
            Modules =
            {
                new ValidateModule(),
            },
        };

        Assert.IsFalse(macro.OptionDefinitions.Contains("is_OnMacroInitialized_called"));

        ((IMacroBase)macro).Initialize();

        Assert.IsTrue(macro.OptionDefinitions.Contains("is_OnMacroInitialized_called"));

        var processor = new MacroProcessor(macro);

        Assert.IsFalse(processor.SessionStorage.ContainsKey("is_OnProcessorStartup_called"));
    }
}
