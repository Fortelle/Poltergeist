using System.Drawing;
using System.Drawing.Imaging;
using Poltergeist.Automations.Processors;

namespace Poltergeist.Operations.Foreground;

public class ForegroundCapturingService : CapturingProvider
{
    private readonly ForegroundLocatingService Locating;

    public ForegroundCapturingService(
        MacroProcessor processor,
        ForegroundLocatingService locating
        ) : base(processor)
    {
        Locating = locating;
    }

    protected override Bitmap DoCapture()
    {
        var clientArea = new Rectangle(Point.Empty, Locating.ClientRegion.Size);
        var screenArea = Locating.RectangleToScreen(clientArea);

        var bmp = CaptureFromScreen(screenArea);

        Logger.Debug($"Captured an image from the screen.", new { clientArea, screenArea });
        
        return bmp;
    }

    protected override Bitmap DoCapture(Rectangle clientArea)
    {
        var screenArea = Locating.RectangleToScreen(clientArea);

        var bmp = CaptureFromScreen(screenArea);

        Logger.Debug($"Captured an image from the screen.", new { clientArea, screenArea });

        return bmp;
    }

    private static Bitmap CaptureFromScreen(Rectangle screen)
    {
        var bmp = new Bitmap(screen.Width, screen.Height, PixelFormat.Format32bppRgb);
        using var gra = Graphics.FromImage(bmp);
        gra.CopyFromScreen(screen.Location, default, screen.Size, CopyPixelOperation.SourceCopy);
        return bmp;
    }

    public static Bitmap? Capture(RegionConfig config)
    {
        var result = ForegroundLocatingService.TryLocate(config, out var client, out _);
        if (result != LocateResult.Succeeded)
        {
            return null;
        }

        return CaptureFromScreen(client);
    }

}
