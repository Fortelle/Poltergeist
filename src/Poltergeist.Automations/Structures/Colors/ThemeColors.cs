using Windows.UI;

namespace Poltergeist.Automations.Structures.Colors;

public static class ThemeColors
{
    private const double BaseSaturation = .8;
    private const double BaseLightLightness = .85;
    private const double BaseDarkLightness = .15;

    public static Dictionary<ThemeColor, ThemeColorSet> Colors { get; } = new()
    {
        [ThemeColor.Red] = new(HSL(0, BaseSaturation, BaseLightLightness), HSL(0, BaseSaturation, BaseDarkLightness)),
        [ThemeColor.Orange] = new(HSL(30, BaseSaturation, BaseLightLightness), HSL(30, BaseSaturation, BaseDarkLightness)),
        [ThemeColor.Yellow] = new(HSL(60, BaseSaturation, BaseLightLightness), HSL(60, BaseSaturation, BaseDarkLightness)),
        [ThemeColor.Chartreuse] = new(HSL(90, BaseSaturation, BaseLightLightness), HSL(90, BaseSaturation, BaseDarkLightness)),
        [ThemeColor.Green] = new(HSL(120, BaseSaturation, BaseLightLightness), HSL(120, BaseSaturation, BaseDarkLightness)),
        [ThemeColor.Spring] = new(HSL(150, BaseSaturation, BaseLightLightness), HSL(150, BaseSaturation, BaseDarkLightness)),
        [ThemeColor.Cyan] = new(HSL(180, BaseSaturation, BaseLightLightness), HSL(180, BaseSaturation, BaseDarkLightness)),
        [ThemeColor.Azure] = new(HSL(210, BaseSaturation, BaseLightLightness), HSL(210, BaseSaturation, BaseDarkLightness)),
        [ThemeColor.Blue] = new(HSL(240, BaseSaturation, BaseLightLightness), HSL(240, BaseSaturation, BaseDarkLightness)),
        [ThemeColor.Violet] = new(HSL(270, BaseSaturation, BaseLightLightness), HSL(270, BaseSaturation, BaseDarkLightness)),
        [ThemeColor.Magenta] = new(HSL(300, BaseSaturation, BaseLightLightness), HSL(300, BaseSaturation, BaseDarkLightness)),
        [ThemeColor.Rose] = new(HSL(330, BaseSaturation, BaseLightLightness), HSL(330, BaseSaturation, BaseDarkLightness)),

        [ThemeColor.Gray] = new(HSL(0, 0, BaseLightLightness), HSL(0, 0, BaseDarkLightness)),
    };

    private static Color HSL(double h, double s, double l)
    {
        var c = (1 - Math.Abs(2 * l - 1)) * s;
        var x = c * (1 - Math.Abs(h / 60 % 2 - 1));
        var m = l - c / 2;

        double r, g, b;
        if (h >= 0 && h < 60)
        {
            r = c; g = x; b = 0;
        }
        else if (h >= 60 && h < 120)
        {
            r = x; g = c; b = 0;
        }
        else if (h >= 120 && h < 180)
        {
            r = 0; g = c; b = x;
        }
        else if (h >= 180 && h < 240)
        {
            r = 0; g = x; b = c;
        }
        else if (h >= 240 && h < 300)
        {
            r = x; g = 0; b = c;
        }
        else
        {
            r = c; g = 0; b = x;
        }

        // Convert RGB to byte values
        var rByte = (byte)((r + m) * 255);
        var gByte = (byte)((g + m) * 255);
        var bByte = (byte)((b + m) * 255);

        return Color.FromArgb(255, rByte, gByte, bByte);
    }


}
