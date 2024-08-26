using System.Drawing;
using System.Runtime.Serialization;

namespace Poltergeist.Automations.Structures.Shapes;

[DataContract]
public class RotatedRectangleShape : PolygonShape
{
    [DataMember]
    public int X { get; set; }

    [DataMember]
    public int Y { get; set; }

    [DataMember]
    public int Width { get; set; }

    [DataMember]
    public int Height { get; set; }

    [DataMember]
    public float Rotation { get; set; }

    public RotatedRectangleShape()
    {
    }

    public RotatedRectangleShape(int x, int y, int width, int height, float rotation)
    {
        var ox = x + width / 2d;
        var oy = y + height / 2d;
        var half_diag = 0.5d * Math.Sqrt(width * width + height * height);
        var angle = Math.PI / 180d * rotation;

        var point1 = new PointF(x, y);
        var point2 = new PointF((float)(ox + half_diag * Math.Cos(angle)), (float)(oy - half_diag * Math.Sin(angle)));
        var point3 = new PointF((float)(ox - half_diag * Math.Sin(angle)), (float)(oy - half_diag * Math.Cos(angle)));
        var point4 = new PointF((float)(ox - half_diag * Math.Cos(angle)), (float)(oy + half_diag * Math.Sin(angle)));

        Points = [
            Point.Round(point1),
            Point.Round(point2),
            Point.Round(point3),
            Point.Round(point4),
        ];
    }

    public RotatedRectangleShape(Rectangle rect, float rotation)
        : this(rect.X, rect.Y, rect.Width, rect.Height, rotation)
    {
    }

    public RotatedRectangleShape(Point pt, Size size, float rotation)
        : this(pt.X, pt.Y, size.Width, size.Height, rotation)
    {
    }

}

