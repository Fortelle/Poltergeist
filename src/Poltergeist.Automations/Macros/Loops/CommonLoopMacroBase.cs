using Poltergeist.Automations.Components.Hooks;
using Poltergeist.Automations.Modules;
using Poltergeist.Automations.Processors;

namespace Poltergeist.Automations.Macros.Loops;

[ModuleDependency<LoopModule>]
[ModuleDependency<LoopConfiguralizationModule>]
[ModuleDependency<LoopVisualizationModule>]
public abstract class CommonLoopMacroBase : CommonMacroBase
{
    protected CommonLoopMacroBase(string? name = null) : base(name)
    {
    }

    protected virtual Task<bool> OnStartAsync(WorkflowController controller)
    {
        return Task.FromResult(true);
    }

    protected virtual Task OnExecuteAsync(WorkflowController controller, IterationContext context)
    {
        return Task.CompletedTask;
    }

    protected virtual Task OnTransitionAsync(WorkflowController controller, TransitionContext context)
    {
        return Task.CompletedTask;
    }

    protected virtual Task OnEndAsync(WorkflowController controller)
    {
        return Task.CompletedTask;
    }

    [MacroHook]
    public void OnLoopSetup(IMacroProcessorShared _, LoopSetupHook hooks)
    {
        hooks.StartAsync = async (processor) =>
        {
            var controller = processor.GetService<WorkflowController>();
            return await OnStartAsync(controller);
        };

        hooks.ExecuteAsync = async (processor, context) =>
        {
            var controller = processor.GetService<WorkflowController>();
            await OnExecuteAsync(controller, context);
        };

        hooks.TransitionAsync = async (processor, context) =>
        {
            var controller = processor.GetService<WorkflowController>();
            await OnTransitionAsync(controller, context);
        };

        hooks.EndAsync = async (processor) =>
        {
            var controller = processor.GetService<WorkflowController>();
            await OnEndAsync(controller);
        };
    }
}
