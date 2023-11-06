using System.Drawing;
using Poltergeist.Automations.Components.Panels;
using Poltergeist.Automations.Processors;
using Poltergeist.Automations.Services;
using Poltergeist.Common.Utilities.Images;

namespace Poltergeist.Operations;

public abstract class CapturingProvider : MacroService
{
    public const string PreviewCaptureKey = "preview_capture";

    public abstract Bitmap DoCapture();
    public abstract Bitmap DoCapture(Rectangle area);

    protected Bitmap? CachedImage { get; set; }
    protected ImageInstrument? Instrument { get; set; }

    public CapturingProvider(MacroProcessor processor) : base(processor)
    {
        var isPreviewable = Processor.GetOption<bool>(PreviewCaptureKey);
        if (isPreviewable)
        {
            var inst = processor.GetService<DashboardService>();
            Instrument = inst.Create<ImageInstrument>(inst =>
            {
                inst.Key = PreviewCaptureKey;
                inst.Title = "Preview Capture:";
            });
        }
    }

    public Bitmap Capture()
    {
        Bitmap bmp;

        if (CachedImage == null)
        {
            bmp = DoCapture();
            Instrument?.Add(new(new Bitmap(bmp)));
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
            Instrument?.Add(new(new Bitmap(bmp)));
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

    public void Cache(bool refresh = false)
    {
        var hasCache = CachedImage != null;

        if (hasCache)
        {
            if (refresh)
            {
                ReleaseCache();
            }
            else
            {
                return;
            }
        }

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
