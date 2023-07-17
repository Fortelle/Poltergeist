using System.Drawing;
using Poltergeist.Automations.Processors;
using Poltergeist.Common.Utilities.Images;

namespace Poltergeist.Operations.Android;

public class AdbCapturingService : CapturingSource
{
    public AdbService Adb { get; }

    public AdbCapturingService(
        MacroProcessor processor,
        AdbService adb
        )
        : base(processor)
    {
        Adb = adb;
    }

    // experimental
    public override Bitmap DoCapture()
    {
        var begintime = DateTime.Now;

        var data = Adb.ExecOut("screencap -p");

        var endtime = DateTime.Now;
        var duration = endtime - begintime;

        using var ms = new MemoryStream(data);
        var bmp = (Bitmap)Image.FromStream(ms);

        Logger.Debug($"Captured an image via adb screencap.", new { dataLength = data.Length, screencapSize = bmp.Size, duration });
        return bmp;
    }

    public override Bitmap DoCapture(Rectangle area)
    {
        using var bmp = DoCapture();
        var bmp2 = BitmapUtil.Crop(bmp, area);
        return bmp2;
    }

}
