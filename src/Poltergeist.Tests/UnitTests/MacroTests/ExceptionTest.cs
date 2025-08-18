using Poltergeist.Automations.Macros;
using Poltergeist.Automations.Processors;

namespace Poltergeist.Tests.UnitTests.MacroTests;

[TestClass]
public class ExceptionTests
{
    private class TestBreakpointException : Exception
    {
    }

    private class GlitchMacro : MacroBase
    {
        public GlitchMacro() : base()
        {
            Exception = new Exception("This is a glitch exception.");
        }
    }

    [TestMethod]
    public void TestGlitchMacro()
    {
        var macro = new GlitchMacro();
        var processor = new MacroProcessor(macro);

        Assert.AreEqual(ProcessorStatus.Invalid, processor.Status);
        Assert.ThrowsExactly<InvalidOperationException>(processor.Execute);
    }

    private class ConfigureTestMacro : MacroBase
    {
        protected override void OnConfigure(IConfigurableProcessor processor)
        {
            throw new TestBreakpointException();
        }
    }

    [TestMethod]
    public void TestConfigure()
    {
        var macro = new ConfigureTestMacro();
        var processor = new MacroProcessor(macro);
        processor.Execute();

        Assert.IsInstanceOfType<TestBreakpointException>(processor.Exception?.InnerException);
    }


    private class PrepareTestMacro : MacroBase
    {
        protected override void OnPrepare(IPreparableProcessor processor)
        {
            throw new TestBreakpointException();
        }
    }

    [TestMethod]
    public void TestPrepare()
    {
        var macro = new PrepareTestMacro();
        var result = MacroProcessor.Execute(macro);

        Assert.IsInstanceOfType<TestBreakpointException>(result.Exception);
    }


    private class ProcessorStartedTestMacro : MacroBase
    {
        protected override void OnPrepare(IPreparableProcessor processor)
        {
            base.OnPrepare(processor);
            processor.Hooks.Register<ProcessorStartedHook>(hook =>
            {
                throw new TestBreakpointException();
            });
        }
    }

    [TestMethod]
    public void TestProcessorStarted()
    {
        var macro = new ProcessorStartedTestMacro();
        var result = MacroProcessor.Execute(macro);

        Assert.IsInstanceOfType<TestBreakpointException>(result.Exception?.InnerException);
    }

}
