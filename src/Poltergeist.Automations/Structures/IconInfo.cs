using System.Text;

namespace Poltergeist.Automations.Structures;

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

    public static bool IsEmoji(Rune r)
    {
        return r.Value is (>= 0x1F300 and <= 0x1FFFF) or (>= 0x2300 and <= 0x27FF);
    }
}
