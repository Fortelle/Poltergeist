using Microsoft.UI.Xaml.Media;
using Poltergeist.Automations.Components.Panels;
using Poltergeist.Helpers;

namespace Poltergeist.Pages.Macros.Instruments;

public class ImageInstrumentItemViewModel
{
    public ImageSource Image { get; set; }

    public string? Label { get; set; }

    public ImageInstrumentItemViewModel(ImageInstrumentItem item)
    {
        Image = BitmapHelper.ToImageSource(item.Image);
        Label = item.Label;
    }
}
