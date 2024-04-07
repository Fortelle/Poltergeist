using Poltergeist.Automations.Components.Interactions;
using Poltergeist.Automations.Macros;
using Poltergeist.Automations.Parameters;
using Poltergeist.Automations.Processors;

namespace Poltergeist.Automations.Components;

public class MinimizationModule : MacroModule
{
    public bool IsConfigurable { get; set; }

    public MinimizationModule()
    {

    }

    public MinimizationModule(bool isConfigurable)
    {
        IsConfigurable = isConfigurable;
    }

    public override void OnMacroInitialize(IInitializableMacro macro)
    {
        base.OnMacroInitialize(macro);

        macro.UserOptions.Add(new OptionDefinition<bool>("minimization", true)
        {
            DisplayLabel = "Minimize application",
            Status = IsConfigurable ? ParameterStatus.Normal : ParameterStatus.Hidden,
        });
    }

    public override void OnProcessorPrepare(IPreparableProcessor processor)
    {
        base.OnProcessorPrepare(processor);

        processor.Hooks.Register<ProcessorStartedHook>(OnProcessorStarted);
        processor.Hooks.Register<ProcessorEndingHook>(OnProcessorEnding);
    }

    private void OnProcessorStarted(ProcessorStartedHook hook, IUserProcessor processor)
    {
        if (!processor.Options.Get<bool>("minimization"))
        {
            return;
        }

        var model = new AppWindowModel(AppWindowAction.Minimize);
        processor.Interact(model);
    }

    private void OnProcessorEnding(ProcessorEndingHook hook, IUserProcessor processor)
    {
        if (!processor.Options.Get<bool>("minimization"))
        {
            return;
        }

        var model = new AppWindowModel(AppWindowAction.Restore);
        processor.Interact(model);
    }

}
