using Poltergeist.Automations.Components.Hooks;
using Poltergeist.Automations.Macros.Loops;
using Poltergeist.Automations.Processors;
using Poltergeist.Automations.Structures.Parameters;
using Poltergeist.Automations.Utilities;

namespace Poltergeist.Examples;

[ExampleMacro]
public class LoopMacroExample : LoopMacro
{
    public LoopMacroExample() : base()
    {
        Title = nameof(LoopMacro);

        Category = "Macros";

        Description = $"A macro that can be repeatedly executed.";

        OptionDefinitions.Add(new EnumOption<LoopInstrumentView>(nameof(LoopInstrumentView), LoopInstrumentView.List)
        {
            DisplayLabel = "Instrument Style",
            Category = LocalizationUtil.Localize("Loops_Category"),
        });

        ExecuteAsync = async (args, index) =>
        {
            await Task.Delay(1000, args.Processor.CancellationToken);
        };

        OptionPresets =
        [
            LoopConfiguralizationModule.PatternDefinition.WithValue(LoopPattern.Multiple),
            LoopConfiguralizationModule.CountDefinition.WithValue(10),
        ];
    }

    [MacroHook]
    private static void OnProcessorStartup(IMacroProcessorShared processor, ProcessorStartupHook hook)
    {
        var instrumentView = processor.Options.Get<LoopInstrumentView>(nameof(LoopInstrumentView));
        processor.SessionStorage.AddOrUpdate(LoopVisualizationModule.ViewDefinition, instrumentView);
    }
}
