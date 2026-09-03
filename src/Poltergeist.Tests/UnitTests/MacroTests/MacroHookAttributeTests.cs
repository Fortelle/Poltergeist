using Poltergeist.Automations.Components.Hooks;
using Poltergeist.Automations.Macros;
using Poltergeist.Automations.Processors;

namespace Poltergeist.Tests.UnitTests.MacroTests;

[TestClass]
public class MacroHookAttributeTests
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "<Pending>")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "IDE0051:Remove unused private members", Justification = "<Pending>")]
    private class TestingMacro : MacroBase
    {
        [MacroHook]
        public void OnProcessorStartup_Instance_Public(IMacroProcessorShared processor, ProcessorStartupHook _)
        {
            processor.OutputStorage.Add("instance_public", true);
        }

        [MacroHook]
        private void OnProcessorStartup_Instance_Private(IMacroProcessorShared processor, ProcessorStartupHook _)
        {
            processor.OutputStorage.Add("instance_private", true);
        }

        [MacroHook]
        public static void OnProcessorStartup_Static_Public(IMacroProcessorShared processor, ProcessorStartupHook _)
        {
            processor.OutputStorage.Add("static_public", true);
        }

        [MacroHook]
        private static void OnProcessorStartup_Static_Private(IMacroProcessorShared processor, ProcessorStartupHook _)
        {
            processor.OutputStorage.Add("static_private", true);
        }
    }

    [TestMethod]
    public void TestInjections()
    {
        var macro = new TestingMacro();
        var result = macro.Test();

        Assert.IsTrue(result.Outputs.Get<bool>("instance_public"));
        Assert.IsTrue(result.Outputs.Get<bool>("instance_private"));
        Assert.IsTrue(result.Outputs.Get<bool>("static_public"));
        Assert.IsTrue(result.Outputs.Get<bool>("static_private"));
    }

    private class TestingMacro_EmptyParameter : MacroBase
    {
        [MacroHook]
        public static void OnProcessorStarted() 
        {
        }
    }

    [TestMethod]
    public void TestEmptyParameter()
    {
        var macro = new TestingMacro_EmptyParameter();
        var result = macro.Test();

        Assert.AreNotEqual(ProcessorConclusion.Success, result.Conclusion);
    }

    private class TestingMacro_WrongParameterType : MacroBase
    {
        [MacroHook]
        public static void OnProcessorStarted(int _)
        {
        }
    }

    [TestMethod]
    public void TestWrongParameterType()
    {
        var macro = new TestingMacro_WrongParameterType();
        var result = macro.Test();

        Assert.AreEqual(ProcessorConclusion.Crushed, result.Conclusion);
    }
}
