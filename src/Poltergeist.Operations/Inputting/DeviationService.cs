using System.Drawing;
using Poltergeist.Automations.Processors;
using Poltergeist.Automations.Services;
using Poltergeist.Automations.Structures.Shapes;
using Poltergeist.Automations.Utilities.Maths;

namespace Poltergeist.Operations.Inputting;

public class DeviationService : MacroService
{
    private readonly DistributionService DistributionService;

    public DeviationService(
        MacroProcessor processor,
        DistributionService distributionService
        ) : base(processor)
    {
        DistributionService = distributionService;
    }

    public Point GetRandomPoint(IShape targetShape, Rectangle bounds, ShapeDistributionType distribution, Point? lastPosition = null)
    {
        if (lastPosition is not null && targetShape.Contains(lastPosition.Value))
        {
            Logger.Trace($"Used the last position because it is still inside the target shape.", new { lastPosition, targetShape });
            return lastPosition.Value;
        }

        var randomPoint = GetPointByShape(targetShape, distribution, bounds);

        Logger.Trace($"Calculated random point on workspace: {{{randomPoint.X},{randomPoint.Y}}}.", new { targetShape, distribution, bounds });

        return randomPoint;
    }

    public Point GetRandomPoint(Rectangle targetRectangle, Rectangle bounds, ShapeDistributionType distribution, Point? lastPosition = null)
    {
        return GetRandomPoint(new RectangleShape(targetRectangle), bounds, distribution, lastPosition);
    }

    private Point GetPointByShape(IShape shape, ShapeDistributionType type, Rectangle bounds)
    {
        if (bounds == default)
        {
            return DistributionService.GetPointByShape(shape, type);
        }

        // todo: check intersection first

        for (var i = 0; i < MouseInputOptions.PointInShapeMaxRetry; i++)
        {
            var point = DistributionService.GetPointByShape(shape, type);

            if (bounds.Contains(point))
            {
                return point;
            }
        }

        throw new Exception("Failed to get a point inside the bounds after maximum retries.");
    }
}
