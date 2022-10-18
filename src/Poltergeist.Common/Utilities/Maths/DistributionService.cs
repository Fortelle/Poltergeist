using System;
using System.Drawing;
using System.Linq;
using Poltergeist.Common.Structures.Shapes;

namespace Poltergeist.Common.Utilities.Maths;

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
            (RectangleShape rect, ShapeDistributionType.Central) => RectangleToPointCentral(rect),
            (RectangleShape rect, ShapeDistributionType.Inclination) => RectangleToPointInclination(rect),

            (CircleShape circle, ShapeDistributionType.Uniform) => CircleToPointUniform(circle),
            (CircleShape circle, ShapeDistributionType.Central) => CircleToPointCentral(circle),

            (PolygonShape polygon, ShapeDistributionType.Uniform) => PolygonToPointUniform(polygon),
            (PolygonShape polygon, ShapeDistributionType.Central) => PolygonToPointCentral(polygon),

            _ => throw new NotSupportedException(),
        };
        return targetPoint;
    }

    public Point GetPointByOffset(Point clientPoint, int offset)
    {
        if (offset != 0)
        {
            var offsetX = Random.NextDouble(-1, 1) * offset;
            var offsetY = Random.NextDouble(-1, 1) * offset;
            clientPoint.X += (int)offsetX;
            clientPoint.Y += (int)offsetY;
        }

        return clientPoint;
    }

    public double NextDouble(RangeDistributionType type)
    {
        return type switch
        {
            RangeDistributionType.Uniform => Random.NextDouble(),
            RangeDistributionType.Gaussian => Random.NextDoubleBoxMuller(),
            RangeDistributionType.Triangular => Random.NextDoubleTriangular(),
            RangeDistributionType.Increasing => 1 - Random.NextDoubleExponential(),
            RangeDistributionType.Decreasing => Random.NextDoubleExponential(),
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

    private Point RectangleToPointCentral(RectangleShape rect)
    {
        double randX = Random.NextDoubleBoxMuller(),
               randY = Random.NextDoubleBoxMuller();
        int x = (int)(rect.X + rect.Width * randX),
            y = (int)(rect.Y + rect.Height * randY);

        return new Point(x, y);
    }

    private Point RectangleToPointInclination(RectangleShape rect)
    {
        double meanX = GetMeanRate(rect.X, rect.Width),
               meanY = GetMeanRate(rect.Y, rect.Height);
        meanX = meanX * 0.5 + 0.25;
        meanY = meanY * 0.5 + 0.25;
        double randX = Random.NextDoubleBoxMuller(meanX),
               randY = Random.NextDoubleBoxMuller(meanY);
        int x = rect.X + (int)(rect.Width * randX),
            y = rect.Y + (int)(rect.Height * randY);

        return new Point(x, y);
    }

    private Point CircleToPointUniform(CircleShape circle)
    {
        var randR = Random.NextDouble();
        var randT = Random.NextDouble();
        var radius = Math.Sqrt(randR) * circle.Radius;
        var theta = randT * Math.PI * 2;
        var x = circle.Origin.X + radius * Math.Cos(theta);
        var y = circle.Origin.Y + radius * Math.Sin(theta);

        return new Point((int)x, (int)y);
    }

    private Point CircleToPointCentral(CircleShape circle)
    {
        var randR = Random.NextDouble();
        var randT = Random.NextDouble();
        var radius = randR * circle.Radius;
        var theta = randT * Math.PI * 2;
        var x = circle.Origin.X + radius * Math.Cos(theta);
        var y = circle.Origin.Y + radius * Math.Sin(theta);

        return new Point((int)x, (int)y);
    }

    private Point PolygonToPointUniform(PolygonShape polygon)
    {
        var bounds = polygon.Bounds;
        while (true)
        {
            double randX = Random.NextDouble(),
                   randY = Random.NextDouble();
            int x = (int)(bounds.X + bounds.Width * randX),
                y = (int)(bounds.Y + bounds.Height * randY);

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
        double meanX = (double)(mean.X - bounds.X) / bounds.Width,
               meanY = (double)(mean.Y - bounds.Y) / bounds.Height;
        while (true)
        {
            double randX = Random.NextDoubleBoxMuller(meanX),
                   randY = Random.NextDoubleBoxMuller(meanY);
            int x = (int)(bounds.X + bounds.Width * randX),
                y = (int)(bounds.Y + bounds.Height * randY);
            var point = new Point(x, y);
            if (rect.Contains(point))
            {
                return point;
            }
        }
    }

    private int CombineHashCodes(int h1, int h2)
    {
        var x = (h1 << 5) + h1 ^ h2;
        return x;
    }

    private uint UIntToHash(uint x)
    {
        x = x + 0x7ed55d16 + (x << 12);
        x = x ^ 0xc761c23c ^ x >> 19;
        x = x + 0x165667b1 + (x << 5);
        x = x + 0xd3a2646c ^ x << 9;
        x = x + 0xfd7046c5 + (x << 3);
        x = x ^ 0xb55a4f09 ^ x >> 16;
        return x;
    }

    private double GetMeanRate(params int[] values)
    {
        var hash = (double)UIntToHash((uint)values.Sum()) / uint.MaxValue;
        return hash;
    }

    private double GetMeanRate(double range, params int[] values)
    {
        var hash = (double)values.Aggregate(0, (total, next) => CombineHashCodes(total, next)) / int.MaxValue;
        var mean = 0.5 - range + range * 2 * hash;
        return mean;
    }

}
