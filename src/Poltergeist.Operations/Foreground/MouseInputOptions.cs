﻿using Poltergeist.Automations.Utilities.Maths;

namespace Poltergeist.Operations.Foreground;

public class MouseInputOptions : InputOptions
{
    public (int Min, int Max)? ClickTime { get; set; }
    public (int Min, int Max)? DoubleClickTime { get; set; }

    public MouseMoveMotion? Motion { get; set; }
    public bool? UnmoveInShape { get; set; }
    public int? PointOffsetRange { get; set; }
    public ShapeDistributionType? ShapeDistribution { get; set; }

    public (int Min, int Max)? VerticalWheelInterval { get; set; }
    public (int Min, int Max)? HorizonWheelInterval { get; set; }
}
