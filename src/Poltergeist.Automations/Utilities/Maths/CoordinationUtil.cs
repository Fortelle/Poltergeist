using System.Drawing;

namespace Poltergeist.Automations.Utilities.Maths;

public static class CoordinationUtil
{
    public static Rectangle GetWholeRect(Point[] points)
    {
        var l = points.Min(x => x.X);
        var t = points.Min(x => x.Y);
        var r = points.Max(x => x.X) + 1;
        var b = points.Max(x => x.Y) + 1;
        return Rectangle.FromLTRB(l, t, r, b);
    }

    public static Rectangle GetWholeRect(Rectangle[] rectangles)
    {
        var l = rectangles.Min(x => x.X);
        var t = rectangles.Min(x => x.Y);
        var r = rectangles.Max(x => x.Right) + 1;
        var b = rectangles.Max(x => x.Bottom) + 1;
        return Rectangle.FromLTRB(l, t, r, b);
    }

    public static double GetDistance(float x1, float y1, float x2, float y2)
    {
        var w = x1 - x2;
        var h = y1 - y2;
        return Math.Sqrt(w * w + h * h);
    }

    public static double GetDistance(int x1, int y1, int x2, int y2)
    {
        var w = x1 - x2;
        var h = y1 - y2;
        return Math.Sqrt(w * w + h * h);
    }

    public static double GetDistance(Point pt1, Point pt2)
    {
        var w = pt1.X - pt2.X;
        var h = pt1.Y - pt2.Y;
        return Math.Sqrt(w * w + h * h);
    }

    public static double Angle(Point pt1, Point pt2)
    {
        return Math.Atan2(pt2.Y - pt1.Y, pt2.X - pt1.X) * 180 / Math.PI;
    }
}


