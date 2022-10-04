using System;
using System.Drawing;
using System.Windows.Media;
using Poltergeist.Automations.Processors;
using Poltergeist.Common.Converters;

namespace Poltergeist.Automations.Instruments;

public class ImageInstrument : InstrumentModel<ImageInstrumentControl, ImageInstrumentViewModel>
{
    public int Max { get; set; }

    public event Action<int, ImageSource> Changed;

    public ImageInstrument(MacroProcessor processor) : base(processor)
    {
        Max = 1;
    }

    public void Add(Bitmap bitmap)
    {
        var newBitmap = new Bitmap(bitmap);
        Processor.RaiseAction(() =>
        {
            var image = BitmapToImageSourceConverter.ToImageSource(newBitmap);
            Changed?.Invoke(-1, image);
            newBitmap.Dispose();
        });
    }

    public void Update(int index, Bitmap bitmap)
    {
        var newBitmap = new Bitmap(bitmap);
        Processor.RaiseAction(() =>
        {
            var image = BitmapToImageSourceConverter.ToImageSource(newBitmap);
            Changed?.Invoke(index, image);
            newBitmap.Dispose();
        });
    }

    public override ImageInstrumentViewModel CreateViewModel()
    {
        return new ImageInstrumentViewModel(this);
    }
}
