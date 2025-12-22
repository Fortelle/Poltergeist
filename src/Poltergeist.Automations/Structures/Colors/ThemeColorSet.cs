using System.Drawing;

namespace Poltergeist.Automations.Structures.Colors;

public class ThemeColorSet
{
    public Color Color { get; set; }
    public Color Background { get; set; }
    public Color Foreground { get; set; }

    public ThemeColorSet(Color color)
    {
        Color = color;
    }

    public ThemeColorSet(Color color, Color background, Color foreground)
    {
        Color = color;
        Background = background;
        Foreground = foreground;
    }

    public ThemeColorSet(string color)
    {
        Color = HexToColor(color);
    }

    public ThemeColorSet(string color, string background, string foreground)
    {
        Color = HexToColor(color);
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
