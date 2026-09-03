using Poltergeist.Automations.Components.Hooks;
using Poltergeist.Automations.Modules;
using Poltergeist.Automations.Processors;

namespace Poltergeist.Tests.UnitTests.MacroTests;

[TestClass]
public class MacroModuleMacroHookAttributeTests
{
    private class TestingModule : MacroModule
    {
        [MacroHook]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "<Pending>")]
        public void OnProcessorStartup_Instance(IMacroProcessorShared processor, ProcessorStartupHook hook)
        {
            processor.OutputStorage.Add("instance", true);
        }

        [MacroHook]
        public static void OnProcessorStartup_Static(IMacroProcessorShared processor, ProcessorStartupHook hook)
        {
            processor.OutputStorage.Add("static", true);
        }
    }

    [TestMethod]
    public void TestMacroHookAttribute()
    {
        var macro = new TestMacro()
        {
            Modules =
            {
                new TestingModule(),
            }
        };
        var result = macro.Test();

        Assert.IsTrue(result.Outputs.Get<bool>("instance"));
        Assert.IsTrue(result.Outputs.Get<bool>("static"));
    }

    private class TestingModule_EmptyParameter : MacroModule
    {
        [MacroHook]
        public static void OnProcessorStarted()
        {
        }
    }

    [TestMethod]
    public void TestEmptyParameter()
    {
        var macro = new TestMacro()
        {
            Modules =
            {
                new TestingModule_EmptyParameter(),
            }
        };
        var result = macro.Test();

        Assert.AreNotEqual(ProcessorConclusion.Success, result.Conclusion);
    }

    private class TestingModule_WrongParameterType : MacroModule
    {
        [MacroHook]
        public static void OnProcessorStarted(int _)
        {
        }
    }

    [TestMethod]
    public void TestWrongParameterType()
    {
        var macro = new TestMacro()
        {
            Modules =
            {
                new TestingModule_WrongParameterType(),
            }
        };
        var result = macro.Test();

        Assert.AreNotEqual(ProcessorConclusion.Success, result.Conclusion);
    }
}
