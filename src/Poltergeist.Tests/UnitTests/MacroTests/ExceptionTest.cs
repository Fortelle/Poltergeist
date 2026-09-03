using Poltergeist.Automations.Macros;
using Poltergeist.Automations.Processors;

namespace Poltergeist.Tests.UnitTests.MacroTests;

[TestClass]
public class ExceptionTests
{
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
}
