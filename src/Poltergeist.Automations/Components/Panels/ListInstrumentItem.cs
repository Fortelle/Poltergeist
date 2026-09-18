using Poltergeist.Automations.Structures;
using Poltergeist.Automations.Structures.Colors;

namespace Poltergeist.Automations.Components.Panels;

public class ListInstrumentItem
{
    public string? Text { get; set; }

    public string? Key { get; set; }
    public int? Index { get; set; }
    public string? Subtext { get; set; }
    public double? Progress { get; set; }

    public string? TemplateKey { get; set; }
    public IconInfo? Icon { get; set; }
    public ThemeColor? Color { get; set; }

    public string? InstrumentKey { get; set; }
    public string? MacroKey { get; set; }
    public string? ProcessorId { get; set; }

    public ListInstrumentButton[]? Buttons { get; set; }

    public ListInstrumentItem()
    {
    }

    public ListInstrumentItem(string text, string? subtext = null)
    {
        Text = text;
        Subtext = subtext;
    }

    public ListInstrumentItem(string key, string text, string? subtext = null)
    {
        Key = key;
        Text = text;
        Subtext = subtext;
    }

    public override string ToString()
    {
        var s = Text ?? "";
        if (!string.IsNullOrEmpty(Subtext))
        {
            s += "(" + Subtext + ")";
        }
        return s;
    }

}
