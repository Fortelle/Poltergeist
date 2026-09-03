using Poltergeist.Automations.Components.Hooks;
using Poltergeist.Automations.Modules;
using Poltergeist.Automations.Processors;

namespace Poltergeist.Automations.Macros.Oneshots;

[ModuleDependency<OneshotModule>]
[ModuleDependency<OneshotVisualizationModule>]
public abstract class CommonOneshotMacroBase : CommonMacroBase
{
    public bool ShowStatusBar { get; set; }

    protected CommonOneshotMacroBase(string? name = null) : base(name)
    {
    }

    protected override void OnProcessorCreated(IMacroProcessorInformation processor)
    {
        if (!ShowStatusBar)
        {
            processor.SessionStorage.Add("oneshot_visualization_disabled", true);
        }
    }

    protected virtual void OnExecute(WorkflowController controller)
    {
    }

    protected virtual Task OnExecuteAsync(WorkflowController controller)
    {
        return Task.CompletedTask;
    }

    [MacroHook]
    public void OnOneshotSetup(IMacroProcessorShared processor, OneshotSetupHook hooks)
    {
        hooks.ExecuteAsync = async (processor) =>
        {
            var controller = processor.GetService<WorkflowController>();
            OnExecute(controller);
            await OnExecuteAsync(controller);
        };
    }
}
