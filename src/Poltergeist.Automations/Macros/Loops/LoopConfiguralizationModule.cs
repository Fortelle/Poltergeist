using Poltergeist.Automations.Components.Hooks;
using Poltergeist.Automations.Modules;
using Poltergeist.Automations.Processors;
using Poltergeist.Automations.Structures;
using Poltergeist.Automations.Structures.Parameters;
using Poltergeist.Automations.Utilities;

namespace Poltergeist.Automations.Macros.Loops;

public class LoopConfiguralizationModule : MacroModule
{
    public static readonly EntryDefinition<LoopPattern> PatternDefinition = new("loop-pattern");
    public static readonly EntryDefinition<int> CountDefinition = new("loop-count");
    public static readonly EntryDefinition<TimeOnly> DurationDefinition = new("loop-duration");
    public static readonly EntryDefinition<int> TotalIterationDefinition = new("loop_total_iterations");

    public override void OnMacroInitialized(IMacroInformation macro)
    {
        base.OnMacroInitialized(macro);

        macro.StatisticDefinitions.Add(new StatisticDefinition<int>(TotalIterationDefinition.Key)
        {
            DisplayLabel = LocalizationUtil.Localize("Statistic_TotalIterationCount"),
            TargetKey = LoopService.ReportIterationDefinition.Key,
            Update = (total, next) => total + next,
        });

        macro.OptionDefinitions.Add(new OptionDefinition<LoopPattern>(PatternDefinition.Key, LoopPattern.Once)
        {
            Category = LocalizationUtil.Localize("Loops_Category"),
        });

        macro.OptionDefinitions.Add(new OptionDefinition<int>(CountDefinition.Key, 1)
        {
            DisplayLabel = LocalizationUtil.Localize("Loops_Option_Count"),
            Category = LocalizationUtil.Localize("Loops_Category"),
        });

        macro.OptionDefinitions.Add(new OptionDefinition<TimeOnly>(DurationDefinition.Key)
        {
            DisplayLabel = LocalizationUtil.Localize("Loops_Option_Duration"),
            Category = LocalizationUtil.Localize("Loops_Category"),
        });

        macro.Interventions.Add(new ProcessorIntervention()
        {
            Key = "stop_after_current",
            Title = LocalizationUtil.Localize("Loops_StopAfterCurrent_Title"),
            Icon = IconInfo.FromGlyph("\uE71A"),
            Description = LocalizationUtil.Localize("Loops_StopAfterCurrent_Description"),
            Message = LocalizationUtil.Localize("Loops_StopAfterCurrent_Message"),
            Variables =
            [
                LoopService.StopAfterCurrentDefinition.WithValue(true),
            ],
        });
    }

    [MacroHook]
    public static void OnLoopSetup(IMacroProcessorShared processor, LoopSetupHook hooks)
    {
        hooks.Pattern = processor.Options.GetValueOrDefault(PatternDefinition, LoopPattern.Once);
        hooks.MaxIterations = processor.Options.GetValueOrDefault(CountDefinition, 0);
        hooks.MaxDuration = processor.Options.GetValueOrDefault(DurationDefinition).ToTimeSpan();
    }
}
