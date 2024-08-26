using System.Drawing;
using Microsoft.Extensions.Options;
using Poltergeist.Automations.Processors;
using Poltergeist.Automations.Services;
using Poltergeist.Automations.Structures.Shapes;
using Poltergeist.Automations.Utilities.Maths;

namespace Poltergeist.Android.Adb;

public class AdbInputService : MacroService
{
    public AdbService Adb { get; }

    private readonly DistributionService Distribution;
    private readonly AdbInputOptions DefaultOptions;

    public AdbInputService(MacroProcessor processor,
        AdbService adb,
        DistributionService distribution,
        IOptions<AdbInputOptions> options
        ) : base(processor)
    {
        Adb = adb;
        Distribution = distribution;
        DefaultOptions = options.Value;
    }

    #region "Tap"
    public void Tap(Point targetPoint)
    {
        //Logger.Debug($"Simulating tap: {{{targetPoint}}}.");

        var point = GetPoint(targetPoint);
        DoTap(point);
    }

    public void Tap(IShape targetShape)
    {
        //Logger.Debug($"Simulating tap: {{{targetShape}}}.");

        var point = GetPoint(targetShape);
        DoTap(point);
    }

    public void Tap(Rectangle targetRectangle)
    {
        //Logger.Debug($"Simulating tap: {{{targetRectangle}}}.");

        var point = GetPoint(targetRectangle);
        DoTap(point);
    }

    public void Tap(int x, int y)
    {
        Tap(new Point(x, y));
    }
    #endregion

    #region "LongTap"
    public void LongTap(Point targetPoint)
    {
        //Logger.Debug($"Simulating long-tap: {{{targetPoint}}}.");

        var point = GetPoint(targetPoint);
        DoLongTap(point);
    }

    public void LongTap(IShape targetShape)
    {
        //Logger.Debug($"Simulating long-tap: {{{targetShape}}}.");

        var point = GetPoint(targetShape);
        DoLongTap(point);
    }

    public void LongTap(Rectangle targetRectangle)
    {
        //Logger.Debug($"Simulating long-tap: {{{targetRectangle}}}.");

        var point = GetPoint(new RectangleShape(targetRectangle));
        DoLongTap(point);
    }

    public void LongTap(int x, int y)
    {
        LongTap(new Point(x, y));
    }
    #endregion


    public void Swipe(Point p1, Point p2)
    {
        //Logger.Debug($"Simulating swipe: {{{p1}}} to {{{p2}}}.");

        DoSwipe(p1, p2);
    }

    public void Swipe(IShape targetShape1, IShape targetShape2)
    {
        //Logger.Debug($"Simulating swipe: {{{targetShape1}}} to {{{targetShape2}}}.");

        var point1 = GetPoint(targetShape1);
        var point2 = GetPoint(targetShape2);
        DoSwipe(point1, point2);
    }

    public void Swipe(Rectangle targetRectangle1, Rectangle targetRectangle2)
    {
        //Logger.Debug($"Simulating swipe: {{{targetRectangle1}}} to {{{targetRectangle2}}}.");

        var point1 = GetPoint(targetRectangle1);
        var point2 = GetPoint(targetRectangle2);
        DoSwipe(point1, point2);
    }

    public void Swipe(int x1, int x2, int y1, int y2)
    {
        Swipe(new Point(x1, y1), new Point(x2, y2));
    }


    public Point GetPoint(Point targetPoint)
    {
        var offsetRange = DefaultOptions?.PointOffsetRange ?? 0;
        var point = Distribution.GetPointByOffset(targetPoint, offsetRange);
        return point;
    }

    public Point GetPoint(IShape targetShape)
    {
        var distribution = DefaultOptions?.ShapeDistribution ?? default;
        var point = Distribution.GetPointByShape(targetShape, distribution);
        return point;
    }

    public Point GetPoint(Rectangle targetRectangle) => GetPoint(new RectangleShape(targetRectangle));

    public void DoTap(Point targetPoint)
    {
        Adb.Execute($"shell input tap {targetPoint.X} {targetPoint.Y}");

        Logger.Debug($"Simulated tap: {{{targetPoint}}}.");

    }

    public void DoLongTap(Point targetPoint)
    {
        var (min, max) = DefaultOptions?.LongPressTime ?? (3000, 3000);
        var duration = Distribution.Random.Next(min, max);
        Adb.Execute($"shell input swipe {targetPoint.X} {targetPoint.Y} {targetPoint.X} {targetPoint.Y} {duration}");

        Logger.Debug($"Simulated long-tap: {{{targetPoint}}}.", new { duration });
    }

    public void DoSwipe(Point p1, Point p2)
    {
        var (min, max) = DefaultOptions?.SwipeTime ?? (0, 0);
        var duration = max == 0 ? 0 : Distribution.Random.Next(min, max);
        if (duration == 0)
        {
            Adb.Execute($"shell input swipe {p1.X} {p1.Y} {p2.X} {p2.Y}");
        }
        else
        {
            Adb.Execute($"shell input swipe {p1.X} {p1.Y} {p2.X} {p2.Y} {duration}");
        }
        Logger.Debug($"Simulated swipe: {{{p1}}} to {{{p2}}}.", new { duration });
    }

}
