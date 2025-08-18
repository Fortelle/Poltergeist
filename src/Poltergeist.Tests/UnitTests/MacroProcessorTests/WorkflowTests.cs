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
        var b = false;
        var macro = new WorkflowTestMacro([
            new("step_1", () => {})
            {
                IsDefault = true,
                SuccessStepId = "step_2",
            },
            new("step_2", e =>
            {
                if (b)
                {
                    return;
                }
                e.SuccessStepId = "step_1";
                b = true;
            }),
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
            }]
            );

        var result = MacroProcessor.Execute(macro);

        Assert.AreEqual("Workflow step 'step_4' is not found.", result.Exception?.Message);
    }

    [TestMethod]
    public void TestInterrupt()
    {
        foreach (var isInterruptable in new[] { true, false })
        {
            var b = false;
            var macro = new WorkflowTestMacro([
                new("begin", e =>{
                    Thread.Sleep(500); // interruption happens here
                    b = true;
                })
                {
                    IsDefault = true,
                    IsInterruptable = isInterruptable,
                    SuccessStepId = "success",
                    InterruptionStepId = "interruption",
                },
                new("success", () => {}),
                new("interruption", () => {}),
            ]);

            var processor = new MacroProcessor(macro);
            processor.Start();
            Thread.Sleep(200);
            processor.Stop(AbortReason.Unknown);

            var result = processor.GetResult();

            Assert.AreNotEqual(isInterruptable, b);
            Assert.AreEqual(EndReason.Interrupted, result.Reason);
            Assert.IsTrue(result.Output.Get<string[]>("footsteps") is ["begin", "interruption"]);
        }
    }
    
    [TestMethod]
    public void TestFinally()
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
    public void TestOutput()
    {
        var value = 0;
        var macro = new WorkflowTestMacro([
            new("step_1", e => {
                e.Output = 1;
            })
            {
                IsDefault = true,
                SuccessStepId = "step_2",
                Finally = e =>
                {
                    e.Output = (int)e.Output! + 1;
                }
            },
            new("step_2", e => {
                e.Output = (int)e.PreviousResult!.Output! + 1;
            })
            {
                Finally = e =>
                {
                    value = (int)e.Output!;
                }
            },
        ]);

        var result = MacroProcessor.Execute(macro);

        Assert.AreEqual(3, value);
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
    public void TestErrorStep()
    {
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
            AssertFootsteps(macro, ["begin", "failure"]);
        }

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
            AssertFootsteps(macro, ["begin", "success"]);
        }
    }

}
