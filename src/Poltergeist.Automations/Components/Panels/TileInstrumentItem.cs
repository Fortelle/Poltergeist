using Poltergeist.Automations.Structures;
using Poltergeist.Automations.Structures.Colors;

namespace Poltergeist.Automations.Components.Panels;

public class TileInstrumentItem
{
    public string? Key { get; set; }

    public IconInfo? Icon { get; set; }

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
