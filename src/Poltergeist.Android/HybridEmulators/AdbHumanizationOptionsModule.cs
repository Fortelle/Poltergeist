using Microsoft.Extensions.DependencyInjection;
using Poltergeist.Android.Adb;
using Poltergeist.Automations.Modules;
using Poltergeist.Automations.Utilities.Maths;
using Poltergeist.Operations.Inputting;
using Poltergeist.Operations.Timers;

namespace Poltergeist.Android.HybridEmulators;

public class AdbHumanizationOptionsModule : MacroModule
{
    public override void RegisterServices(IServiceCollection services, RegisterServicesArguments args)
    {
        base.RegisterServices(services, args);

        services.Configure<AdbInputOptions>(options =>
        {
            options.SwipeTime = TimeSpanRange.FromMilliseconds(1000, 1500);
            options.MaxDeviationRadius = 32;
            options.DeviationDistribution = ShapeDistributionType.Gaussian;
            options.ShapeDistribution = ShapeDistributionType.Gaussian;
            options.MovingMotion = MouseMoveMotion.Linear;
            options.MovingInterval = 15;
        });
    }
}
