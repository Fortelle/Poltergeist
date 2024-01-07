using Microsoft.Extensions.DependencyInjection;
using Poltergeist.Automations.Macros;
using Poltergeist.Automations.Processors;
using Poltergeist.Common.Utilities.Maths;
using Poltergeist.Operations.Timers;

namespace Poltergeist.Operations.Foreground;

public class ForegroundModule : MacroModule
{
    public override void OnProcessorConfigure(IConfigurableProcessor processor)
    {
        base.OnProcessorConfigure(processor);

        processor.Services.AddSingleton<ForegroundOperator>();

        processor.Services.AddSingleton<ForegroundLocatingService>();
        processor.Services.AddSingleton<ForegroundCapturingService>();
        processor.Services.AddSingleton<ForegroundMouseService>();
        processor.Services.AddSingleton<ForegroundKeyboardService>();
        processor.Services.AddSingleton<TimerService>();

        processor.Services.AddSingleton<RandomEx>();
        processor.Services.AddSingleton<DistributionService>();
    }
}
