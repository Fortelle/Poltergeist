using System.Diagnostics;
using System.Drawing;
using Poltergeist.Automations.Components.Panels;
using Poltergeist.Automations.Macros;
using Poltergeist.Automations.Structures.Shapes;
using Poltergeist.Automations.Utilities.Images;
using Poltergeist.Automations.Utilities.Maths;

namespace Poltergeist.Examples.Macros;

[ExampleMacro]
public class DistributionServiceExample : BasicMacro
{
    public DistributionServiceExample() : base()
    {
        Title = nameof(DistributionService);

        Category = "Features";

        Description = "This example visualizes point distributions within shapes using different distribution types.";

        Execute = (args) =>
        {
            var random = new RandomEx();
            var distributionService = new DistributionService(random);

            var canvasWidth = 400;
            var canvasHeight = 300;

            var stopwatch = new Stopwatch();

            var li = args.Processor.GetService<DashboardService>().Create<ImageInstrument>();
            li.MaximumColumns = 1;

            void Visualize(IShape shape, ShapeDistributionType distributionType)
            {
                var dataCount = (int)shape.Area;
                var buffer = new byte[canvasWidth * canvasHeight * 4];
                stopwatch.Restart();
                for (var i = 0; i < dataCount; i++)
                {
                    var point = distributionService.GetPointByShape(shape, distributionType);
                    var index = (point.Y * canvasWidth + point.X) * 4;
                    buffer[index] = 0;
                    buffer[index + 1] = 0;
                    buffer[index + 2] = 0;
                    buffer[index + 3] += 32;
                }
                stopwatch.Stop();

                var pixelData = new PixelData(buffer, canvasWidth, canvasHeight);
                var bmp = pixelData.ToBitmap();
                var centroid = shape.Centroid;
                using (var gra = Graphics.FromImage(bmp))
                {
                    gra.DrawLine(Pens.Red, centroid.X - 5, centroid.Y, centroid.X + 5, centroid.Y);
                    gra.DrawLine(Pens.Red, centroid.X, centroid.Y - 5, centroid.X, centroid.Y + 5);
                }
                li.Add(new(bmp, $"{shape.GetSignature()}, {distributionType} ({stopwatch.Elapsed}ms)"));
            }

            var rect = new RectangleShape(0, 0, canvasWidth, canvasHeight);
            Visualize(rect, ShapeDistributionType.Uniform);
            Visualize(rect, ShapeDistributionType.Gaussian);
            Visualize(rect, ShapeDistributionType.Eccentric);

            var circle = new CircleShape(0, 0, canvasWidth, canvasHeight);
            Visualize(circle, ShapeDistributionType.Uniform);
            Visualize(circle, ShapeDistributionType.Gaussian);
            Visualize(circle, ShapeDistributionType.Eccentric);

            var polygon = new PolygonShape(new Point(canvasWidth / 2, (int)(canvasHeight * .2)), new Point((int)(canvasWidth * .2), (int)(canvasHeight * .8)), new Point((int)(canvasWidth * .8), (int)(canvasHeight * .8)));
            Visualize(polygon, ShapeDistributionType.Uniform);
            Visualize(polygon, ShapeDistributionType.Gaussian);
        };
    }
}
