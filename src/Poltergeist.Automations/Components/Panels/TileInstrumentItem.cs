using Poltergeist.Automations.Structures.Colors;

namespace Poltergeist.Automations.Components.Panels;

public class TileInstrumentItem
{
    public int? Index { get; set; }

    public string? Glyph { get; set; }
    public string? Text { get; set; }
    public string? Emoji { get; set; }

    public string? Tooltip { get; set; }

    public string? TemplateKey { get; set; }

    public ThemeColor? Color { get; set; }

    public TileInstrumentItem()
    {
    }

    public TileInstrumentItem(string templateKey)
    {
        TemplateKey = templateKey;
    }
}
