using Microsoft.Extensions.DependencyInjection;
using Poltergeist.Automations.Macros;
using Poltergeist.Automations.Processors;
using Poltergeist.Automations.Services;
using Poltergeist.Common.Utilities.Maths;
using Poltergeist.Operations.ForegroundWindows;
using Poltergeist.Operations.Timers;

namespace Poltergeist.Operations;

public class InputOptionsModule : MacroModule
{
    // todo: global options
    public override void OnMacroConfigure(MacroServiceCollection services, IConfigureProcessor processor)
    {
        services.Configure<DelayOptions>(options =>
        {
            options.Floating = true;
            options.FloatingRange = (1d, 1.5d);
            options.ApplyMinimalDelay = false;
            options.RangeDistribution = RangeDistributionType.Uniform;
            options.FloatDistribution = RangeDistributionType.Uniform;
        });
        services.Configure<MouseInputOptions>(options =>
        {
            options.ClickTime = (50, 150);
            options.DoubleClickTime = (80, 110);
            options.Motion = MouseMoveMotion.Jump;
            options.UnmoveInShape = true;
            options.PointOffsetRange = 16;
            options.ShapeDistribution = ShapeDistributionType.Inclination;
            options.VerticalWheelInterval = (50, 150);
            options.HorizonWheelInterval = (50, 150);
        });
        services.Configure<KeyboardInputOptions>(options =>
        {
            options.PressTime = (50, 100);
            options.PressInterval = (50, 100);
        });
    }
}
