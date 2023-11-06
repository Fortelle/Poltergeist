using Microsoft.Extensions.DependencyInjection;
using Poltergeist.Automations.Macros;
using Poltergeist.Automations.Processors;
using Poltergeist.Common.Utilities.Maths;
using Poltergeist.Operations.Timers;

namespace Poltergeist.Operations.Foreground;

public class ForegroundModule : MacroModule
{
    public override void OnMacroConfiguring(ServiceCollection services, IConfigureProcessor processor)
    {
        services.AddSingleton<ForegroundOperator>();

        services.AddSingleton<ForegroundLocatingService>();
        services.AddSingleton<ForegroundCapturingService>();
        services.AddSingleton<ForegroundMouseService>();
        services.AddSingleton<ForegroundKeyboardService>();
        services.AddSingleton<TimerService>();

        services.AddSingleton<RandomEx>();
        services.AddSingleton<DistributionService>();
    }
}
