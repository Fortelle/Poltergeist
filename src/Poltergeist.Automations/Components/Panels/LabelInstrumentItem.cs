using Poltergeist.Automations.Structures;
using Poltergeist.Automations.Structures.Colors;

namespace Poltergeist.Automations.Components.Panels;

public class LabelInstrumentItem
{
    public string? Key { get; set; }

    public string? Label { get; set; }
    public string? Text { get; set; }
    public string? Tooltip { get; set; }

    public IconInfo? Icon { get; set; }

    public string? TemplateKey { get; set; }

    public ThemeColor? Color { get; set; }

    public LabelInstrumentItem()
    {
    }

    public LabelInstrumentItem(string templateKey)
    {
        TemplateKey = templateKey;
    }
}