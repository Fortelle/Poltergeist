using Poltergeist.Automations.Macros;
using Poltergeist.Automations.Processors;

namespace Poltergeist.Tests.UnitTests;

[TestClass]
public class ControlTests
{
    private class ControlTestMacro : MacroBase
    {
        public Action<IUserProcessor>? Execute;
        public Func<IUserProcessor, Task>? ExecuteAsync;

        protected override void OnPrepare(IPreparableProcessor processor)
        {
            base.OnPrepare(processor);

            processor.AddStep(new("execution", () =>
            {
                if (Execute is not null)
                {
                    Execute((IUserProcessor)processor);
                }
                else if (ExecuteAsync is not null)
                {
                    ExecuteAsync((IUserProcessor)processor).GetAwaiter().GetResult();
                }
            })
            {
                IsDefault = true,
                IsInterruptable = true,
            });
        }
    }

    [TestMethod]
    public void TestStart()
    {
        var value = 0;

        var processor = new MacroProcessor(new ControlTestMacro()
        {
            Execute = _ =>
            {
                Thread.Sleep(1000);
                value = 1;
            },
        });
        processor.Start();
        Thread.Sleep(2000);

        Assert.AreEqual(ProcessorStatus.Complete, processor.Status);
        Assert.AreEqual(1, value);
    }

    [TestMethod]
    public void TestGetResult()
    {
        var value = 0;

        var processor = new MacroProcessor(new ControlTestMacro()
        {
            Execute = p =>
            {
                Thread.Sleep(1000);
                value = 1;
            },
        });
        processor.Start();
        var result = processor.GetResult();

        Assert.AreEqual(EndReason.Complete, result.Reason);
        Assert.AreEqual(1, value);
    }

    [TestMethod]
    public void TestExecute()
    {
        var value = 0;

        var processor = new MacroProcessor(new ControlTestMacro()
        {
            Execute = _ =>
            {
                Thread.Sleep(1000);
                value = 1;
            },
        });
        processor.Execute();

        Assert.AreEqual(ProcessorStatus.Complete, processor.Status);
        Assert.AreEqual(1, value);
    }

    [TestMethod]
    public async Task TestExecuteAsync()
    {
        var value = 0;

        var processor = new MacroProcessor(new ControlTestMacro()
        {
            Execute = _ =>
            {
                Thread.Sleep(1000);
                value = 1;
            },
        });
        await processor.ExecuteAsync();

        Assert.AreEqual(ProcessorStatus.Complete, processor.Status);
        Assert.AreEqual(1, value);
    }

    [TestMethod]
    public void TestStop()
    {
        var value = 0;

        var processor = new MacroProcessor(new ControlTestMacro()
        {
            Execute = _ =>
            {
                Thread.Sleep(1000);
                value = 1;
            },
        });

        processor.Start();
        Thread.Sleep(500);
        processor.Stop(AbortReason.Unknown);
        Thread.Sleep(1000);

        Assert.AreEqual(ProcessorStatus.Stopped, processor.Status);
        Assert.AreEqual(0, value);
    }

    [TestMethod]
    public void TestCancel()
    {
        var value = 0;

        var processor = new MacroProcessor(new ControlTestMacro()
        {
            ExecuteAsync = async (p) =>
            {
                await Task.Delay(1000, p.CancellationToken); // interrupt here
                p.ThrowIfInterrupted();
                value = 1;
            },
        });

        processor.Start();
        Thread.Sleep(500);
        processor.Stop(AbortReason.Unknown);
        Thread.Sleep(1000);

        Assert.AreEqual(0, value);
    }

}
