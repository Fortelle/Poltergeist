using System.Diagnostics;
using System.Drawing;
using Poltergeist.Automations.Components.Panels;
using Poltergeist.Automations.Macros;
using Poltergeist.Automations.Utilities.Maths;

namespace Poltergeist.Examples.Macros;

[ExampleMacro]
public class RandomExExample : BasicMacro
{
    public RandomExExample() : base()
    {
        Title = nameof(RandomEx);

        Category = "Features";

        Description = "This example generates random numbers using different distributions and visualizes their histograms.";

        Execute = (args) =>
        {
            var random = new RandomEx();
            var dataCount = 10000;
            var histogramWidth = 500;
            var histogramHeight = 100;
            var barWidth = 3;
            var binCount = 100;

            var stopwatch = new Stopwatch();

            var li = args.Processor.GetService<DashboardService>().Create<ImageInstrument>();
            li.MaximumColumns = 1;

            void Visualize(string text, Func<double> func)
            {
                var data = new double[dataCount];
                stopwatch.Restart();
                for (var i = 0; i < dataCount; i++)
                {
                    data[i] = func();
                }
                stopwatch.Stop();
                var bmp = DrawHistogram(data);
                li.Add(new(bmp, text + $" ({stopwatch.Elapsed}ms)"));
            }

            Bitmap DrawHistogram(double[] data)
            {
                var frequencies = new int[binCount];
                foreach (var value in data)
                {
                    frequencies[(int)(value * binCount)]++;
                }

                var max = frequencies.Max();
                var bmp = new Bitmap(histogramWidth, histogramHeight);
                using var gra = Graphics.FromImage(bmp);
                for (var i = 0; i < binCount; i++)
                {
                    var barHeight = 1f * frequencies[i] / max * histogramHeight;
                    var x = 1f * histogramWidth / binCount * i;
                    var y = histogramHeight - barHeight;
                    gra.FillRectangle(Brushes.Black, x, y, barWidth, barHeight);
                }

                return bmp;
            }

            Visualize("RandomEx.NextDouble()", () => random.NextDouble());
            Visualize("RandomEx.NextDouble(0, 1)", () => random.NextDouble(0, 1));
            Visualize("RandomEx.NextDoubleCentral(4)", () => random.NextDoubleCentral(4));
            Visualize("RandomEx.NextDoubleCentral(12)", () => random.NextDoubleCentral(12));
            Visualize("RandomEx.NextDoubleBoxMuller(0.5)", () => random.NextDoubleBoxMuller(0.5));
            Visualize("RandomEx.NextDoubleBoxMuller(0.8)", () => random.NextDoubleBoxMuller(0.8));
            Visualize("RandomEx.NextDoublePolar(0.5)", () => random.NextDoublePolar(0.5));
            Visualize("RandomEx.NextDoublePolar(0.8)", () => random.NextDoublePolar(0.8));
            Visualize("RandomEx.NextDoubleCauchy(0.5)", () => random.NextDoubleCauchy(0.5));
            Visualize("RandomEx.NextDoubleCauchy(0.8)", () => random.NextDoubleCauchy(0.8));
            Visualize("RandomEx.NextDoubleLaplace(0.5)", () => random.NextDoubleLaplace(0.5));
            Visualize("RandomEx.NextDoubleLaplace(0.8)", () => random.NextDoubleLaplace(0.8));
            Visualize("RandomEx.NextDoubleRayleigh(0.25)", () => random.NextDoubleRayleigh(0.25));
            Visualize("RandomEx.NextDoubleRayleigh(0.5)", () => random.NextDoubleRayleigh(0.5));
            Visualize("RandomEx.NextDoubleExponential(5)", () => random.NextDoubleExponential(5));
            Visualize("RandomEx.NextDoubleExponential(7)", () => random.NextDoubleExponential(7));
            Visualize("RandomEx.NextDoubleTriangular(0.5)", () => random.NextDoubleTriangular(0.5));
            Visualize("RandomEx.NextDoubleTriangular(0.5)", () => random.NextDoubleTriangular(0.8));
        };
    }
}
