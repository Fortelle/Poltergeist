using Microsoft.Extensions.DependencyInjection;
using Poltergeist.Automations.Components.Hooks;
using Poltergeist.Automations.Macros;
using Poltergeist.Automations.Modules;
using Poltergeist.Automations.Processors;
using Poltergeist.Automations.Services;

namespace Poltergeist.Tests.UnitTests.MacroTests;

[TestClass]
public class MacroEventTests
{
    private class TestBreakpointException : Exception
    {
    }

    private class Macro_OnInitializing : MacroBase
    {
        public int Value;

        protected override void OnInitializing()
        {
            Value++;
        }
    }

    [TestMethod]
    public void TestOnInitializing()
    {
        var macro = new Macro_OnInitializing();

        Assert.AreEqual(0, macro.Value);

        var processor = new MacroProcessor(macro);

        Assert.AreEqual(1, macro.Value);

        processor.Execute();

        Assert.AreEqual(ProcessorStatus.Complete, processor.Status);
        Assert.AreEqual(1, macro.Value); // ensure only execute once
    }

    private class Macro_OnInitializing_Exception : MacroBase
    {
        protected override void OnInitializing()
        {
            throw new TestBreakpointException();
        }
    }

    [TestMethod]
    public void TestOnInitializingException()
    {
        var macro = new Macro_OnInitializing_Exception();

        Assert.IsNull(macro.Exception);

        var processor = new MacroProcessor(macro);

        Assert.IsInstanceOfType<TestBreakpointException>(macro.Exception);

        Assert.ThrowsExactly<InvalidOperationException>(processor.Execute);
        Assert.AreEqual(ProcessorStatus.Invalid, processor.Status);
    }



    private class Module_OnModuleInstalled : MacroModule
    {
    }

    [ModuleDependency<Module_OnModuleInstalled>]
    private class Macro_OnModuleInstalled : MacroBase
    {
        public int Value;

        protected override void OnModuleInstalled(MacroModule module)
        {
            Value++;
        }
    }

    [TestMethod]
    public void TestOnModuleInstalled()
    {
        var macro = new Macro_OnModuleInstalled();
        Assert.AreEqual(0, macro.Value);

        ((IMacroBase)macro).Initialize();
        Assert.AreEqual(1, macro.Value);

        var processor = new MacroProcessor(macro);
        processor.Execute();
        Assert.AreEqual(1, macro.Value);
    }

    [ModuleDependency<Module_OnModuleInstalled>]
    private class Macro_OnModuleInstalled_Exception : MacroBase
    {
        protected override void OnModuleInstalled(MacroModule module)
        {
            throw new TestBreakpointException();
        }
    }

    [TestMethod]
    public void TestOnModuleInstalledException()
    {
        var macro = new Macro_OnModuleInstalled_Exception();

        ((IMacroBase)macro).Initialize();

        Assert.IsInstanceOfType<TestBreakpointException>(macro.Exception);
    }




    private class Macro_OnProcessorCompleted : MacroBase
    {
        public int Value;

        [MacroHook]
        private void OnProcessorCompleted(IMacroProcessorShared processor, ProcessorCompletedHook hook)
        {
            Value++;
        }
    }

    [TestMethod]
    public void TestOnProcessorCompleted()
    {
        var macro = new Macro_OnProcessorCompleted();
        Assert.AreEqual(0, macro.Value);

        var processor = new MacroProcessor(macro);
        Assert.AreEqual(0, macro.Value);

        processor.Execute();
        Assert.AreEqual(1, macro.Value);
    }









    private class Service_OnServiceRegistering : MacroService
    {
        public Service_OnServiceRegistering(MacroProcessor processor) : base(processor)
        {
            processor.OutputStorage.Add("foo", "bar");
        }
    }


    private class Macro_OnServiceRegistering : MacroBase
    {
        public int Value;

        protected override void RegisterServices(IServiceCollection services, RegisterServicesArguments args)
        {
            Value++;
            services.AddSingleton<Service_OnServiceRegistering>();
        }

        [MacroHook]
        private void OnProcessorCompleted(IMacroProcessorShared processor, ProcessorCompletedHook hook)
        {
            processor.GetService<Service_OnServiceRegistering>();
        }
    }

    [TestMethod]
    public void TestOnServiceRegistering()
    {
        var macro = new Macro_OnServiceRegistering();
        Assert.AreEqual(0, macro.Value);

        var processor = new MacroProcessor(macro);
        Assert.AreEqual(0, macro.Value);

        var result = processor.Execute();
        Assert.AreEqual(1, macro.Value);
        Assert.AreEqual("bar", result.Outputs["foo"]);
    }


}
