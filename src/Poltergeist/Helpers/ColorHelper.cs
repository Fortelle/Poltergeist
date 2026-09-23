using Windows.UI;

namespace Poltergeist.Helpers;

public static class ColorHelper
{
    public static Color ToColor(System.Drawing.Color color)
    {
        return Color.FromArgb(color.A, color.R, color.G, color.B);
    }
}