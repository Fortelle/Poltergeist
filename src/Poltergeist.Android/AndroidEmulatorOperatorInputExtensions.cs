using System.Drawing;
using Poltergeist.Automations.Structures.Shapes;

namespace Poltergeist.Operations.Macros;

public static class AndroidEmulatorOperatorInputExtensions
{
    public static IOperator MoveTo(this IOperator @operator, Rectangle targetRectangle)
    {
        @operator.Hand.MoveTo(targetRectangle);
        return @operator;
    }

    public static IOperator MoveTo(this IOperator @operator, IShape targetShape)
    {
        @operator.Hand.MoveTo(targetShape);
        return @operator;
    }

    public static IOperator MoveTo(this IOperator @operator, Point targetPoint)
    {
        @operator.Hand.MoveTo(targetPoint);
        return @operator;
    }

    public static IOperator MoveTo(this IOperator @operator, int x, int y)
    {
        @operator.Hand.MoveTo(x, y);
        return @operator;
    }

    public static IOperator Tap(this IOperator @operator)
    {
        @operator.Hand.Tap();
        return @operator;
    }

    public static IOperator LongTap(this IOperator @operator)
    {
        @operator.Hand.LongTap();
        return @operator;
    }

    public static IOperator Drag(this IOperator @operator)
    {
        @operator.Hand.Drag();
        return @operator;
    }

    public static IOperator Drop(this IOperator @operator)
    {
        @operator.Hand.Drop();
        return @operator;
    }

}