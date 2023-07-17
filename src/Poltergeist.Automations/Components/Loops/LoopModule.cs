using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Poltergeist.Automations.Components.Repetitions;
using Poltergeist.Automations.Configs;
using Poltergeist.Automations.Macros;
using Poltergeist.Automations.Processors;

namespace Poltergeist.Automations.Components.Loops;

public class LoopModule : MacroModule
{
    public LoopOptions Options { get; }

    public LoopModule()
    {
        Options = new();
    }

    public LoopModule(LoopOptions options)
    {
        Options = options;
    }

    public override void OnMacroInitialized(IMacroInitializer macro)
    {
        macro.Statistics.Add(new(LoopService.StatisticTotalIterationCountKey, 0));

        macro.UserOptions.Add(new OptionItem<bool>(LoopService.ConfigEnableKey, true)
        {
            DisplayLabel = "Enable",
            Category = "Loops",
        });

        if (Options?.IsCountLimitable == true)
        {
            macro.UserOptions.Add(new OptionItem<int>(LoopService.ConfigCountKey, Options?.DefaultCount ?? 1)
            {
                DisplayLabel = "Count",
                Category = "Loops",
            });
        }

        if (Options?.IsDurationLimitable == true)
        {
            macro.UserOptions.Add(new OptionItem<int>(LoopService.ConfigDurationKey, Options?.DefaultDuration ?? 0)
            {
                DisplayLabel = "Duration (seconds)",
                Category = "Loops",
            });
        }

    }

    public override void OnMacroConfiguring(ServiceCollection services, IConfigureProcessor processor)
    {
        base.OnMacroConfiguring(services, processor);

        services.AddSingleton<IOptions<LoopOptions>>(new OptionsWrapper<LoopOptions>(Options));

        services.AddTransient<LoopBeforeArguments>();
        services.AddTransient<LoopExecutionArguments>();
        services.AddTransient<LoopCheckContinueArguments>();
        services.AddSingleton<LoopService>();
    }

}
