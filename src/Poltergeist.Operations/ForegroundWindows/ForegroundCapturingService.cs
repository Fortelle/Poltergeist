using System.Drawing;
using System.Drawing.Imaging;
using Poltergeist.Automations.Processors;

namespace Poltergeist.Operations.ForegroundWindows;

public class ForegroundCapturingService : CapturingSource
{
    private ForegroundLocatingService Locating { get; }

    public ForegroundCapturingService(
        MacroProcessor processor,
        ForegroundLocatingService locating
        )
        : base(processor)
    {
        Locating = locating;
    }


    public override Bitmap DoCapture()
    {
        var clientArea = new Rectangle(Point.Empty, Locating.ClientRegion.Size);

        return DoCapture(clientArea);
    }

    public override Bitmap DoCapture(Rectangle clientArea)
    {
        var begintime = DateTime.Now;

        var screenArea = Locating.RectangleToScreen(clientArea);
        var bmp = CaptureFromScreen(screenArea);

        var endtime = DateTime.Now;
        var duration = endtime - begintime;

        Logger.Debug($"Captured an image from screen.", new { clientArea, screenArea, duration });

        return bmp;
    }

    public static Bitmap CaptureFromScreen(Rectangle screen)
    {
        var bmp = new Bitmap(screen.Width, screen.Height, PixelFormat.Format32bppRgb);
        using var gra = Graphics.FromImage(bmp);
        gra.CopyFromScreen(screen.Location, default, screen.Size, CopyPixelOperation.SourceCopy);
        return bmp;
    }

    public static Bitmap? Capture(RegionConfig config)
    {
        var result = ForegroundLocatingService.TryLocate(config, out var client);
        if (result != LocateResult.Succeeded)
        {
            return null;
        }

        return CaptureFromScreen(client);
    }

}
