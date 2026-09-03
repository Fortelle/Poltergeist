using Microsoft.Extensions.DependencyInjection;
using Poltergeist.Automations.Modules;
using Poltergeist.Automations.Utilities.Maths;
using Poltergeist.Operations.Timers;

namespace Poltergeist.Operations.Inputting;

public class HumanizationOptionsModule : MacroModule
{
    public override void RegisterServices(IServiceCollection services, RegisterServicesArguments args)
    {
        base.RegisterServices(services, args);

        services.Configure<DelayOptions>(options =>
        {
            options.Floating = true;
            options.FloatingRange = (1d, 1.5d);
            options.RangeDistribution = RangeDistributionType.Uniform;
            options.FloatDistribution = RangeDistributionType.Uniform;
        });
        services.Configure<MouseInputOptions>(options =>
        {
            options.MouseDownUpInterval = TimeSpanRange.FromMilliseconds(50, 150);
            options.DoubleClickInterval = TimeSpanRange.FromMilliseconds(80, 110);
            options.Motion = MouseMoveMotion.Jump;
            options.KeepUnmovedInShape = true;
            options.MaxDeviationRadius = 16;
            options.DeviationDistribution = ShapeDistributionType.Gaussian;
            options.ShapeDistribution = ShapeDistributionType.Gaussian;
            options.VerticalWheelInterval = TimeSpanRange.FromMilliseconds(50, 150);
            options.HorizontalWheelInterval = TimeSpanRange.FromMilliseconds(50, 150);
        });
        services.Configure<KeyboardInputOptions>(options =>
        {
            options.KeyDownUpInterval = TimeSpanRange.FromMilliseconds(50, 100);
            options.PressInterval = TimeSpanRange.FromMilliseconds(50, 100);
            options.Mode = KeyboardInputMode.Scancode;
        });
    }
}
