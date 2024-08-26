using System.Drawing;

namespace Poltergeist.Automations.Components.Panels;

public class ImageInstrumentItem : IDisposable
{
    public Bitmap Image { get; set; }

    public string? Label { get; set; }

    public ImageInstrumentItem(Bitmap image)
    {
        Image = image;
    }

    public ImageInstrumentItem(Bitmap image, string label)
    {
        Image = image;
        Label = label;
    }

    public void Dispose()
    {
        Image?.Dispose();
    }
}
