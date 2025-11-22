using Microsoft.Extensions.DependencyInjection;
using Poltergeist.Automations.Macros;
using Poltergeist.Automations.Processors;
using Poltergeist.Automations.Utilities.Maths;
using Poltergeist.Operations.Timers;

namespace Poltergeist.Operations.Inputing;

public class InputOptionsModule : MacroModule
{
    public override void OnProcessorConfigure(IConfigurableProcessor processor)
    {
        base.OnProcessorConfigure(processor);

        processor.Services.Configure<DelayOptions>(options =>
        {
            options.Floating = true;
            options.FloatingRange = (1d, 1.5d);
            options.ApplyMinimalDelay = false;
            options.RangeDistribution = RangeDistributionType.Uniform;
            options.FloatDistribution = RangeDistributionType.Uniform;
        });
        processor.Services.Configure<MouseInputOptions>(options =>
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
        processor.Services.Configure<KeyboardInputOptions>(options =>
        {
            options.KeyDownUpInterval = TimeSpanRange.FromMilliseconds(50, 100);
            options.PressInterval = TimeSpanRange.FromMilliseconds(50, 100);
            options.Mode = KeyboardInputMode.Scancode;
        });
    }
}
