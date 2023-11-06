using Windows.UI;

namespace Poltergeist.Common.Structures.Colors;

public class ThemeColorSet
{
    public Color Background { get; set; }
    public Color Foreground { get; set; }

    public ThemeColorSet(Color background)
    {
        Background = background;
    }

    public ThemeColorSet(Color background, Color foreground)
    {
        Background = background;
        Foreground = foreground;
    }

    public ThemeColorSet(string background)
    {
        Background = HexToColor(background);
    }

    public ThemeColorSet(string background, string foreground)
    {
        Background = HexToColor(background);
        Foreground = HexToColor(foreground);
    }

    public static Color HexToColor(string hex)
    {
        hex = hex.Replace("#", "");

        byte a = 255;
        if (hex.Length == 8)
        {
            a = byte.Parse(hex[..2], System.Globalization.NumberStyles.HexNumber);
            hex = hex[2..];
        }

        var r = byte.Parse(hex[0..2], System.Globalization.NumberStyles.HexNumber);
        var g = byte.Parse(hex[2..4], System.Globalization.NumberStyles.HexNumber);
        var b = byte.Parse(hex[4..6], System.Globalization.NumberStyles.HexNumber);

        return Color.FromArgb(a, r, g, b);
    }

}
