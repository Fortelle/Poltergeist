using Microsoft.Extensions.DependencyInjection;
using Poltergeist.Automations.Macros;
using Poltergeist.Automations.Processors;
using Poltergeist.Automations.Utilities.Maths;
using Poltergeist.Operations.Capturing;
using Poltergeist.Operations.Inputting;
using Poltergeist.Operations.Locating;
using Poltergeist.Operations.Timers;

namespace Poltergeist.Operations;

public class OperationModule : MacroModule
{
    public override void OnProcessorConfigure(IConfigurableProcessor processor)
    {
        base.OnProcessorConfigure(processor);

        processor.Services.AddSingleton<ScreenLocatingService>();
        processor.Services.AddSingleton<WindowLocatingService>();

        processor.Services.AddSingleton<ScreenCapturingService>();
        processor.Services.AddSingleton<PrintWindowCapturingService>();
        processor.Services.AddSingleton<BitBltCapturingService>();

        processor.Services.AddSingleton<MouseSendInputService>();
        processor.Services.AddSingleton<MouseSendMessageService>();
        processor.Services.AddSingleton<DeviationService>();
        processor.Services.AddOptions<MouseInputOptions>();

        processor.Services.AddSingleton<KeyboardSendInputService>();
        processor.Services.AddSingleton<KeyboardSendMessageService>();
        processor.Services.AddOptions<KeyboardInputOptions>();

        processor.Services.AddSingleton<TimerService>();
        processor.Services.AddOptions<DelayOptions>();

        processor.Services.AddSingleton<RandomEx>();
        processor.Services.AddSingleton<DistributionService>();
    }
}
