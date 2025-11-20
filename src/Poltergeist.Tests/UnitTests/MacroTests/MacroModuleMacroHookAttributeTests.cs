using Poltergeist.Automations.Components.Hooks;
using Poltergeist.Automations.Macros;
using Poltergeist.Automations.Processors;

namespace Poltergeist.Tests.UnitTests.MacroTests;

[TestClass]
public class MacroModuleMacroHookAttributeTests
{
    private class TestingModule : MacroModule
    {
        [MacroHook]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "<Pending>")]
        public void OnProcessorStarted_Instance(ProcessorStartedHook hook)
        {
            hook.Processor.OutputStorage.Add("instance", true);
        }

        [MacroHook]
        public static void OnProcessorStarted_Static(ProcessorStartedHook hook)
        {
            hook.Processor.OutputStorage.Add("static", true);
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

        Assert.IsTrue(result.Output.Get<bool>("instance"));
        Assert.IsTrue(result.Output.Get<bool>("static"));
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

        Assert.IsFalse(result.IsSucceeded);
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

        Assert.IsFalse(result.IsSucceeded);
    }
}
