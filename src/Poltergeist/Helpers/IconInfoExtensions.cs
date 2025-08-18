using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using Poltergeist.Automations.Structures;

namespace Poltergeist.Helpers;

public static class IconInfoHelper
{
    public static IconSource? ConvertToIconSource(IconInfo? info)
    {
        if (info is null)
        {
            return null;
        }

        if (info.Glyph is not null)
        {
            return new FontIconSource()
            {
                Glyph = info.Glyph,
            };
        }
        else if (info.Uri is not null)
        {
            return new ImageIconSource()
            {
                ImageSource = new BitmapImage(new Uri(info.Uri)),
            };
        }
        else if (info.Emoji is not null)
        {
            return new FontIconSource()
            {
                FontFamily = new("Segoe UI Emoji"),
                Glyph = info.Emoji,
            };
        }
        return null;
    }

    public static IconElement? ConvertToIconElement(IconInfo? info)
    {
        if (info is null)
        {
            return null;
        }
        if (info.Glyph is not null)
        {
            return new FontIcon()
            {
                Glyph = info.Glyph,
            };
        }
        else if (info.Uri is not null)
        {
            return new ImageIcon()
            {
                Source = new BitmapImage(new Uri(info.Uri)),
            };
        }
        else if (info.Emoji is not null)
        {
            return new FontIcon()
            {
                FontFamily = new("Segoe UI Emoji"),
                Glyph = info.Emoji,
            };
        }
        return null;
    }
}
