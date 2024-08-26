using Poltergeist.Automations.Utilities.Maths;
using Poltergeist.Operations;

namespace Poltergeist.Android.Adb;

public class AdbInputOptions : InputOptions
{
    public (int Min, int Max)? LongPressTime { get; set; } = (3000, 3000);
    public (int Min, int Max)? SwipeTime { get; set; }
    public int? PointOffsetRange { get; set; }
    public ShapeDistributionType? ShapeDistribution { get; set; }
}
