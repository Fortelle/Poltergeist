using Microsoft.Extensions.DependencyInjection;
using Poltergeist.Automations.Macros;
using Poltergeist.Automations.Processors;
using Poltergeist.Automations.Structures.Parameters;
using Poltergeist.Automations.Utilities;

namespace Poltergeist.Automations.Components.Loops;

public class LoopModule : MacroModule
{
    private readonly LoopOptions Options;

    public LoopModule()
    {
        Options = new();
    }

    public LoopModule(LoopOptions options)
    {
        Options = options;
    }

    public override void OnMacroInitialize(IInitializableMacro macro)
    {
        base.OnMacroInitialize(macro);

        macro.StatisticDefinitions.Add(new StatisticDefinition<int>(LoopService.StatisticTotalIterationCountKey)
        {
            DisplayLabel = LocalizationUtil.Localize("Statistic_TotalIterationCount"),
            TargetKey = LoopService.ReportIterationCountKey,
            Update = (total, next) => total + next,
        });

        macro.OptionDefinitions.Add(new OptionDefinition<bool>(LoopService.ConfigEnableKey, true)
        {
            DisplayLabel = LocalizationUtil.Localize("Loops_Option_Enable"),
            Category = LocalizationUtil.Localize("Loops_Category"),
        });

        if (Options?.IsCountLimitable == true)
        {
            macro.OptionDefinitions.Add(new OptionDefinition<int>(LoopService.ConfigCountKey, Options?.DefaultCount ?? 1)
            {
                DisplayLabel = LocalizationUtil.Localize("Loops_Option_Count"),
                Category = LocalizationUtil.Localize("Loops_Category"),
            });
        }

        if (Options?.IsDurationLimitable == true)
        {
            macro.OptionDefinitions.Add(new OptionDefinition<TimeOnly>(LoopService.ConfigDurationKey, Options?.DefaultDuration ?? default)
            {
                DisplayLabel = LocalizationUtil.Localize("Loops_Option_Duration"),
                Category = LocalizationUtil.Localize("Loops_Category"),
            });
        }
    }

    public override void OnProcessorConfigure(IConfigurableProcessor processor)
    {
        base.OnProcessorConfigure(processor);

        processor.Services.AddSingleton<LoopService>();
        processor.Services.AddTransient<LoopBeforeArguments>();
        processor.Services.AddTransient<IterationArguments>();
        processor.Services.AddTransient<LoopCheckContinueArguments>();
    }

    public override void OnProcessorPrepare(IPreparableProcessor processor)
    {
        base.OnProcessorPrepare(processor);

        processor.SessionStorage.TryAdd("loop-options", Options);
    }
}
