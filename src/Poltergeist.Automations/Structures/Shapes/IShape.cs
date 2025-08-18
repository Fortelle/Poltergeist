using System.Drawing;

namespace Poltergeist.Automations.Structures.Shapes;

public interface IShape : ICloneable
{
    string? Name { get; set; }

    Point Location { get; set; }
    Rectangle Bounds { get; }

    PointF Centroid { get; }
    float Area { get; }
    double Perimeter { get; }

    bool IsValid { get; }
    bool IsRegular { get; }

    bool Contains(Point pt);
    void Pan(int x, int y);

    Bitmap ToMask();
    Bitmap CropFrom(Bitmap src);
    bool[] GetPointAvailabilities();
    string GetSignature();

    static IShape? FromSignature(string sign)
    {
        var values = sign.Split('-');
        if (values[0] == "Rectangle")
        {
            var x = int.Parse(values[1]);
            var y = int.Parse(values[2]);
            var w = int.Parse(values[3]);
            var h = int.Parse(values[4]);
            var rect = new RectangleShape(x, y, w, h);
            return rect;
        }
        else if (values[0] == "Circle")
        {
            var x = float.Parse(values[1]);
            var y = float.Parse(values[2]);
            var r = double.Parse(values[4]);
            var circle = new CircleShape(new PointF(x, y), r);
            return circle;
        }
        else if (values[0] == "Polygon")
        {
            var count = (values.Length - 1) / 2;
            var points = new Point[count];
            for (var i = 0; i < count; i++)
            {
                var x = int.Parse(values[1 + i * 2]);
                var y = int.Parse(values[1 + i * 2 + 1]);
                points[i] = new Point(x, y);
            }
            var polygon = new PolygonShape(points);
            return polygon;
        }
        else
        {
            return null;
        }
    }

}
