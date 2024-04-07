using System.Text;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;

namespace Poltergeist.Automations.Common;

public class IconInfo
{
    public string? Emoji { get; set; }
    public string? Glyph { get; set; }
    public string? Text { get; set; }
    public string? Uri { get; set; }

    public IconInfo()
    {
    }

    public IconInfo(string value)
    {
        if (value.StartsWith("ms-appx:///"))
        {
            Uri = value;
            return;
        }

        var runes = value.EnumerateRunes().ToArray();
        if (runes.Length > 0 && IsEmoji(runes[0]))
        {
            Emoji = value;
        }
        else if (runes.Length >= 1 && (runes[0].Value is >= 0xE700 and < 0xF8FF))
        {
            Glyph = value;
        }
        else
        {
            Text = value;
        }
    }

    public bool IsIcon => Emoji is not null || Glyph is not null || Uri is not null;

    public static bool IsEmoji(Rune r)
    {
        return r.Value
            is (>= 0x1F300 and <= 0x1FFFF)
            or (>= 0x2300 and <= 0x27FF)
            ;
    }

    public IconSource? ToIconSource()
    {
        if (Glyph is not null)
        {
            return new FontIconSource()
            {
                Glyph = Glyph,
            };
        }
        else if (Uri is not null)
        {
            return new ImageIconSource()
            {
                ImageSource = new BitmapImage(new Uri(Uri)),
            };
        }
        else if (Emoji is not null)
        {
            return new FontIconSource()
            {
                FontFamily = new("Segoe UI Emoji"),
                Glyph = Emoji,
            };
        }
        return null;
    }

    public IconElement? ToIconElement()
    {
        if (Glyph is not null)
        {
            return new FontIcon()
            {
                Glyph = Glyph,
            };
        }
        else if (Uri is not null)
        {
            return new ImageIcon()
            {
                Source = new BitmapImage(new Uri(Uri)),
            };
        }
        else if (Emoji is not null)
        {
            return new FontIcon()
            {
                FontFamily = new("Segoe UI Emoji"),
                Glyph = Emoji,
            };
        }
        return null;
    }
}
