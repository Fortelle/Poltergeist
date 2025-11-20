using Poltergeist.Automations.Macros;
using Poltergeist.Automations.Processors;

namespace Poltergeist.Tests.UnitTests.MacroTests;

[TestClass]
public class ModuleDependencyAttributeTests
{
    private class TestModule : MacroModule
    {
        public int Foobar { get; set; } = 100;

        public override void OnProcessorPrepare(IPreparableProcessor processor)
        {
            base.OnProcessorPrepare(processor);

            processor.OutputStorage.Add(nameof(Foobar), Foobar);
        }
    }

    [ModuleDependency<TestModule>]
    private class ModuleTestMacro : MacroBase
    {
        public ModuleTestMacro() : base()
        {
        }
    }


    [TestMethod]
    public void TestModuleDependencyAttribute()
    {
        var macro = new ModuleTestMacro();
        var result = macro.Test();

        Assert.AreEqual(100, result.Output.Get<int>(nameof(TestModule.Foobar)));
    }


    [TestMethod]
    public void TestModuleDependencyOverrides()
    {
        var macro = new ModuleTestMacro()
        {
            Modules =
            {
                new TestModule() { Foobar = 200 }
            }
        };
        var result = macro.Test();

        Assert.AreEqual(200, result.Output.Get<int>(nameof(TestModule.Foobar)));
    }
}
