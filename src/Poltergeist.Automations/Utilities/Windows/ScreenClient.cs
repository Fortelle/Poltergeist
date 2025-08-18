using System.Drawing;

namespace Poltergeist.Automations.Utilities.Windows;

public class ScreenClient
{
    public int X = 0;
    public int Y = 0;
    public int Width = 0;
    public int Height = 0;
    public int Left => X;
    public int Top => Y;
    public int Right => X + Width;
    public int Bottom => Y + Height;

    public ScreenClient()
    {
    }

    public ScreenClient(Rectangle rect)
    {
        Bound = rect;
    }

    public Rectangle Bound
    {
        get
        {
            return new Rectangle(X, Y, Width, Height);
        }
        set
        {
            X = value.X;
            Y = value.Y;
            Width = value.Width;
            Height = value.Height;
        }
    }

    public Size Size
    {
        get
        {
            return new Size(Width, Height);
        }
        set
        {
            Width = value.Width;
            Height = value.Height;
        }
    }

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

    public bool Contains(Point pt)
    {
        return pt.X > 0 &&
            pt.X < Width &&
            pt.Y > 0 &&
            pt.Y < Height;
    }

    public bool IntersectsWith(Rectangle rect)
    {
        return rect.X < Width &&
            0 < rect.X + rect.Width &&
            rect.Y < Height &&
            0 < rect.Y + rect.Height;
    }

    public Rectangle Intersect(Rectangle rect)
    {
        var bound = Bound;
        bound.Intersect(rect);
        return bound;
    }

    public Point PointToScreen(Point p)
    {
        return new Point(X + p.X, Y + p.Y);
    }

    public Point PointToClient(Point p)
    {
        return new Point(p.X - X, p.Y - Y);
    }

}
