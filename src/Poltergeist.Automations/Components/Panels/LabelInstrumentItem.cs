using Poltergeist.Automations.Structures;
using Poltergeist.Automations.Structures.Colors;

namespace Poltergeist.Automations.Components.Panels;

public class LabelInstrumentItem
{
    public string? Key { get; set; }

    public string? Tooltip { get; set; }

    public IconInfo? HeaderIcon { get; set; }
    public string? Header { get; set; }

    public string? Text { get; set; }
    public string? Subtext { get; set; }
    public IconInfo? LeftIcon { get; set; }
    public IconInfo? RightIcon { get; set; }
    public string? LeftText { get; set; }
    public string? RightText { get; set; }

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