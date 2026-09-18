namespace Poltergeist.Automations.Structures;

public class GlyphIcon : IconInfo
{
    public string Glyph { get; }

    public GlyphIcon(string glyph)
    {
        Glyph = glyph;
    }

    public override string ToString() => Glyph;
}
