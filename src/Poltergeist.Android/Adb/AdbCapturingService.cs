using System.Drawing;
using Poltergeist.Automations.Processors;
using Poltergeist.Operations.Capturing;

namespace Poltergeist.Android.Adb;

public class AdbCapturingService : CapturingProvider
{
    public AdbService AdbService { get; }

    public AdbCapturingService(MacroProcessor processor, AdbService adbService, AdbLocatingService adbLocatingService) : base(processor, adbLocatingService)
    {
        AdbService = adbService;

        CaptureClientFullHandler = CaptureClientFullImpl;
    }

    // experimental
    private Bitmap CaptureClientFullImpl()
    {
        Logger.Trace($"Capturing a screenshot from the android device.");

        var data = AdbService.ExecOut("screencap -p");

        using var ms = new MemoryStream(data);
        var bmp = (Bitmap)Image.FromStream(ms);

        Logger.Debug($"Captured a screenshot from the android device.", new { dataLength = data.Length, screencapSize = bmp.Size });

        return bmp;
    }
}
