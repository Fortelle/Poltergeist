using System.Drawing;
using Poltergeist.Automations.Utilities.Maths;

namespace Poltergeist.Operations.Inputing;

public class CoarsePoint : PositionToken
{
    public Point Location { get; }

    public int? MaxDeviationRadius { get; init; }

    public ShapeDistributionType? DeviationDistribution { get; init; }

    public bool? KeepUnmovedInShape { get; init; }

    public CoarsePoint(Point point)
    {
        Location = point;
    }

    public CoarsePoint(int x, int y)
    {
        Location = new Point(x, y);
    }
}
