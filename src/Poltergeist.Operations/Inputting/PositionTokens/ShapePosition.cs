using System.Drawing;
using Poltergeist.Automations.Structures.Shapes;
using Poltergeist.Automations.Utilities.Maths;

namespace Poltergeist.Operations.Inputting;

public class ShapePosition : PositionToken
{
    public IShape Shape { get; }

    public ShapeDistributionType? ShapeDistribution { get; init; }

    public bool? KeepUnmovedInShape { get; init; }

    public ShapePosition(IShape shape)
    {
        Shape = shape;
    }

    public ShapePosition(Rectangle rect)
    {
        Shape = new RectangleShape(rect);
    }
}
