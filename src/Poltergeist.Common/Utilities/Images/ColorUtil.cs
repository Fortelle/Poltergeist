using System;
using System.Drawing;

namespace Poltergeist.Common.Utilities.Images;

public static class ColorUtil
{
    public static double MaxDistance = GetDistance(0, 0, 0, 255, 255, 255);

    public static double GetDistance(Color c1, Color c2)
    {
        var distance = Math.Pow(c1.R - c2.R, 2) + Math.Pow(c1.G - c2.G, 2) + Math.Pow(c1.B - c2.B, 2);
        return Math.Sqrt(distance);
    }

    public static double GetDistance(byte r1, byte g1, byte b1, byte r2, byte g2, byte b2)
    {
        var distance = Math.Pow(r1 - r2, 2) + Math.Pow(g1 - g2, 2) + Math.Pow(b1 - b2, 2);
        return Math.Sqrt(distance);
    }

    public static byte ToLumaGrayscale(byte r, byte g, byte b)
    {
        return (byte)Math.Round(r * .299 + g * .587 + b * .114);
    }

    public static byte ToAverage(byte r, byte g, byte b)
    {
        return (byte)((r + g + b) / 3);
    }

    public static byte Lightness(byte r, byte g, byte b)
    {
        var m = Math.Max(Math.Max(r, g), b);
        var n = Math.Min(Math.Min(r, g), b);
        return (byte)((m + n) / 2);
    }

    public static byte ToDecompositionMax(byte r, byte g, byte b)
    {
        return Math.Max(Math.Max(r, g), b);
    }

    public static byte ToDecompositionMin(byte r, byte g, byte b)
    {
        return Math.Min(Math.Min(r, g), b);
    }

#pragma warning disable IDE0060
    public static bool IsRed(byte r, byte g, byte b)
    {
        return r > 127;
    }

    public static bool IsGreen(byte r, byte g, byte b)
    {
        return g > 127;
    }

    public static bool IsBlue(byte r, byte g, byte b)
    {
        return b > 127;
    }
#pragma warning restore IDE0060

    public static bool IsAverage(byte r, byte g, byte b)
    {
        return r + g + b > 381;
    }

    public static bool IsLumaGrayscale(byte r, byte g, byte b)
    {
        return r * .299 + g * .587 + b * .114 > 186;
    }

}
