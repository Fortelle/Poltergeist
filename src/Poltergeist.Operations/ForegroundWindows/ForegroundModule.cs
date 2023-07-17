using Microsoft.Extensions.DependencyInjection;
using Poltergeist.Automations.Macros;
using Poltergeist.Automations.Processors;
using Poltergeist.Common.Utilities.Maths;
using Poltergeist.Operations.Macros;
using Poltergeist.Operations.Timers;

namespace Poltergeist.Operations.ForegroundWindows;

public class ForegroundModule : MacroModule
{
    public override void OnMacroConfiguring(ServiceCollection services, IConfigureProcessor processor)
    {
        services.AddTransient<ForegroundOperator>();

        services.AddTransient<ForegroundLocatingService>();
        services.AddTransient<ForegroundCapturingService>();
        services.AddTransient<ForegroundMouseService>();
        services.AddTransient<ForegroundKeyboardService>();
        services.AddTransient<TimerService>();

        services.AddSingleton<RandomEx>();
        services.AddTransient<DistributionService>();
    }
}
