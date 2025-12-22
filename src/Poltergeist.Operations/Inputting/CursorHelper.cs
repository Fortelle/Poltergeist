using System.Drawing;

namespace Poltergeist.Operations.Inputting;

public static class CursorHelper
{
    public static Point[] GetLinearPositions(Point begin, Point end)
    {
        var xd = end.X - begin.X;
        var yd = end.Y - begin.Y;

        var distance = Math.Sqrt(xd * xd + yd * yd);
        var steps = (int)Math.Ceiling(distance / 15);

        var xi = xd / steps;
        var yi = yd / steps;

        var points = new Point[steps];

        for (var i = 0; i < steps; i++)
        {
            var x = xi * i + begin.X;
            var y = yi * i + begin.Y;
            points[i] = new Point((int)x, (int)y);
        }

        return points;
    }
}
