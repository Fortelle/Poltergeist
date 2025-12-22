using Poltergeist.Automations.Utilities.Maths;
using Poltergeist.Operations.Timers;

namespace Poltergeist.Operations.Inputting;

public class MouseInputOptions : InputOptions
{
    public const int MouseMoveInterval = 15;
    public const int PointInShapeMaxRetry = 100;
    public const int MouseWheelDelta = 120;

    public TimeSpanRange? MouseDownUpInterval { get; set; }
    public TimeSpanRange? DoubleClickInterval { get; set; }

    public MouseMoveMotion? Motion { get; set; }
    public bool? KeepUnmovedInShape { get; set; }
    public int? MaxDeviationRadius { get; set; }
    public ShapeDistributionType? DeviationDistribution { get; set; }
    public ShapeDistributionType? ShapeDistribution { get; set; }

    public TimeSpanRange? VerticalWheelInterval { get; set; }
    public TimeSpanRange? HorizontalWheelInterval { get; set; }
}
