using Poltergeist.Automations.Components.Hooks;
using Poltergeist.Automations.Macros;
using Poltergeist.Automations.Macros.Oneshots;
using Poltergeist.Automations.Modules;
using Poltergeist.Automations.Processors;

namespace Poltergeist.Tests.UnitTests;

[TestClass]
public class ControlTests
{

    [ModuleDependency<OneshotModule>]
    private class ControlTestMacro : MacroBase
    {
        public Action<IMacroProcessorShared>? Execute;
        public Func<IMacroProcessorShared, Task>? ExecuteAsync;

        [MacroHook]
        public void OnOneshotSetup(IMacroProcessorShared processor, OneshotSetupHook hooks)
        {
            hooks.ExecuteAsync = async (processor) =>
            {
                if (Execute is not null)
                {
                    Execute(processor);
                }
                else if (ExecuteAsync is not null)
                {
                    await ExecuteAsync(processor);
                }
            };
        }
    }

    [TestMethod]
    public void TestStart()
    {
        var value = 0;

        var macro = new ControlTestMacro()
        {
            Execute = _ =>
            {
                Thread.Sleep(1000);
                value = 1;
            },
        };

        var processor = new MacroProcessor(macro);
        processor.Start();
        Thread.Sleep(2000);

        Assert.AreEqual(ProcessorStatus.Complete, processor.Status);
        Assert.AreEqual(1, value);
    }

    [TestMethod]
    public void TestGetResult()
    {
        var value = 0;

        var macro = new ControlTestMacro()
        {
            Execute = p =>
            {
                Thread.Sleep(1000);
                value = 1;
            },
        };

        var processor = new MacroProcessor(macro);
        processor.Start();
        var result = processor.GetResult();

        Assert.AreEqual(ProcessorConclusion.Success, result.Conclusion);
        Assert.AreEqual(1, value);
    }

    [TestMethod]
    public void TestExecute()
    {
        var value = 0;

        var macro = new ControlTestMacro()
        {
            Execute = _ =>
            {
                Thread.Sleep(1000);
                value = 1;
            },
        };

        var processor = new MacroProcessor(macro);
        processor.Execute();

        Assert.AreEqual(ProcessorStatus.Complete, processor.Status);
        Assert.AreEqual(1, value);
    }

    [TestMethod]
    public async Task TestExecuteAsync()
    {
        var value = 0;

        var macro = new ControlTestMacro()
        {
            Execute = _ =>
            {
                Thread.Sleep(1000);
                value = 1;
            },
        };

        var processor = new MacroProcessor(macro);
        await processor.ExecuteAsync();

        Assert.AreEqual(ProcessorStatus.Complete, processor.Status);
        Assert.AreEqual(1, value);
    }

    [TestMethod]
    public void TestStop()
    {
        var value = 0;

        var macro = new ControlTestMacro()
        {
            Execute = p =>
            {
                p.CancellationToken.WaitHandle.WaitOne(1000);
                p.ThrowIfCancellationRequested();
                value = 1;
            },
        };

        var processor = new MacroProcessor(macro);

        processor.Start();
        Thread.Sleep(500);
        processor.Stop(AbortReason.Test);
        Thread.Sleep(1000);

        Assert.AreEqual(ProcessorStatus.Stopped, processor.Status);
        Assert.AreEqual(0, value);
    }

    [TestMethod]
    public void TestCancel()
    {
        var value = 0;

        var macro = new ControlTestMacro()
        {
            ExecuteAsync = async (p) =>
            {
                value = 1;
                await Task.Delay(1000, p.CancellationToken); // interrupt here
                //p.ThrowIfCancellationRequested();
                value = 2;
            },
        };

        var processor = new MacroProcessor(macro);

        processor.Start();
        Thread.Sleep(500);
        processor.Stop(AbortReason.Test);
        Thread.Sleep(1000);

        Assert.AreEqual(1, value);
    }

    [TestMethod]
    public void TestIntervene()
    {
        var isIntervened = false;

        var macro = new ControlTestMacro
        {
            Execute = (p) =>
            {
                Thread.Sleep(1000);
                isIntervened = p.SessionStorage.GetValueOrDefault<string>("test_key") == "test_value";
            },
            Interventions =
            {
                new()
                {
                    Key = "test_intervention",
                    Title = "test_intervention",
                    Variables = new()
                    {
                        { "test_key", "test_value" },
                    },
                }
            },
        };

        var processor = new MacroProcessor(macro);
        processor.Start();
        Thread.Sleep(500);
        processor.TryIntervene("test_intervention");
        processor.GetResult();

        Assert.IsTrue(isIntervened);
    }

}
