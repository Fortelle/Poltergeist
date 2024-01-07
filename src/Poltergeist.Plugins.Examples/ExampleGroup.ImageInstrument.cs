using System.Drawing;
using Poltergeist.Automations.Attributes;
using Poltergeist.Automations.Components.Panels;
using Poltergeist.Automations.Macros;

namespace Poltergeist.Test;

public partial class ExampleGroup : MacroGroup
{
    [AutoLoad]
    public BasicMacro ImageInstrumentExample = new("example_imageinstrument")
    {
        Title = "ImageInstrument Example",

        Description = "This example shows how to display images by using the ImageInstrument.",

        ExecuteAsync = async (args) =>
        {
            var li = args.Processor.GetService<DashboardService>().Create<ImageInstrument>();
            li.Title = "Images:";

            for (var i = 0; i < 5; i++)
            {
                using var wc = new HttpClient();
                using var stream = await wc.GetStreamAsync("https://picsum.photos/200");
                var bmp = new Bitmap(stream);

                li.Add(new(bmp, $"Image {i + 1}"));
            }
        }
    };

}
