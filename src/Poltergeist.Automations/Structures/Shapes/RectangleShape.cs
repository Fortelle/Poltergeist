using System.Drawing;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using Poltergeist.Automations.Utilities.Images;

namespace Poltergeist.Automations.Structures.Shapes;

[DataContract]
public class RectangleShape : IShape
{
    [DataMember]
    public string? Name { get; set; }

    [DataMember]
    public int X { get; set; }

    [DataMember]
    public int Y { get; set; }

    [DataMember]
    public int Width { get; set; }

    [DataMember]
    public int Height { get; set; }

    public RectangleShape()
    {
    }

    public RectangleShape(int x, int y, int width, int height)
    {
        X = x;
        Y = y;
        Width = width;
        Height = height;
    }

    public RectangleShape(Rectangle rect)
    {
        X = rect.X;
        Y = rect.Y;
        Width = rect.Width;
        Height = rect.Height;
    }

    public RectangleShape(Point pt, Size size)
    {
        X = pt.X;
        Y = pt.Y;
        Width = size.Width;
        Height = size.Height;
    }

    public int Left => X;
    public int Right => X + Width;
    public int Top => Y;
    public int Bottom => Y + Height;
    public Size Size => new(Width, Height);
    //public Point Location => new Point(X, Y);

    public bool IsValid => Width != 0 && Height != 0;
    public bool IsRegular => Width != 0 && Width == Height;

    public bool Contains(Point pt)
    {
        return pt.X >= X
            && pt.X <= X + Width
            && pt.Y >= Y
            && pt.Y <= Y + Width;
    }

    public void Pan(int x, int y)
    {
        X += x;
        Y += y;
    }

    public Rectangle Bounds => new(X, Y, Width, Height);

    [JsonIgnore]
    public Point Location
    {
        get
        {
            return new Point(X, Y);
        }
        set
        {
            X = value.X;
            Y = value.Y;
        }
    }

    public double Perimeter => Math.Abs(Width) * 2 + Math.Abs(Height) * 2;

    public float Area => Math.Abs(Width) * Math.Abs(Height);

    public PointF Centroid => new(X + Width / 2f, Y + Height / 2f);

    public Bitmap ToMask()
    {
        var bmp = new Bitmap(Width, Height);
        using var gra = Graphics.FromImage(bmp);
        gra.FillRectangle(Brushes.White, 0, 0, Width, Height);
        return bmp;
    }

    public Bitmap CropFrom(Bitmap src)
    {
        if (src.Size == Bounds.Size)
        {
            return new Bitmap(src);
        }
        else
        {
            return BitmapUtil.Crop(src, Bounds);
        }
    }

    public bool[] GetPointAvailabilities()
    {
        return Enumerable.Repeat(true, Width * Height).ToArray();
    }

    public object Clone()
    {
        return new RectangleShape(Bounds);
    }

    public string GetSignature()
    {
        return $"Rectangle-{X}-{Y}-{Width}-{Height}";
    }

}

