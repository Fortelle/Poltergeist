using System.Drawing;
using Poltergeist.Automations.Utilities.Windows;

namespace Poltergeist.Operations.Foreground;

public static class CursorHelper
{
    public static Point Location
    {
        get => SendInputHelper.Cursor;
        set => SendInputHelper.Cursor = value;
    }

    public static void MoveTo(Point screenPoint, MouseMoveMotion motion)
    {
        switch (motion)
        {
            case MouseMoveMotion.Jump:
                Location = screenPoint;
                break;
            case MouseMoveMotion.Linear:
                var current = SendInputHelper.Cursor;
                var points = MoveLinear(current, screenPoint);
                foreach (var point in points)
                {
                    Location = point;
                }
                break;
            default:
                throw new NotImplementedException();
        }
    }

    public static IEnumerable<Point> MoveLinear(Point begin, Point end)
    {
        var xd = end.X - begin.X;
        var yd = end.Y - begin.Y;

        var distance = Math.Sqrt(xd * xd + yd * yd);
        var steps = Math.Ceiling(distance / 15);

        var xi = xd / steps;
        var yi = yd / steps;

        for (var i = 0; i < steps; i++)
        {
            var point = new Point((int)(xi * i + begin.X), (int)(yi * i + begin.Y));
            if (i > 0)
            {
                DoDelay(15);
            }

            yield return point;
        }
    }

    private static void DoDelay(int timeout)
    {
        if (timeout == 0)
        {
            return;
        }

        Thread.Sleep(timeout);
    }

}
