using System.Drawing;
using Poltergeist.Automations.Structures.Shapes;

namespace Poltergeist.Automations.Utilities.Maths;

public class DistributionService
{
    public RandomEx Random { get; }

    public DistributionService(RandomEx random)
    {
        Random = random;
    }

    public Point GetPointByShape(IShape shape, ShapeDistributionType type)
    {
        var targetPoint = (shape, type) switch
        {
            (RectangleShape rect, ShapeDistributionType.Uniform) => RectangleToPointUniform(rect),
            (RectangleShape rect, ShapeDistributionType.Gaussian) => RectangleToPointGaussian(rect),
            (RectangleShape rect, ShapeDistributionType.Eccentric) => RectangleToPointEccentric(rect),
            (RectangleShape rect, ShapeDistributionType.Centroid) => Point.Round(rect.Centroid),

            (CircleShape circle, ShapeDistributionType.Uniform) => CircleToPointUniform(circle),
            (CircleShape circle, ShapeDistributionType.Gaussian) => CircleToPointGaussian(circle),
            (CircleShape circle, ShapeDistributionType.Eccentric) => CircleToPointEccentric(circle),
            (CircleShape circle, ShapeDistributionType.Centroid) => Point.Round(circle.Centroid),

            (PolygonShape polygon, ShapeDistributionType.Uniform) => PolygonToPointUniform(polygon),
            (PolygonShape polygon, ShapeDistributionType.Gaussian) => PolygonToPointCentral(polygon),
            (PolygonShape polygon, ShapeDistributionType.Centroid) => Point.Round(polygon.Centroid), // warn: may be outside

            _ => throw new NotSupportedException(),
        };
        return targetPoint;
    }

    public double NextDouble(RangeDistributionType type)
    {
        return type switch
        {
            RangeDistributionType.Uniform => Random.NextDouble(),
            RangeDistributionType.Gaussian => Random.NextDoubleBoxMuller(),
            RangeDistributionType.Triangular => Random.NextDoubleTriangular(.5),
            RangeDistributionType.Increasing => 1 - Random.NextDoubleExponential(),
            RangeDistributionType.Decreasing => Random.NextDoubleExponential(),
            RangeDistributionType.Maximum => (int.MaxValue - 1.0) / int.MaxValue, // https://github.com/dotnet/runtime/blob/12f87d76318ae5021af3d8aa2a74a32f7bba2ebd/src/libraries/System.Private.CoreLib/src/System/Random.CompatImpl.cs#L357
            RangeDistributionType.Minimum => 0,
            RangeDistributionType.Average => .5,
            _ => throw new NotImplementedException(),
        };
    }

    private Point RectangleToPointUniform(RectangleShape rect)
    {
        double randX = Random.NextDouble(),
               randY = Random.NextDouble();
        int x = (int)(rect.X + rect.Width * randX),
            y = (int)(rect.Y + rect.Height * randY);

        return new Point(x, y);
    }

    private Point RectangleToPointGaussian(RectangleShape rect)
    {
        var randX = Random.NextDoubleBoxMuller();
        var randY = Random.NextDoubleBoxMuller();
        var x = (int)(rect.X + rect.Width * randX);
        var y = (int)(rect.Y + rect.Height * randY);

        return new Point(x, y);
    }

    private Point RectangleToPointEccentric(RectangleShape rect)
    {
        var meanT = GetMeanRate(rect.X, rect.Y, rect.Width, rect.Height);
        var theta = meanT * Math.PI * 2;
        var c = Math.Cos(theta);
        var s = Math.Sin(theta);
        var tx = rect.Width / 2.0 / Math.Abs(c);
        var ty = rect.Height / 2.0 / Math.Abs(s);
        var tmin = Math.Min(tx, ty);
        var meanX = ((0.618 - 0.5) * tmin * c) / rect.Width / 2 / 2 + 0.5;
        var meanY = ((0.618 - 0.5) * tmin * s) / rect.Height / 2 / 2 + 0.5;
        var randX = Random.NextDoubleBoxMuller(meanX);
        var randY = Random.NextDoubleBoxMuller(meanY);
        var x = rect.X + (int)(rect.Width * randX);
        var y = rect.Y + (int)(rect.Height * randY);

        return new Point(x, y);
    }

    private Point CircleToPointUniform(CircleShape circle)
    {
        var randD = Random.NextDouble();
        var randT = Random.NextDouble();
        var distance = Math.Sqrt(randD) * circle.Radius;
        var theta = randT * Math.PI * 2;
        var x = circle.Origin.X + distance * Math.Cos(theta);
        var y = circle.Origin.Y + distance * Math.Sin(theta);

        return new Point((int)x, (int)y);
    }

    private Point CircleToPointGaussian(CircleShape circle)
    {
        while (true)
        {
            var randX = (Random.NextDoubleBoxMuller() - 0.5) * 2;
            var randY = (Random.NextDoubleBoxMuller() - 0.5) * 2;
            var offsetX = circle.Radius * randX;
            var offsetY = circle.Radius * randY;
            var distanceToCentroid = Math.Sqrt(offsetX * offsetX + offsetY * offsetY);
            if (distanceToCentroid < circle.Radius)
            {
                var x = (int)(circle.Centroid.X + offsetX);
                var y = (int)(circle.Centroid.Y + offsetY);
                return new Point(x, y);
            }
        }
    }

    private Point CircleToPointEccentric(CircleShape circle)
    {
        var length = circle.Radius * (0.618 - 0.5);
        var meanT = GetMeanRate((int)circle.Centroid.X, (int)circle.Centroid.Y, (int)circle.Radius);
        var theta = meanT * Math.PI * 2;
        var meanX = length * Math.Cos(theta) / circle.Radius / 2 + 0.5;
        var meanY = length * Math.Sin(theta) / circle.Radius / 2 + 0.5;
        while (true)
        {
            var randX = (Random.NextDoubleBoxMuller(meanX) - 0.5) * 2;
            var randY = (Random.NextDoubleBoxMuller(meanY) - 0.5) * 2;
            var offsetX = circle.Radius * randX;
            var offsetY = circle.Radius * randY;
            var distanceToCentroid = Math.Sqrt(offsetX * offsetX + offsetY * offsetY);
            if (distanceToCentroid < circle.Radius)
            {
                var x = (int)(circle.Centroid.X + offsetX);
                var y = (int)(circle.Centroid.Y + offsetY);
                return new Point(x, y);
            }
        }
    }

    private Point PolygonToPointUniform(PolygonShape polygon)
    {
        var bounds = polygon.Bounds;
        while (true)
        {
            var randX = Random.NextDouble();
            var randY = Random.NextDouble();
            var x = (int)(bounds.X + bounds.Width * randX);
            var y = (int)(bounds.Y + bounds.Height * randY);

            var point = new Point(x, y);
            if (polygon.Contains(point))
            {
                return point;
            }
        }
    }

    private Point PolygonToPointCentral(PolygonShape rect)
    {
        var mean = rect.Centroid;
        var bounds = rect.Bounds;
        var meanX = (double)(mean.X - bounds.X) / bounds.Width;
        var meanY = (double)(mean.Y - bounds.Y) / bounds.Height;
        while (true)
        {
            var randX = Random.NextDoubleBoxMuller(meanX);
            var randY = Random.NextDoubleBoxMuller(meanY);
            var x = (int)(bounds.X + bounds.Width * randX);
            var y = (int)(bounds.Y + bounds.Height * randY);
            var point = new Point(x, y);
            if (rect.Contains(point))
            {
                return point;
            }
        }
    }

    private static int CombineHashCodes(int h1, int h2)
    {
        var x = (h1 << 5) + h1 ^ h2;
        return x;
    }

    private static uint UIntToHash(uint x)
    {
        x = x + 0x7ed55d16 + (x << 12);
        x = x ^ 0xc761c23c ^ x >> 19;
        x = x + 0x165667b1 + (x << 5);
        x = x + 0xd3a2646c ^ x << 9;
        x = x + 0xfd7046c5 + (x << 3);
        x = x ^ 0xb55a4f09 ^ x >> 16;
        return x;
    }

    private static double GetMeanRate(params int[] values)
    {
        var hash = 0u;
        foreach (var value in values)
        {
            hash |= UIntToHash((uint)value);
        }
        return (double)hash / uint.MaxValue;
    }

    private static double GetMeanRate(double range, params int[] values)
    {
        var hash = (double)values.Aggregate(0, CombineHashCodes) / int.MaxValue;
        var mean = 0.5 - range + range * 2 * hash;
        return mean;
    }

}
