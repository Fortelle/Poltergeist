using Poltergeist.Automations.Utilities.Maths;
using Poltergeist.Operations.Inputing;
using Poltergeist.Operations.Timers;

namespace Poltergeist.Android.Adb;

public class AdbInputOptions : InputOptions
{
    public TimeSpanRange? LongPressTime { get; set; } = TimeSpanRange.FromMilliseconds(3000, 3000);
    public TimeSpanRange? SwipeTime { get; set; }
    public int? MaxDeviationRadius { get; set; }
    public ShapeDistributionType? DeviationDistribution { get; set; }
    public ShapeDistributionType? ShapeDistribution { get; set; }
    public MouseMoveMotion? MovingMotion { get; set; }
    public int? MovingInterval { get; set; }
}
