using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using Poltergeist.Common.Utilities.Images;
using Poltergeist.Common.Utilities.Maths;

namespace Poltergeist.Common.Structures.Shapes;

public class CircleShape : IShape
{
    public string Name { get; set; }

    public PointF Origin { get; set; }
    public double Radius { get; set; }
    public double Diameter => Radius * 2;

    public bool IsValid => Radius > 0;
    public bool IsRegular => Radius > 0;

    public CircleShape()
    {
    }

    public CircleShape(Point origin, int radius)
    {
        Origin = origin;
        Radius = radius;
    }

    public CircleShape(PointF origin, double radius)
    {
        Origin = origin;
        Radius = radius;
    }

    public CircleShape(Point origin, Point pt)
    {
        Origin = origin;
        Radius = CoordinationUtil.GetDistance(origin, pt);
    }

    public CircleShape(Point origin, double radius)
    {
        Origin = origin;
        Radius = radius;
    }

    public CircleShape(Point pt1, Point pt2, Point pt3)
    {
        float a = pt1.X - pt2.X;
        float b = pt1.Y - pt2.Y;
        float c = pt1.X - pt3.X;
        float d = pt1.Y - pt3.Y;
        var e = (pt1.X * pt1.X - pt2.X * pt2.X + (pt1.Y * pt1.Y - pt2.Y * pt2.Y)) / 2f;
        var f = (pt1.X * pt1.X - pt3.X * pt3.X + (pt1.Y * pt1.Y - pt3.Y * pt3.Y)) / 2f;
        var g = b * c - a * d;
        var x = -(d * e - b * f) / g;
        var y = -(a * f - c * e) / g;
        var r = (float)CoordinationUtil.GetDistance(x, y, pt1.X, pt1.Y);

        Origin = new PointF(x, y);
        Radius = r;
    }

    public CircleShape(int x, int y, int width, int height)
    {
        Origin = new PointF(x + (float)width / 2, y + (float)height / 2);
        Radius = Math.Min(width / 2, height / 2);
    }

    public CircleShape(Rectangle rect) : this(rect.X, rect.Y, rect.Width, rect.Height)
    {
    }

    public Rectangle Bounds
    {
        get
        {
            return Rectangle.FromLTRB(
                (int)Math.Floor(Origin.X - Radius),
                (int)Math.Floor(Origin.Y - Radius),
                (int)Math.Ceiling(Origin.X + Radius) + 1,
                (int)Math.Ceiling(Origin.Y + Radius) + 1
                );
        }
    }

    public Point Location
    {
        get
        {
            return new Point(
                (int)Math.Floor(Origin.X - Radius),
                (int)Math.Floor(Origin.Y - Radius)
                );
        }
        set
        {
            Origin = value;
        }
    }

    public PointF Centroid => Origin;

    public bool Contains(Point pt)
    {
        var distance = CoordinationUtil.GetDistance(Origin.X, Origin.Y, pt.X, pt.Y);
        return distance <= Radius;
    }

    public float Area => (float)(Math.PI * Radius * Radius);

    public double Perimeter => 2 * Math.PI * Radius;

    public Point GetPoint(CircleDistributeType type, Random random, int value = 0)
    {
        double x = 0, y = 0;

        if (type == CircleDistributeType.Random)
        {
            var radius = random.NextDouble() * Radius;
            var theta = random.NextDouble() * Math.PI * 2;
            x = Origin.X + radius * Math.Cos(theta);
            y = Origin.Y + radius * Math.Sin(theta);
        }
        else if (type == CircleDistributeType.Uniform)
        {
            var radius = Math.Sqrt(random.NextDouble()) * Radius;
            var theta = random.NextDouble() * Math.PI * 2;
            x = Origin.X + radius * Math.Cos(theta);
            y = Origin.Y + radius * Math.Sin(theta);
        }
        else if (type == CircleDistributeType.Average)
        {
            for (var i = 0; i < value; i++)
            {
                var radius = Math.Sqrt(random.NextDouble()) * Radius;
                var theta = random.NextDouble() * Math.PI * 2;
                x += Origin.X + radius * Math.Cos(theta);
                y += Origin.Y + radius * Math.Sin(theta);
            }
            x /= value;
            y /= value;
        }
        else if (type == CircleDistributeType.Measure)
        {
            var radius = Math.Sqrt(random.NextDouble()) * Radius;
            var theta = random.NextDouble() * Math.PI * 2;
            var rate = value / 16d;
            var mRadius = 0.618 * Radius;
            var mTheta = rate * Math.PI * 2;
            var x0 = Origin.X + mRadius * Math.Cos(mTheta);
            var y0 = Origin.Y + mRadius * Math.Sin(mTheta);
            var x1 = Origin.X + radius * Math.Cos(theta);
            var y1 = Origin.Y + radius * Math.Sin(theta);
            x = (x0 + x1) / 2;
            y = (y0 + y1) / 2;
        }
        else if (type == CircleDistributeType.Donuts) // value > 2
        {
            var theta = random.NextDouble() * Math.PI * 2;
            for (var i = 0; i < value; i++)
            {
                var radius = random.NextDouble() * Radius;
                x += Origin.X + radius * Math.Cos(theta);
                y += Origin.Y + radius * Math.Sin(theta);
            }
            x /= value;
            y /= value;
        }

        return new Point((int)x, (int)y);
    }

    //public void Offset(int x, int y)
    //{
    //    Origin = new PointF(Origin.X + x, Origin.Y + y);
    //}

    public Bitmap ToMask()
    {
        var bounds = Bounds;
        var bmp = new Bitmap(bounds.Width, bounds.Height);
        using var gra = Graphics.FromImage(bmp);
        var bruBack = Brushes.Black;
        var bruFore = Brushes.White;
        gra.FillRectangle(bruBack, 0, 0, bounds.Width, bounds.Height);
        gra.FillEllipse(bruFore, 0, 0, bounds.Width, bounds.Height);
        return bmp;
    }

    public Bitmap CropFrom(Bitmap src)
    {
        var bounds = Bounds;

        var destRect = new Rectangle(default, bounds.Size);
        var srcRect = src.Size == destRect.Size ? destRect : bounds;

        var dest = new Bitmap(destRect.Width, destRect.Height);
        using var gra = Graphics.FromImage(src);
        using var path = new GraphicsPath();
        path.AddEllipse(destRect);
        gra.SmoothingMode = SmoothingMode.Default;
        gra.Clip = new Region(path);
        gra.DrawImage(src, destRect, srcRect, GraphicsUnit.Pixel);
        return dest;
    }

    public bool[] GetPointAvailabilities()
    {
        using var bmp = ToMask();
        var pixels = PixelData.FromBitmap(bmp);
        var pa = pixels.ColorIterator((r, g, b, a) => a == 255).ToArray();
        return pa;
    }

    public object Clone()
    {
        return new CircleShape(Origin, Radius);
    }

    public string GetSignature()
    {
        return $"Circle-{Origin.X}-{Origin.Y}-{Radius}";
    }

    public enum CircleDistributeType
    {
        Uniform,
        Random,
        Average,
        Measure,
        Donuts,
    }

}

