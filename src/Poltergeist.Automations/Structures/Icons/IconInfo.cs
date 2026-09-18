using System.Text;

namespace Poltergeist.Automations.Structures;

// todo: support 3rd icons (eg "fa-flag")

public abstract class IconInfo
{
    public IconAnimation? Animation { get; set; }

    public static IconInfo FromString(string value)
    {
        if (value.StartsWith("ms-appx:///"))
        {
            return new UriIcon(value);
        }

        var runes = value.EnumerateRunes().ToArray();
        if (runes.Length > 0 && IsEmoji(runes[0]))
        {
            return new EmojiIcon(value);
        }
        else if (runes.Length >= 1 && IsGlyph(runes[0]))
        {
            return new GlyphIcon(value);
        }
        else
        {
            return new TextIcon(value);
        }
    }

    public static bool IsEmoji(Rune r)
    {
        return r.Value is (>= 0x1F300 and <= 0x1FFFF) or (>= 0x2300 and <= 0x27FF);
    }

    public static bool IsGlyph(Rune r)
    {
        return r.Value is (>= 0xE700 and < 0xF8FF);
    }

    public static implicit operator IconInfo(string value) => FromString(value);
}
