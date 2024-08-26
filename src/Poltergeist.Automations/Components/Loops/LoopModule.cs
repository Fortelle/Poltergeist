using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
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

        macro.Statistics.Add(new ParameterDefinition<int>(LoopService.StatisticTotalIterationCountKey)
        {
            DisplayLabel = ResourceHelper.Localize("Poltergeist.Automations/Resources/Statistic_TotalIterationCount"),
        });

        macro.UserOptions.Add(new OptionDefinition<bool>(LoopService.ConfigEnableKey, true)
        {
            DisplayLabel = ResourceHelper.Localize("Poltergeist.Automations/Resources/Loops_Option_Enable"),
            Category = ResourceHelper.Localize("Poltergeist.Automations/Resources/Loops_Category"),
        });

        if (Options?.IsCountLimitable == true)
        {
            macro.UserOptions.Add(new OptionDefinition<int>(LoopService.ConfigCountKey, Options?.DefaultCount ?? 1)
            {
                DisplayLabel = ResourceHelper.Localize("Poltergeist.Automations/Resources/Loops_Option_Count"),
                Category = ResourceHelper.Localize("Poltergeist.Automations/Resources/Loops_Category"),
            });
        }

        if (Options?.IsDurationLimitable == true)
        {
            macro.UserOptions.Add(new OptionDefinition<TimeOnly>(LoopService.ConfigDurationKey, Options?.DefaultDuration ?? default)
            {
                DisplayLabel = ResourceHelper.Localize("Poltergeist.Automations/Resources/Loops_Option_Duration"),
                Category = ResourceHelper.Localize("Poltergeist.Automations/Resources/Loops_Category"),
            });
        }
    }

    public override void OnProcessorConfigure(IConfigurableProcessor processor)
    {
        base.OnProcessorConfigure(processor);

        processor.Services.AddSingleton<IOptions<LoopOptions>>(new OptionsWrapper<LoopOptions>(Options));

        processor.Services.AddTransient<LoopBeforeArguments>();
        processor.Services.AddSingleton<LoopService>();
    }

}
