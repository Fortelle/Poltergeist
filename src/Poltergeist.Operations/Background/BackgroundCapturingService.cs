using System.Drawing;
using Poltergeist.Automations.Processors;
using Poltergeist.Common.Utilities.Images;
using Poltergeist.Input.Windows;

namespace Poltergeist.Operations.Background;

public class BackgroundCapturingService : CapturingProvider
{
    private BackgroundLocatingService Locating { get; }

    public BackgroundCapturingService(
        MacroProcessor processor,
        BackgroundLocatingService locating
        )
        : base(processor)
    {
        Locating = locating;
    }

    public override Bitmap DoCapture()
    {
        var begintime = DateTime.Now;

        var hwnd = Locating.SendMessage.Hwnd;
        var size = Locating.ClientSize;
        var bmp = WindowHelper.Capture(hwnd, size, 2);

        var endtime = DateTime.Now;
        var duration = endtime - begintime;

        Logger.Debug($"Captured an image from background window.", new { hwnd, clientSize = size, duration });
        return bmp;
    }

    public override Bitmap DoCapture(Rectangle area)
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
