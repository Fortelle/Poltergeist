using System.Drawing;
using Poltergeist.Automations.Processors;
using Poltergeist.Automations.Utilities.Windows;
using Poltergeist.Operations.Locating;

namespace Poltergeist.Operations.Capturing;

public class PrintWindowCapturingService : CapturingProvider
{
    private readonly WindowLocatingService WindowLocatingService;

    public PrintWindowCapturingService(MacroProcessor processor, WindowLocatingService windowLocatingService) : base(processor, windowLocatingService)
    {
        WindowLocatingService = windowLocatingService;

        CaptureClientFullHandler = CaptureClientFullImpl;
    }

    private Bitmap CaptureClientFullImpl()
    {
        var hwnd = WindowLocatingService.Handle;
        var size = WindowLocatingService.ClientSize!.Value;

        Logger.Trace($"Capturing an image of the window.", new { hwnd, size });

        var bmp = WindowUtil.Capture(hwnd, size);

        Logger.Debug($"Captured an image of the window.", new { hwnd, size });

        return bmp;
    }

    public static Bitmap? Capture(RegionConfig config)
    {
        if (WindowLocatingService.Locate(config, out var info) != LocateResult.Succeeded)
        {
            return null;
        }

        var bmp = WindowUtil.Capture(info!.Handle, info.ClientArea.Size);

        if (info.ClientArea.Size != config.WorkspaceSize)
        {
            var newBmp = ResizeImage(bmp, config.WorkspaceSize!.Value);
            bmp.Dispose();
            bmp = newBmp;
        }

        return bmp;
    }
}
