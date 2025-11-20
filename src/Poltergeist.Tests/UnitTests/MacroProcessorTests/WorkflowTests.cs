using System.Reflection;
using Poltergeist.Automations.Components.Hooks;
using Poltergeist.Automations.Macros;
using Poltergeist.Automations.Processors;

namespace Poltergeist.Tests.UnitTests;

[TestClass]
public class WorkflowTests
{
    private class WorkflowTestMacro : MacroBase
    {
        private readonly WorkflowStep[] Steps;

        public WorkflowTestMacro(WorkflowStep[] steps)
        {
            Steps = steps;
        }

        protected override void OnPrepare(IPreparableProcessor processor)
        {
            base.OnPrepare(processor);

            foreach (var step in Steps)
            {
                processor.AddStep(step);
            }

            var hookService = processor.GetService<HookService>();
            hookService.Register<ProcessorEndingHook>(e =>
            {
                var prop = typeof(MacroProcessor).GetProperty("Footsteps", BindingFlags.NonPublic | BindingFlags.Instance);
                var getter = prop!.GetGetMethod(nonPublic: true);
                var footsteps = getter!.Invoke(processor, null);

                e.OutputStorage.Add("footsteps", ((List<string>)footsteps!).ToArray());
            });
        }
    }

    private static void AssertFootsteps(WorkflowTestMacro macro, string[] expectedFootsteps)
    {
        using var processor = new MacroProcessor(macro);
        var result = processor.Execute();
        var footsteps = result.Output.Get<string[]>("footsteps");

        Assert.IsTrue(footsteps.SequenceEqual(expectedFootsteps));
    }

    [TestMethod]
    public void TestSuccess()
    {
        var macro = new WorkflowTestMacro([
            new("step_1", () => {})
            {
                IsDefault = true,
                SuccessStepId = "step_2",
            },
            new("step_2", () => {})
            {
                SuccessStepId = "step_3",
            },
            new("step_3", () => {}),
            ]);

        AssertFootsteps(macro, ["step_1", "step_2", "step_3"]);
    }

    [TestMethod]
    public void TestIteration()
    {
        var b = 0;
        var macro = new WorkflowTestMacro([
            new("step_1", () =>
            {
                b++;
            })
            {
                IsDefault = true,
                SuccessStepId = "step_2",
            },
            new("step_2", e =>
            {
                return b < 2;
            })
            {
                SuccessStepId = "step_1"
            },
            ]);

        AssertFootsteps(macro, ["step_1", "step_2", "step_1", "step_2"]);
    }

    [TestMethod]
    public void TestWrongStepId()
    {
        var macro = new WorkflowTestMacro([
            new("step_1", () => {})
            {
                IsDefault = true,
                SuccessStepId = "step_2",
            },
            new("step_2", () => {})
            {
                SuccessStepId = "step_3",
            },
            new("step_3", () => {})
            {
                SuccessStepId = "step_4",
            }
            ]);

        var result = MacroProcessor.Execute(macro);

        Assert.AreEqual("Workflow step 'step_4' is not found.", result.Exception?.Message);
    }

    [TestMethod]
    public void TestInterrupt()
    {
        foreach (var isInterruptable in new[] { true, false })
        {
            var isInterrupted = false;
            var macro = new WorkflowTestMacro([
                new("begin", e =>
                {
                    Thread.Sleep(500); // interruption happens here
                    isInterrupted = true;
                })
                {
                    IsDefault = true,
                    IsInterruptable = isInterruptable,
                    SuccessStepId = "success",
                },
                new("success", () => {}),
            ]);

            var processor = new MacroProcessor(macro);
            processor.Start();
            Thread.Sleep(200);
            processor.Stop(AbortReason.Unknown);

            var result = processor.GetResult();

            Assert.AreNotEqual(isInterruptable, isInterrupted);
            Assert.AreEqual(EndReason.Interrupted, result.Reason);
            Assert.IsTrue(result.Output.Get<string[]>("footsteps") is ["begin"]);
        }
    }

    [TestMethod]
    public void TestInitially()
    {
        var value = 0;
        var macro = new WorkflowTestMacro([
            new("initially-normal")
                {
                    IsDefault = true,
                    SuccessStepId = "success",
                    Initially = e =>
                    {
                        value++;
                    },
                    Action = e =>
                    {
                        value++;
                        return true;
                    },
                    Finally = e =>
                    {
                        value++;
                    }
                },
                new("success", () => { }),
            ]);
        AssertFootsteps(macro, ["initially-normal", "success"]);
        Assert.AreEqual(3, value);
    }

    [TestMethod]
    public void TestInitiallyError()
    {
        var value = 0;
        var macro = new WorkflowTestMacro([
            new("initially-error")
                {
                    IsDefault = true,
                    SuccessStepId = "success",
                    ErrorStepId = "error",
                    Initially = e =>
                    {
                        throw new Exception();
                    },
                    Action = e =>
                    {
                        value++;
                        return true;
                    },
                    Finally = e =>
                    {
                        value++;
                    }
                },
                new("success", () => { }),
                new("error", () => { }),
            ]);
        AssertFootsteps(macro, ["initially-error", "error"]);
        Assert.AreEqual(1, value);
    }

    [TestMethod]
    public void TestFinallyWithInitiallyError()
    {
        var b = false;
        var macro = new WorkflowTestMacro([
            new("initially-error")
                {
                    IsDefault = true,
                    SuccessStepId = "success",
                    Initially = e =>
                    {
                        throw new Exception();
                    },
                    Finally = e =>
                    {
                        b = true;
                    }
                },
            ]);
        MacroProcessor.Execute(macro);
        Assert.IsTrue(b);
    }

    [TestMethod]
    public void TestFinallyWithBodyError()
    {
        var b = false;
        var macro = new WorkflowTestMacro([
            new("action-error")
                {
                    IsDefault = true,
                    SuccessStepId = "success",
                    Action = e =>
                    {
                        throw new Exception();
                    },
                    Finally = e =>
                    {
                        b = true;
                    }
                },
            ]);
        MacroProcessor.Execute(macro);
        Assert.IsTrue(b);
    }

    [TestMethod]
    public void TestFinallyError()
    {
        var macro = new WorkflowTestMacro([
            new("begin", () => throw new Exception())
                {
                    IsDefault = true,
                    SuccessStepId = "success",
                    ErrorStepId = "error",
                    Finally = e =>
                    {
                        throw new Exception();
                    }
                },
                new("success", () => {}),
                new("error", () => {}),
            ]);
        var result = MacroProcessor.Execute(macro);
        Assert.IsTrue(result.Output.Get<string[]>("footsteps") is ["begin"]);
        Assert.AreEqual(EndReason.ErrorOccurred, result.Reason);
    }

    [TestMethod]
    public void TestFinallyRedirect()
    {
        var macro = new WorkflowTestMacro([
            new("begin", () => throw new Exception())
                {
                    IsDefault = true,
                    SuccessStepId = "success",
                    ErrorStepId = "error",
                    Finally = e =>
                    {
                        e.NextStepId = "finally";
                    }
                },
                new("success", () => {}),
                new("error", () => {}),
                new("finally", () => {}),
            ]);
        AssertFootsteps(macro, ["begin", "finally"]);
    }

    [TestMethod]
    public void TestFailureStep()
    {
        var macro = new WorkflowTestMacro([
            new("begin", _ => false)
            {
                IsDefault = true,
                SuccessStepId = "success",
                FailureStepId = "failure",
                ErrorStepId = "error",
            },
            new("success", () => {}),
            new("failure", () => {}),
            new("error", () => {}),
            ]);

        AssertFootsteps(macro, ["begin", "failure"]);
    }

    [TestMethod]
    public void TestExceptionWithErrorStepId()
    {
        var macro = new WorkflowTestMacro([
            new("begin", () => throw new Exception())
                {
                    IsDefault = true,
                    SuccessStepId = "success",
                    FailureStepId = "failure",
                    ErrorStepId = "error",
                },
                new("success", () => { }),
                new("failure", () => { }),
                new("error", () => { }),
                ]);
        AssertFootsteps(macro, ["begin", "error"]);
    }

    [TestMethod]
    public void TestExceptionWithoutErrorStepId()
    {
        var macro = new WorkflowTestMacro([
            new("begin", () => throw new Exception())
                {
                    IsDefault = true,
                    SuccessStepId = "success",
                    FailureStepId = "failure",
                },
                new("success", () => { }),
                new("failure", () => { }),
                ]);
        AssertFootsteps(macro, ["begin"]);
    }

    [TestMethod]
    public void TestExceptionWithFailureStepId()
    {
        var macro = new WorkflowTestMacro([
            new("begin", () => throw new Exception())
            {
                IsDefault = true,
                SuccessStepId = "success",
            },
            new("success", () => { }),
            new("failure", () => { }),
            ]);
        AssertFootsteps(macro, ["begin"]);
    }

}
