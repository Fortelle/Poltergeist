using Poltergeist.Automations.Components.Hooks;
using Poltergeist.Automations.Components.Interactions;
using Poltergeist.Automations.Macros;
using Poltergeist.Automations.Modules;
using Poltergeist.Automations.Processors;
using Poltergeist.Automations.Structures.Parameters;

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

    public override void OnMacroInitialized(IMacroInformation macro)
    {
        base.OnMacroInitialized(macro);

        macro.OptionDefinitions.Add(new OptionDefinition<bool>("minimization", true)
        {
            DisplayLabel = "Minimize application",
            Status = IsConfigurable ? ParameterStatus.Normal : ParameterStatus.Hidden,
        });
    }

    [MacroHook]
    public static void OnProcessorStarted(IMacroProcessorShared processor, ProcessorStartupHook hook)
    {
        if (!processor.Options.GetValueOrDefault<bool>("minimization"))
        {
            return;
        }

        var model = new AppWindowModel(AppWindowAction.Minimize);
        processor.GetService<InteractionService>().Push(model);
    }

    [MacroHook]
    public static void OnProcessorCompleted(IMacroProcessorShared processor, ProcessorCompletedHook hook)
    {
        if (!processor.Options.GetValueOrDefault<bool>("minimization"))
        {
            return;
        }

        var model = new AppWindowModel(AppWindowAction.Restore);
        processor.GetService<InteractionService>().Push(model);
    }

}
