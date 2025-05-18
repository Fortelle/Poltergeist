using System.Drawing;
using Poltergeist.Automations.Processors;
using Poltergeist.Automations.Utilities.Windows;
using Poltergeist.Automations.Utilities.Images;

namespace Poltergeist.Operations.Background;

public class BackgroundCapturingService : CapturingProvider
{
    private readonly BackgroundLocatingService Locating;

    public BackgroundCapturingService(MacroProcessor processor, BackgroundLocatingService locating) : base(processor)
    {
        Locating = locating;
    }

    protected override Bitmap DoCapture()
    {
        var begintime = DateTime.Now;

        var hwnd = Locating.SendMessage.Hwnd;
        var size = Locating.ClientSize;
        var bmp = WindowHelper.Capture(hwnd, size, 2);

        var endtime = DateTime.Now;
        var duration = endtime - begintime;

        Logger.Debug($"Captured an image of the background window.", new { hwnd = (ulong)hwnd, clientSize = size, duration });
        
        return bmp;
    }

    protected override Bitmap DoCapture(Rectangle area)
    {
        using var bmp = DoCapture();
        var bmp2 = BitmapUtil.Crop(bmp, area);
        return bmp2;
    }

    public static Bitmap? Capture(RegionConfig config)
    {
        var result = BackgroundLocatingService.TryLocate(config, out var hwnd, out var size);
        return result == LocateResult.Succeeded ? WindowHelper.Capture(hwnd, size, 2) : null;
    }

}
