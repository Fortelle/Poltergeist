﻿using System.Drawing;
using Poltergeist.Automations.Processors;
using Poltergeist.Automations.Utilities.Images;
using Poltergeist.Operations;

namespace Poltergeist.Android.Adb;

public class AdbCapturingService : CapturingProvider
{
    public AdbService Adb { get; }

    public AdbCapturingService(
        MacroProcessor processor,
        AdbService adb
        ) : base(processor)
    {
        Adb = adb;
    }

    // experimental
    protected override Bitmap DoCapture()
    {
        var begintime = DateTime.Now;

        var data = Adb.ExecOut("screencap -p");

        var endtime = DateTime.Now;
        var duration = endtime - begintime;

        using var ms = new MemoryStream(data);
        var bmp = (Bitmap)Image.FromStream(ms);

        Logger.Trace($"Captured a screenshot via adb screencap.", new { dataLength = data.Length, screencapSize = bmp.Size, duration });

        Cache(new Bitmap(bmp));

        return bmp;
    }

    protected override Bitmap DoCapture(Rectangle area)
    {
        using var bmp = DoCapture();
        var bmp2 = BitmapUtil.Crop(bmp, area);
        return bmp2;
    }
}
