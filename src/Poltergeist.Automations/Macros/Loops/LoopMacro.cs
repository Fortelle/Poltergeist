using Poltergeist.Automations.Components.Hooks;
using Poltergeist.Automations.Modules;
using Poltergeist.Automations.Processors;

namespace Poltergeist.Automations.Macros.Loops;

[ModuleDependency<LoopModule>]
[ModuleDependency<LoopConfiguralizationModule>]
[ModuleDependency<LoopVisualizationModule>]
public class LoopMacro : CommonMacroBase
{
    public Func<WorkflowController, bool>? Start { get; set; }
    public Func<WorkflowController, Task<bool>>? StartAsync { get; set; }

    public Action<WorkflowController, IterationContext>? Execute { get; set; }
    public Func<WorkflowController, IterationContext, Task>? ExecuteAsync { get; set; }

    public Action<WorkflowController, TransitionContext>? Transition { get; set; }
    public Func<WorkflowController, TransitionContext, Task>? TransitionAsync { get; set; }

    public Action<WorkflowController>? End { get; set; }
    public Func<WorkflowController, Task>? EndAsync { get; set; }

    public LoopMacro(string? name = null) : base(name)
    {
    }

    [MacroHook]
    public void OnLoopSetup(IMacroProcessorShared processor, LoopSetupHook hooks)
    {
        if (Start is not null || StartAsync is not null)
        {
            hooks.StartAsync = async (processor) =>
            {
                var controller = processor.GetService<WorkflowController>();
                return Start?.Invoke(controller) ?? await StartAsync!.Invoke(controller);
            };
        }

        if (Execute is not null || ExecuteAsync is not null)
        {
            hooks.ExecuteAsync = async (processor, context) =>
            {
                var controller = processor.GetService<WorkflowController>();
                if (Execute is not null)
                {
                    Execute.Invoke(controller, context);
                }
                else if (ExecuteAsync is not null)
                {
                    await ExecuteAsync.Invoke(controller, context);
                }
            };
        }

        if (Transition is not null || TransitionAsync is not null)
        {
            hooks.TransitionAsync = async (processor, context) =>
            {
                var controller = processor.GetService<WorkflowController>();
                if (Transition is not null)
                {
                    Transition.Invoke(controller, context);
                }
                else if (TransitionAsync is not null)
                {
                    await TransitionAsync.Invoke(controller, context);
                }
            };
        }

        if (End is not null || EndAsync is not null)
        {
            hooks.EndAsync = async (processor) =>
            {
                var controller = processor.GetService<WorkflowController>();
                if (End is not null)
                {
                    End.Invoke(controller);
                }
                else if (EndAsync is not null)
                {
                    await EndAsync.Invoke(controller);
                }
            };
        }
    }
}
