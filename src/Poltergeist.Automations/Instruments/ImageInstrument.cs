using System;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Windows.Media;
using Poltergeist.Common.Converters;

namespace Poltergeist.Automations.Instruments;

public class ImageInstrument// : InstrumentBase
{
    public ObservableCollection<ImageSource> Images { get; set; } = new();

    public void Clear()
    {
        Images.Clear();
    }

    public void Add(Bitmap bitmap)
    {
        var image = BitmapToImageSourceConverter.ToImageSource(bitmap);
        bitmap.Dispose();
        Images.Add(image);
    }

    //public override Type GetControlType() => typeof(ImageInstrumentControl);
}
