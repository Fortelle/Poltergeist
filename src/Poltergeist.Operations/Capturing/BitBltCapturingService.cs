using System.Drawing;
using Poltergeist.Automations.Processors;
using Poltergeist.Automations.Utilities.Windows;
using Poltergeist.Operations.Locating;

namespace Poltergeist.Operations.Capturing;

public class BitBltCapturingService : CapturingProvider
{
    private readonly WindowLocatingService WindowLocatingService;

    public BitBltCapturingService(MacroProcessor processor, WindowLocatingService windowLocatingService) : base(processor, windowLocatingService)
    {
        WindowLocatingService = windowLocatingService;

        CaptureClientPartHandler = CaptureClientPartImpl;
        CaptureClientPartsHandler = CaptureClientPartsImpl;
    }

    private Bitmap CaptureClientPartImpl(Rectangle rectangleOnClient)
    {
        var hwnd = WindowLocatingService.Handle;
        var size = WindowLocatingService.ClientSize;

        Logger.Trace($"Capturing an image from the window.", new { hwnd, size, rectangleOnClient });

        var images = BitBltHelper.Capture(hwnd, rectangleOnClient);

        Logger.Debug($"Captured an image from the window.", new { hwnd, size, rectangleOnClient });

        return images;
    }

    private Bitmap[] CaptureClientPartsImpl(Rectangle[] rectanglesOnClient)
    {
        var hwnd = WindowLocatingService.Handle;
        var size = WindowLocatingService.ClientSize;

        Logger.Trace($"Capturing images from the window.", new { hwnd, size, rectanglesOnClient });

        var images = BitBltHelper.Capture(hwnd, rectanglesOnClient);

        Logger.Debug($"Captured {images.Length} images from the window.", new { hwnd, size, rectanglesOnClient });

        return images;
    }
}
