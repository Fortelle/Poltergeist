using Poltergeist.Automations.Macros.Loops;
using Poltergeist.Automations.Processors;
using Poltergeist.Automations.Structures.Parameters;

namespace Poltergeist.Tests.UnitTests.MacroTests;

[TestClass]
public class LoopMacroTests
{

    [TestMethod]
    public void TestDefaultMacro()
    {
        var processor = new MacroProcessor(new LoopMacro());

        var result = processor.Execute();
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(1, result.Report[LoopService.ReportIterationDefinition.Key]);
    }

    [TestMethod]
    public void TestIterationIndex()
    {
        const int count = 10;

        var buffer = new List<int>();
        var macro = new LoopMacro()
        {
            Execute = (controller, context) =>
            {
                buffer.Add(context.Index);
            },
        };
        var args = new MacroProcessorArguments()
        {
            Options =
            [
                LoopConfiguralizationModule.PatternDefinition.WithValue(LoopPattern.Multiple),
                LoopConfiguralizationModule.CountDefinition.WithValue(count),
            ]
        };

        MacroProcessor.Execute(macro, args);

        Assert.IsTrue(buffer.SequenceEqual(Enumerable.Range(0, count)));
    }

    [TestMethod]
    public void TestBefore()
    {
        const int count = 3;

        var beforeCounter = 0;
        var iterationCounter = 0;

        var macro = new LoopMacro()
        {
            Start = (controller) =>
            {
                controller.Processor.OutputStorage.Add("foo", "bar");
                beforeCounter++;
                return true;
            },
            Execute = (controller, context) =>
            {
                iterationCounter++;
            },
        };

        var args = new MacroProcessorArguments()
        {
            Options =
            [
                LoopConfiguralizationModule.PatternDefinition.WithValue(LoopPattern.Multiple),
                LoopConfiguralizationModule.CountDefinition.WithValue(count),
            ]
        };

        var result = MacroProcessor.Execute(macro, args);

        Assert.AreEqual("bar", result.Outputs["foo"]);
        Assert.AreEqual(1, beforeCounter);
        Assert.AreEqual(count, iterationCounter);
    }

    [TestMethod]
    public void TestAfter()
    {
        const int count = 3;

        var afterCounter = 0;
        var iterationCounter = 0;

        var macro = new LoopMacro()
        {
            Execute = (controller, context) =>
            {
                iterationCounter++;
            },
            End = (controller) =>
            {
                controller.Processor.OutputStorage.Add("foo", "bar");
                afterCounter++;
            },
        };

        var args = new MacroProcessorArguments()
        {
            Options =
            [
                LoopConfiguralizationModule.PatternDefinition.WithValue(LoopPattern.Multiple),
                LoopConfiguralizationModule.CountDefinition.WithValue(count),
            ]
        };

        var result = MacroProcessor.Execute(macro, args);

        Assert.AreEqual("bar", result.Outputs["foo"]);
        Assert.AreEqual(1, afterCounter);
        Assert.AreEqual(count, iterationCounter);
    }

    [TestMethod]
    public void TestTransition_Break()
    {
        const int count = 10;
        const int breakIndex = 5;

        var iterationCounter = 0;

        var macro = new LoopMacro()
        {
            Execute = (controller, context) =>
            {
                iterationCounter++;
            },
            Transition = (controller, context) =>
            {
                if (context.Index == breakIndex)
                {
                    context.Cancel = true;
                }
            },
        };

        var args = new MacroProcessorArguments()
        {
            Options =
            [
                LoopConfiguralizationModule.PatternDefinition.WithValue(LoopPattern.Multiple),
                LoopConfiguralizationModule.CountDefinition.WithValue(count),
            ]
        };

        var result = MacroProcessor.Execute(macro, args);

        Assert.AreEqual(breakIndex + 1, iterationCounter);
    }

    [TestMethod]
    [DataRow(LoopPattern.Once, 0, 1)]
    [DataRow(LoopPattern.Once, 1, 1)]
    [DataRow(LoopPattern.Once, 10, 1)]
    [DataRow(LoopPattern.Multiple, 0, 1)]
    [DataRow(LoopPattern.Multiple, 1, 1)]
    [DataRow(LoopPattern.Multiple, 3, 3)]
    [DataRow(LoopPattern.Multiple, 10, 5)]
    [DataRow(LoopPattern.Unlimited, 0, 5)]
    [DataRow(LoopPattern.Unlimited, 10, 5)]
    public void TestCountOption(LoopPattern pattern, int count, int expected)
    {
        const int breakIndex = 4;

        var iterationCounter = 0;

        var macro = new LoopMacro()
        {
            Execute = (controller, context) =>
            {
                iterationCounter++;
            },
            Transition = (controller, context) =>
            {
                if (context.Index == breakIndex)
                {
                    controller.Exit();
                }
            }
        };

        var args = new MacroProcessorArguments()
        {
            Options =
            [
                LoopConfiguralizationModule.PatternDefinition.WithValue(pattern),
                LoopConfiguralizationModule.CountDefinition.WithValue(count),
            ],
        };

        var result = MacroProcessor.Execute(macro, args);

        Assert.AreEqual(expected, iterationCounter);
    }

    [TestMethod]
    [DataRow(LoopPattern.Once, 1)]
    [DataRow(LoopPattern.Multiple, 1)]
    [DataRow(LoopPattern.Unlimited, 3)]
    public void TestDurationOption(LoopPattern pattern, int expected)
    {
        var iterationCounter = 0;

        var macro = new LoopMacro()
        {
            Execute = (controller, context) =>
            {
                Thread.Sleep(200);
                iterationCounter++;
            },
            Transition = (controller, context) =>
            {
                if (context.Index == 4)
                {
                    controller.Exit();
                }
            }
        };

        var args = new MacroProcessorArguments()
        {
            Options =
            [
                LoopConfiguralizationModule.PatternDefinition.WithValue(pattern),
                LoopConfiguralizationModule.DurationDefinition.WithValue(new TimeOnly(0, 0, 0, 500)),
            ],
        };

        var result = MacroProcessor.Execute(macro, args);

        Assert.AreEqual(expected, iterationCounter);
    }

    [TestMethod]
    [DataRow(LoopPattern.Once, 2, 999, 1)]
    [DataRow(LoopPattern.Multiple, 1, 999, 1)]
    [DataRow(LoopPattern.Multiple, 10, 500, 3)]
    [DataRow(LoopPattern.Unlimited, 1, 500, 3)]
    public void TestCountAndDuration(LoopPattern pattern, int count, int timeout, int expected)
    {
        var iterationCounter = 0;

        var macro = new LoopMacro()
        {
            Execute = (controller, context) =>
            {
                Thread.Sleep(200);
                iterationCounter++;
            },
        };

        var args = new MacroProcessorArguments()
        {
            Options =
            [
                LoopConfiguralizationModule.PatternDefinition.WithValue(pattern),
                LoopConfiguralizationModule.CountDefinition.WithValue(count),
                LoopConfiguralizationModule.DurationDefinition.WithValue(new TimeOnly(0, 0, 0, timeout)),
            ],
        };

        var result = MacroProcessor.Execute(macro, args);

        Assert.AreEqual(expected, iterationCounter);
    }

}
