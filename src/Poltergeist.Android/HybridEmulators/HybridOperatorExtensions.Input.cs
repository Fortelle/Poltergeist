using System.Drawing;
using Poltergeist.Automations.Structures.Shapes;
using Poltergeist.Operations.Inputting;

namespace Poltergeist.Android.HybridEmulators;

// todo: https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/proposals/csharp-15.0/unions
public static partial class HybridOperatorExtensions
{
    extension(IHybridOperator @operator)
    {
        public IHybridOperator Tap(Rectangle targetRectangle)
        {
            @operator.Hand.Tap(new ShapePosition(targetRectangle));
            return @operator;
        }

        public IHybridOperator Tap(IShape targetShape)
        {
            @operator.Hand.Tap(new ShapePosition(targetShape));
            return @operator;
        }

        public IHybridOperator Tap(Point targetPoint)
        {
            @operator.Hand.Tap(new CoarsePoint(targetPoint));
            return @operator;
        }

        public IHybridOperator Tap(string key)
        {
            @operator.Hand.Tap(new NamedPosition(key));
            return @operator;
        }

        public IHybridOperator Tap()
        {
            @operator.Hand.Tap(new LastPoint());
            return @operator;
        }


        public IHybridOperator LongTap(Rectangle targetRectangle)
        {
            @operator.Hand.LongTap(new ShapePosition(targetRectangle));
            return @operator;
        }

        public IHybridOperator LongTap(IShape targetShape)
        {
            @operator.Hand.LongTap(new ShapePosition(targetShape));
            return @operator;
        }

        public IHybridOperator LongTap(Point targetPoint)
        {
            @operator.Hand.LongTap(new CoarsePoint(targetPoint));
            return @operator;
        }

        public IHybridOperator LongTap(string key)
        {
            @operator.Hand.LongTap(new NamedPosition(key));
            return @operator;
        }

        public IHybridOperator LongTap()
        {
            @operator.Hand.LongTap(new LastPoint());
            return @operator;
        }


        public IHybridOperator Swipe(Rectangle beginRectangle, Rectangle endRectangle)
        {
            @operator.Hand.Swipe(new ShapePosition(beginRectangle), new ShapePosition(endRectangle));
            return @operator;
        }

        public IHybridOperator Swipe(IShape beginShape, IShape endShape)
        {
            @operator.Hand.Swipe(new ShapePosition(beginShape), new ShapePosition(endShape));
            return @operator;
        }

        public IHybridOperator Swipe(Point beginPoint, Point endPoint)
        {
            @operator.Hand.Swipe(new CoarsePoint(beginPoint), new CoarsePoint(endPoint));
            return @operator;
        }

        public IHybridOperator Swipe(string beginKey, string endKey)
        {
            @operator.Hand.Swipe(new NamedPosition(beginKey), new NamedPosition(endKey));
            return @operator;
        }
    }
}