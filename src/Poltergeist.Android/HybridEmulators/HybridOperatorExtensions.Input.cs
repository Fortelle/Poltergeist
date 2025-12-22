using System.Drawing;
using Poltergeist.Automations.Structures.Shapes;
using Poltergeist.Operations.Inputting;

namespace Poltergeist.Android.HybridEmulators;

public static partial class HybridOperatorExtensions
{
    public static IHybridOperator Tap(this IHybridOperator @operator, Rectangle targetRectangle)
    {
        @operator.Hand.Tap(new ShapePosition(targetRectangle));
        return @operator;
    }

    public static IHybridOperator Tap(this IHybridOperator @operator, IShape targetShape)
    {
        @operator.Hand.Tap(new ShapePosition(targetShape));
        return @operator;
    }

    public static IHybridOperator Tap(this IHybridOperator @operator, Point targetPoint)
    {
        @operator.Hand.Tap(new CoarsePoint(targetPoint));
        return @operator;
    }

    public static IHybridOperator Tap(this IHybridOperator @operator)
    {
        @operator.Hand.Tap(new LastPoint());
        return @operator;
    }


    public static IHybridOperator LongTap(this IHybridOperator @operator, Rectangle targetRectangle)
    {
        @operator.Hand.LongTap(new ShapePosition(targetRectangle));
        return @operator;
    }

    public static IHybridOperator LongTap(this IHybridOperator @operator, IShape targetShape)
    {
        @operator.Hand.LongTap(new ShapePosition(targetShape));
        return @operator;
    }

    public static IHybridOperator LongTap(this IHybridOperator @operator, Point targetPoint)
    {
        @operator.Hand.LongTap(new CoarsePoint(targetPoint));
        return @operator;
    }

    public static IHybridOperator LongTap(this IHybridOperator @operator)
    {
        @operator.Hand.LongTap(new LastPoint());
        return @operator;
    }

}
