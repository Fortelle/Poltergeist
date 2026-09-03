using Microsoft.Extensions.DependencyInjection;
using Poltergeist.Automations.Modules;
using Poltergeist.Automations.Utilities.Maths;
using Poltergeist.Operations.Capturing;
using Poltergeist.Operations.Inputting;
using Poltergeist.Operations.Locating;
using Poltergeist.Operations.Timers;

namespace Poltergeist.Operations;

public class OperationModule : MacroModule
{
    public override void RegisterServices(IServiceCollection services, RegisterServicesArguments args)
    {
        base.RegisterServices(services, args);

        services.AddSingleton<ScreenLocatingService>();
        services.AddSingleton<WindowLocatingService>();

        services.AddSingleton<ScreenCapturingService>();
        services.AddSingleton<PrintWindowCapturingService>();
        services.AddSingleton<BitBltCapturingService>();

        services.AddSingleton<MouseSendInputService>();
        services.AddSingleton<MouseSendMessageService>();
        services.AddSingleton<DeviationService>();
        services.AddOptions<MouseInputOptions>();

        services.AddSingleton<KeyboardSendInputService>();
        services.AddSingleton<KeyboardSendMessageService>();
        services.AddOptions<KeyboardInputOptions>();

        services.AddSingleton<TimerService>();
        services.AddOptions<DelayOptions>();

        services.AddSingleton<RandomEx>();
        services.AddSingleton<DistributionService>();
    }
}
