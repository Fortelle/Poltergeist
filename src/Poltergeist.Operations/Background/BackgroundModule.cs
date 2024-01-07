using Microsoft.Extensions.DependencyInjection;
using Poltergeist.Automations.Macros;
using Poltergeist.Automations.Processors;
using Poltergeist.Common.Utilities.Maths;
using Poltergeist.Operations.Foreground;
using Poltergeist.Operations.Timers;

namespace Poltergeist.Operations.Background;

public class BackgroundModule : MacroModule
{
    public override void OnProcessorConfigure(IConfigurableProcessor processor)
    {
        base.OnProcessorConfigure(processor);

        processor.Services.AddSingleton<BackgroundOperator>();

        processor.Services.AddSingleton<BackgroundLocatingService>();
        processor.Services.AddSingleton<BackgroundKeyboardService>();
        processor.Services.AddSingleton<BackgroundMouseService>();
        processor.Services.AddSingleton<BackgroundCapturingService>();

        processor.Services.AddSingleton<TimerService>();

        processor.Services.AddSingleton<RandomEx>();
        processor.Services.AddSingleton<DistributionService>();
    }
}
