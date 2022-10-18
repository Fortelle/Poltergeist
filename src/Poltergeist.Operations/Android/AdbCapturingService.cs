using System;
using System.Drawing;
using System.IO;
using Poltergeist.Automations.Processors;
using Poltergeist.Common.Utilities.Images;
using Poltergeist.Common.Windows;

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
    public override Bitmap DoCapture(Rectangle? area)
    {
        var begintime = DateTime.Now;

        var data = Adb.ExecOut("screencap -p");

        var endtime = DateTime.Now;
        var duration = endtime - begintime;

        using var ms = new MemoryStream(data);
        var bmp = (Bitmap)Image.FromStream(ms);

        if (area.HasValue)
        {
            var bmp2 = BitmapUtil.Crop(bmp, area.Value);
            Logger.Debug($"Captured an image via adb screencap.", new { dataLength = data.Length, screencapSize = bmp.Size, areaLocation = area.Value.Location, areaSize = area.Value.Size, duration });

            bmp.Dispose();
            return bmp2;
        }
        else
        {
            Logger.Debug($"Captured an image via adb screencap.", new { dataLength = data.Length, screencapSize = bmp.Size, duration });
            return bmp;
        }
    }

}
