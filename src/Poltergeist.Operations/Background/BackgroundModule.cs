using Microsoft.Extensions.DependencyInjection;
using Poltergeist.Automations.Macros;
using Poltergeist.Automations.Processors;
using Poltergeist.Common.Utilities.Maths;
using Poltergeist.Operations.Foreground;
using Poltergeist.Operations.Timers;

namespace Poltergeist.Operations.Background;

public class BackgroundModule : MacroModule
{
    public override void OnMacroConfiguring(ServiceCollection services, IConfigureProcessor processor)
    {
        services.AddSingleton<BackgroundOperator>();

        services.AddSingleton<BackgroundLocatingService>();
        services.AddSingleton<BackgroundKeyboardService>();
        services.AddSingleton<BackgroundMouseService>();
        services.AddSingleton<BackgroundCapturingService>();

        services.AddSingleton<TimerService>();

        services.AddSingleton<RandomEx>();
        services.AddSingleton<DistributionService>();
    }
}
