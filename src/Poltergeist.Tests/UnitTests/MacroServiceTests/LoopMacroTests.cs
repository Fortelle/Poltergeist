using Poltergeist.Automations.Components.Loops;
using Poltergeist.Automations.Processors;

namespace Poltergeist.Tests.UnitTests.MacroServiceTests;

[TestClass]
public class LoopServiceTests
{

    [TestMethod]
    public void TestDefaultMacro()
    {
        var processor = new MacroProcessor(new LoopMacro());

        var result = processor.Execute();
        Assert.IsTrue(result.IsSucceeded);
        Assert.AreEqual(1, result.Report[LoopService.ReportIterationCountKey]);
    }

    [TestMethod]
    public void TestIterateIndex()
    {
        const int count = 10;

        var buffer = new List<int>();
        var macro = new LoopMacro()
        {
            Iterate = (args) =>
            {
                buffer.Add(args.Index);
            },
        };
        var args = new MacroProcessorArguments()
        {
            Options = new()
            {
                { LoopService.ConfigEnableKey, true },
                { LoopService.ConfigCountKey, count },
            }
        };

        MacroProcessor.Execute(macro, args);

        Assert.IsTrue(buffer.SequenceEqual(Enumerable.Range(0, count)));
    }

    [TestMethod]
    public void TestBefore()
    {
        var macro = new LoopMacro()
        {
            Before = (args) =>
            {
                args.Processor.OutputStorage.Add("foo", "bar");
            },
        };

        var result = MacroProcessor.Execute(macro);

        Assert.AreEqual("bar", result.Output["foo"]);
    }

    [TestMethod]
    public void TestAfter()
    {
        var macro = new LoopMacro()
        {
            After = (args) =>
            {
                args.Processor.OutputStorage.Add("foo", "bar");
            },
        };

        var result = MacroProcessor.Execute(macro);

        Assert.AreEqual("bar", result.Output["foo"]);
    }

    [TestMethod]
    public void TestCheckContinue_Break()
    {
        const int count = 10;
        const int breakIndex = 5;

        var macro = new LoopMacro()
        {
            LoopOptions =
            {
                DefaultCount = count
            },
            CheckContinue = (args) =>
            {
                args.Result = args.IterationIndex == breakIndex
                    ? CheckContinueResult.Break
                    : CheckContinueResult.Continue;
            },
        };

        var result = MacroProcessor.Execute(macro);

        Assert.AreEqual(breakIndex + 1, result.Report[LoopService.ReportIterationCountKey]);
    }

    [TestMethod]
    public void TestRestartLoop()
    {
        const int count = 10;
        const int restartIndex = 5;

        var hasRestarted = false;
        var macro = new LoopMacro()
        {
            LoopOptions =
            {
                DefaultCount = count
            },
            CheckContinue = (args) =>
            {
                if (hasRestarted)
                {
                    return;
                }
                if (args.IterationIndex == restartIndex)
                {
                    args.Result = CheckContinueResult.RestartLoop;
                    hasRestarted = true;
                }
            },
        };

        var result = MacroProcessor.Execute(macro);

        Assert.AreEqual(restartIndex + 1 + count, result.Report[LoopService.ReportIterationCountKey]);
    }

    [TestMethod]
    public void TestRestartIteration()
    {
        const int count = 10;
        const int restartIndex = 5;

        var hasRestarted = false;
        var macro = new LoopMacro()
        {
            LoopOptions =
            {
                DefaultCount = count
            },
            CheckContinue = (args) =>
            {
                if (hasRestarted)
                {
                    return;
                }
                if (args.IterationIndex == restartIndex)
                {
                    args.Result = CheckContinueResult.RestartIteration;
                    hasRestarted = true;
                }
            },
        };

        var result = MacroProcessor.Execute(macro);

        Assert.AreEqual(count + 1, result.Report[LoopService.ReportIterationCountKey]);
    }

    [TestMethod]
    public void TestDurationOption()
    {
        var macro = new LoopMacro()
        {
            Iterate = _ =>
            {
                Thread.Sleep(400);
            },
        };

        {
            var result = MacroProcessor.Execute(macro, new()
            {
                Options = new()
                {
                    [LoopService.ConfigDurationKey] = new TimeOnly(0, 0, 1),
                    [LoopService.ConfigCountKey] = 0,
                }
            });
            Assert.AreEqual(3, result.Report[LoopService.ReportIterationCountKey]);
        }

        {
            var result = MacroProcessor.Execute(macro, new()
            {
                Options = new()
                {
                    [LoopService.ConfigDurationKey] = new TimeOnly(0, 0, 1),
                    [LoopService.ConfigCountKey] = 1,
                }
            });
            Assert.AreEqual(1, result.Report[LoopService.ReportIterationCountKey]);
        }

        {
            var result = MacroProcessor.Execute(macro, new()
            {
                Options = new()
                {
                    [LoopService.ConfigDurationKey] = new TimeOnly(0, 0, 1),
                    [LoopService.ConfigCountKey] = 10,
                }
            });
            Assert.AreEqual(3, result.Report[LoopService.ReportIterationCountKey]);
        }

        {
            var result = MacroProcessor.Execute(macro, new()
            {
                Options = new()
                {
                    [LoopService.ConfigDurationKey] = new TimeOnly(0, 0, 1),
                    [LoopService.ConfigCountKey] = 0,
                }
            });
            Assert.AreEqual(3, result.Report[LoopService.ReportIterationCountKey]);
        }
    }
}
