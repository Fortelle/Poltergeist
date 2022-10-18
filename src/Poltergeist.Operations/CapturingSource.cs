using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Poltergeist.Automations.Instruments;
using Poltergeist.Automations.Processors;
using Poltergeist.Automations.Services;
using Poltergeist.Common.Utilities.Images;

namespace Poltergeist.Operations;

public abstract class CapturingSource : MacroService
{
    public abstract Bitmap DoCapture(Rectangle? area);

    protected Bitmap CachedImage { get; set; }
    protected bool ShowPreview { get; set; }
    protected ImageInstrument Instrument { get; set; }

    public CapturingSource(MacroProcessor processor) : base(processor)
    {
        ShowPreview = Processor.GetOption<bool>("capture_preview");
        if (ShowPreview)
        {
            var inst = processor.GetService<InstrumentService>();
            Instrument = inst.Create<ImageInstrument>(inst =>
            {
                inst.Key = "capture_preview";
                inst.Title = "Preview Capture:";
            });
        }
    }

    public Bitmap Capture()
    {
        Bitmap bmp;

        if (CachedImage == null)
        {
            bmp = DoCapture(null);
            if (ShowPreview)
            {
                Instrument.Add(bmp);
            }
        }
        else
        {
            bmp = new Bitmap(CachedImage);

            Logger.Debug($"Captured an image from cached image.", new { CachedImage.Size });
        }

        return bmp;
    }

    public Bitmap Capture(Rectangle area)
    {
        Bitmap bmp;

        if (CachedImage == null)
        {
            bmp = DoCapture(area);
            if (ShowPreview)
            {
                Instrument.Add(bmp);
            }
        }
        else
        {
            bmp = BitmapUtil.Crop(CachedImage, area);

            Logger.Debug($"Captured an image from cached image.", new { area });
        }

        return bmp;
    }

    public Bitmap[] Capture(IEnumerable<Rectangle> areas)
    {
        var l = areas.Min(x => x.X);
        var t = areas.Min(x => x.Y);
        var r = areas.Max(x => x.Right);
        var b = areas.Max(x => x.Bottom);
        var rect = Rectangle.FromLTRB(l, t, r, b);
        using var bmp = Capture(rect);
        var bmps = areas.Select(x => BitmapUtil.Crop(bmp, new Rectangle(x.X - rect.X, x.Y - rect.Y, x.Width, x.Height))).ToArray();
        return bmps;
    }

    public void Cache()
    {
        ReleaseCache();
        CachedImage = Capture();
    }

    public void Cache(Bitmap img)
    {
        ReleaseCache();
        CachedImage = img;
    }

    public void ReleaseCache()
    {
        CachedImage?.Dispose();
        CachedImage = null;
    }

}
