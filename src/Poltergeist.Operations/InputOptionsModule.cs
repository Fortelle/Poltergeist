using Microsoft.Extensions.DependencyInjection;
using Poltergeist.Automations.Macros;
using Poltergeist.Automations.Processors;
using Poltergeist.Automations.Utilities.Maths;
using Poltergeist.Operations.Foreground;
using Poltergeist.Operations.Timers;

namespace Poltergeist.Operations;

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
            options.ClickDuration = (50, 150);
            options.DoubleClickInterval = (80, 110);
            options.Motion = MouseMoveMotion.Jump;
            options.KeepUnmovedInShape = true;
            options.PointOffsetRange = 16;
            options.ShapeDistribution = ShapeDistributionType.Central;
            options.VerticalWheelInterval = (50, 150);
            options.HorizontalWheelInterval = (50, 150);
        });
        processor.Services.Configure<KeyboardInputOptions>(options =>
        {
            options.PressDuration = (50, 100);
            options.PressInterval = (50, 100);
            options.Mode = KeyboardInputMode.Scancode;
        });
    }
}
