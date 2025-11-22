using System.Drawing;
using System.Drawing.Imaging;
using Poltergeist.Automations.Processors;
using Poltergeist.Operations.Locating;

namespace Poltergeist.Operations.Capturing;

public class ScreenCapturingService : CapturingProvider
{
    private readonly ScreenLocatingService ScreenLocatingService;

    public ScreenCapturingService(MacroProcessor processor, ScreenLocatingService screenLocatingService) : base(processor, screenLocatingService)
    {
        ScreenLocatingService = screenLocatingService;

        CaptureClientPartHandler = CaptureClientPartImpl;
    }

    private Bitmap CaptureClientPartImpl(Rectangle rectangleOnClient)
    {
        var rectangleOnScreen = ScreenLocatingService.ClientRectangleToScreen(rectangleOnClient);

        Logger.Trace($"Capturing an image from the screen.", new { rectangleOnClient, rectangleOnScreen });

        var bmp = CaptureFromScreen(rectangleOnScreen);

        Logger.Debug($"Captured an image from the area {rectangleOnScreen} of the screen.");

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
        if (ScreenLocatingService.Locate(config, out var info) != LocateResult.Succeeded)
        {
            return null;
        }

        var bmp = CaptureFromScreen(info!.ClientArea);

        if (info!.ClientArea.Size != config.WorkspaceSize)
        {
            var newBmp = ResizeImage(bmp, config.WorkspaceSize!.Value);
            bmp.Dispose();
            bmp = newBmp;
        }

        return bmp;
    }
}
