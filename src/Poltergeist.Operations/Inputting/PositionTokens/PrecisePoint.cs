using System.Drawing;

namespace Poltergeist.Operations.Inputting;

public class PrecisePoint : PositionToken
{
    public Point Location { get; }

    public PrecisePoint(Point point)
    {
        Location = point;
    }

    public PrecisePoint(int x, int y)
    {
        Location = new Point(x, y);
    }
}
