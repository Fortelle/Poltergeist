using System.Drawing;

namespace Poltergeist.Operations.Foreground;

public static class CursorHelper
{
    public static IEnumerable<Point> GetLinearPositions(Point begin, Point end)
    {
        var xd = end.X - begin.X;
        var yd = end.Y - begin.Y;

        var distance = Math.Sqrt(xd * xd + yd * yd);
        var steps = Math.Ceiling(distance / 15);

        var xi = xd / steps;
        var yi = yd / steps;

        for (var i = 0; i < steps; i++)
        {
            var x = xi * i + begin.X;
            var y = yi * i + begin.Y;
            yield return new Point((int)x, (int)y);
        }
    }

}
