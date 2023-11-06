using System.Drawing;
using System.Drawing.Drawing2D;
using Poltergeist.Common.Utilities.Maths;
using Poltergeist.Common.Utilities.Images;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Poltergeist.Common.Structures.Shapes;

[DataContract]
public class PolygonShape : IShape
{
    [DataMember]
    public string? Name { get; set; }

    [DataMember]
    public Point[] Points { get; set; }

    public PolygonShape()
    {
        Points = Array.Empty<Point>();
    }

    public PolygonShape(params Point[] points)
    {
        Points = points;
    }

    public Rectangle Bounds => CoordinationUtil.GetWholeRect(Points);

    [JsonIgnore]
    public Point Location
    {
        get
        {
            return new Point(
                Points.Min(x => x.X),
                Points.Min(x => x.Y)
                );
        }
        set
        {
            Points = Points.Select(pt => new Point(pt.X + value.X, pt.Y + value.Y)).ToArray();
        }
    }

    public double Perimeter
    {
        get
        {
            var d = CoordinationUtil.GetDistance(Points[0], Points[Points.Length - 1]);
            for (var i = 1; i < Points.Length; i++)
            {
                d += CoordinationUtil.GetDistance(Points[i], Points[i - 1]);
            }
            return d;
        }
    }

    public float Area
    {
        get
        {
            var newPoints = Points.Concat(new Point[] { Points[0] }).ToArray();
            float area = 0;
            for (var i = 0; i < newPoints.Length - 1; i++)
            {
                area += (newPoints[i + 1].X - newPoints[i].X) * (newPoints[i + 1].Y + newPoints[i].Y) / 2;
            }
            return Math.Abs(area);
        }
    }

    public PointF Centroid
    {
        get
        {
            var newPoints = Points.Concat(new Point[] { Points[0] }).ToArray();

            float X = 0;
            float Y = 0;
            float second_factor;
            for (var i = 0; i < newPoints.Length - 1; i++)
            {
                second_factor = newPoints[i].X * newPoints[i + 1].Y - newPoints[i + 1].X * newPoints[i].Y;
                X += (newPoints[i].X + newPoints[i + 1].X) * second_factor;
                Y += (newPoints[i].Y + newPoints[i + 1].Y) * second_factor;
            }

            var area = Area;
            X /= 6 * area;
            Y /= 6 * area;

            if (X < 0)
            {
                X = -X;
                Y = -Y;
            }

            return new PointF(X, Y);
        }
    }

    public bool IsValid
    {
        get
        {
            if (Points.Length < 2)
            {
                return false;
            }

            var bounds = Bounds;
            return bounds.Height != 0 && bounds.Width != 0;
        }
    }

    public bool IsRegular
    {
        get
        {
            var angles = new double[Points.Length];
            for (var i = 0; i < Points.Length; i++)
            {
                var p0 = Points[i];
                var p1 = Points[i > 0 ? i - 1 : Points.Length - 1];
                var p2 = Points[i < Points.Length - 1 ? i + 1 : 0];
                var x1 = p0.X - p1.X;
                var y1 = p0.Y - p1.Y;
                var x2 = p0.X - p2.X;
                var y2 = p0.Y - p2.Y;
                var v0 = x1 * x2 + y1 * y2;
                var l1 = Math.Sqrt(x1 * x1 + y1 * y1);
                var l2 = Math.Sqrt(x2 * x2 + y2 * y2);
                var cos = v0 / (l1 * l2);
                angles[i] = Math.Acos(cos) * 180 / Math.PI;
            }
            return angles.All(x => x == angles.First());
        }
    }

    public bool Contains(Point pt)
    {
        var result = false;
        for (int i = 0, j = Points.Length - 1; i < Points.Length; j = i++)
        {
            if (Points[i].Y > pt.Y != Points[j].Y > pt.Y && pt.X < (Points[j].X - Points[i].X) * (pt.Y - Points[i].Y) / (Points[j].Y - Points[i].Y) + Points[i].X)
            {
                result = !result;
            }
        }
        return result;
    }



    public Point GetPoint(PolygonDistributeType type, Random random, int value = 0)
    {
        int x = 0, y = 0;

        if (type == PolygonDistributeType.Random)
        {
            var bounds = Bounds;
            while (true)
            {
                x = (int)(bounds.X + bounds.Width * random.NextDouble());
                y = (int)(bounds.Y + bounds.Height * random.NextDouble());
                if (Contains(new Point(x, y)))
                {
                    break;
                }
            }
        }
        else if (type == PolygonDistributeType.Meansure)
        {
            var meansure = Centroid;
            var bounds = Bounds;
            var vertexes = new Point[]
            {
                new Point(bounds.X, bounds.Y),
                new Point(bounds.X + bounds.Width, bounds.Y),
                new Point(bounds.X + bounds.Width, bounds.Y + bounds.Height),
                new Point(bounds.X, bounds.Y + bounds.Height),
            };
            var vertexRadians = vertexes.Select(pt => Math.Atan((pt.Y - meansure.Y) / (pt.X - meansure.X))).ToArray();
            while (true)
            {
                var theta = random.NextDouble() * Math.PI * 2;
                throw new NotImplementedException();
                //double radius = random.NextDouble() * Radius;
                //x = Origin.X + radius * Math.Cos(theta);
                //y = Origin.Y + radius * Math.Sin(theta);

                //x = (int)(bounds.X + bounds.Width * random.NextDouble());
                //y = (int)(bounds.Y + bounds.Height * random.NextDouble());
                //if (Contains(new Point(x, y))) break;
            }
        }

        return new Point(x, y);
    }

    public void Pan(int x, int y)
    {
        Points = Points.Select(pt => new Point(pt.X + x, pt.Y + y)).ToArray();
    }

    public Bitmap ToMask()
    {
        var bounds = Bounds;
        var points = Points.Select(pt => new Point(pt.X - bounds.X, pt.Y - bounds.Y)).ToArray();
        var bmp = new Bitmap(bounds.Width, bounds.Height);
        using var gra = Graphics.FromImage(bmp);
        var bruBack = Brushes.Black;
        var bruFore = Brushes.White;
        gra.FillRectangle(bruBack, 0, 0, bounds.Width, bounds.Height);
        gra.FillPolygon(bruFore, points);
        return bmp;
    }

    public Bitmap CropFrom(Bitmap src)
    {
        var bounds = Bounds;

        var destRect = new Rectangle(default, bounds.Size);
        var destPoints = Points.Select(pt => new Point(pt.X - bounds.X, pt.Y - bounds.Y)).ToArray();

        var srcRect = src.Size == destRect.Size ? destRect : bounds;
        var srcPoints = src.Size == destRect.Size ? destPoints : Points;

        var dest = new Bitmap(bounds.Width, bounds.Height);
        using var gra = Graphics.FromImage(dest);
        using var path = new GraphicsPath();
        path.AddPolygon(srcPoints);
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

    public string GetSignature()
    {
        var s = "Polygon";
        foreach (var p in Points)
        {
            s += $"-{p.X}-{p.Y}";
        }
        return s;
    }



    public object Clone()
    {
        return new PolygonShape(Points.ToArray());
    }

}
