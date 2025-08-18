using System.Drawing;
using Poltergeist.Automations.Components.Panels;
using Poltergeist.Automations.Macros;

namespace Poltergeist.Examples.Macros;

[ExampleMacro]
public class ImageInstrumentExample : BasicMacro
{
    public ImageInstrumentExample() : base()
    {
        Title = "ImageInstrument";

        Category = "Instruments";

        Description = "This example uses the ImageInstrument to display images.";

        Execute = (args) =>
        {
            var li = args.Processor.GetService<DashboardService>().Create<ImageInstrument>();
            li.Title = "Images:";

            for (var i = 0; i < 30; i++)
            {
                var bmp = new Bitmap(128, 128);
                using var gra = Graphics.FromImage(bmp);
                gra.DrawRectangle(Pens.Black, 0, 0, bmp.Width - 1, bmp.Height - 1);
                gra.DrawLine(Pens.Black, 0, 0, bmp.Width - 1, bmp.Height - 1);
                gra.DrawLine(Pens.Black, bmp.Width - 1, 0, 0,  bmp.Height - 1);

                li.Add(new(bmp, $"Image {i + 1}"));
            }
        };
    }
}
