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
            (RectangleShape rect, ShapeDistributionType.Preference) => RectangleToPointMean(rect),
            (RectangleShape rect, ShapeDistributionType.Uniform) => RectangleToPointUniform(rect),
            (CircleShape circle, ShapeDistributionType.Central) => CircleToPointCentral(circle),
            (CircleShape circle, ShapeDistributionType.Uniform) => CircleToPointUniform(circle),
            _ => throw new NotImplementedException(),
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
















































    private Point RectangleToPointUniform(RectangleShape rect)
    {
        double randX = Random.NextDouble(),
               randY = Random.NextDouble();
        int x = (int)(rect.X + rect.Width * randX),
            y = (int)(rect.Y + rect.Height * randY);

        return new Point(x, y);
    }

    private Point RectangleToPointMean(RectangleShape rect)
    {
        double meanX = GetMeanRate(rect.X, rect.Width),
               meanY = GetMeanRate(rect.Y, rect.Height);
        meanX = meanX * 0.5 + 0.25;
        meanY = meanY * 0.5 + 0.25;
        double randX = Random.NextTriangular(meanX),
               randY = Random.NextTriangular(meanY);
        int x = rect.X + (int)(rect.Width * randX),
            y = rect.Y + (int)(rect.Height * randY);

        return new Point(x, y);
    }

    private Point RectangleToPointCLT(RectangleShape rect, int sigma)
    {
        double randX = Random.NextCentral(sigma),
               randY = Random.NextCentral(sigma);
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

    private Point FromCircleTest(CircleShape circle, Random random)
    {
        double x = 0, y = 0;
        for (var i = 0; i < 4; i++)
        {
            var randR = random.NextDouble();// random.NextDouble();
            var randT = random.NextDouble();
            var radius = Math.Sqrt(randR) * circle.Radius;
            var theta = randT * Math.PI * 2;
            x += circle.Origin.X + radius * Math.Cos(theta);
            y += circle.Origin.Y + radius * Math.Sin(theta);
        }
        x /= 4;
        y /= 4;

        return new Point((int)x, (int)y);
    }

    private Point FromCircleG(CircleShape circle, Random random)
    {
        double x2 = 0, y2 = 0;

        if (true)
        {
            var randR = random.NextDouble();
            var randT = random.NextDouble();
            var radius = Math.Sqrt(randR) * circle.Radius;
            var theta = randT * Math.PI * 2;
            x2 += circle.Origin.X + radius * Math.Cos(theta);
            y2 += circle.Origin.Y + radius * Math.Sin(theta);
        }

        var c = 1;
        var t = 2;
        for (var i = 0; i < t - 1; i++)
        {
            if (i > 0 && random.NextDouble() > 0.618)
            {
                continue;
            }
            var randR = random.NextDouble();
            var randT = random.NextDouble();
            var radius = Math.Sqrt(randR) * circle.Radius;
            var theta = randT * Math.PI * 2;
            x2 += circle.Origin.X + radius * Math.Cos(theta);
            y2 += circle.Origin.Y + radius * Math.Sin(theta);
            c++;
        }

        if (random.NextDouble() < 0.618)
        {
            var t2 = GetMeanRate((int)circle.Origin.X, (int)circle.Origin.Y) * Math.PI * 2;
            var r2 = 0.5 / (0.5 + 0.618) * circle.Radius;
            x2 += circle.Origin.X + r2 * Math.Cos(t2);
            y2 += circle.Origin.Y + r2 * Math.Sin(t2);
            c++;
        }

        return new Point((int)(x2 / c), (int)(y2 / c));
    }


    private Point FromPolygonMean(PolygonShape rect)
    {
        var mean = rect.Centroid;
        var bounds = rect.Bounds;
        double meanX = (double)(mean.X - bounds.X) / bounds.Width,
               meanY = (double)(mean.Y - bounds.Y) / bounds.Height;
        while (true)
        {
            double randX = Random.NextTriangular(meanX),
                   randY = Random.NextTriangular(meanY);
            int x = bounds.X + (int)(bounds.Width * randX),
                y = bounds.Y + (int)(bounds.Height * randY);
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

public enum ShapeDistributionType
{
    Uniform,
    Central,
    Preference,
}

