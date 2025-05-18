using Poltergeist.Automations.Utilities.Maths;

namespace Poltergeist.Operations.Foreground;

public class MouseInputOptions : InputOptions
{
    public (int Min, int Max)? ClickDuration { get; set; }
    public (int Min, int Max)? DoubleClickInterval { get; set; }

    public MouseMoveMotion? Motion { get; set; }
    public bool? KeepUnmovedInShape { get; set; }
    public int? PointOffsetRange { get; set; }
    public ShapeDistributionType? ShapeDistribution { get; set; }

    public (int Min, int Max)? VerticalWheelInterval { get; set; }
    public (int Min, int Max)? HorizontalWheelInterval { get; set; }
}
