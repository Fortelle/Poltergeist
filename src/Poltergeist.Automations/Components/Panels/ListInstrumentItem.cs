using Poltergeist.Common.Structures.Colors;

namespace Poltergeist.Automations.Components.Panels;

// todo: merge glyph and emoji to a single property and support 3rd icons (eg "fa-flag")
// todo: implement the dynamic icon effect and duration timer in ui layer
// var icons = new[] { "🕐", "🕑", "🕒", "🕓", "🕔", "🕕", "🕖", "🕗", "🕘", "🕙", "🕚", "🕛" };

public class ListInstrumentItem
{
    public string? Text { get; set; }

    public string? Key { get; set; }
    public int? Index { get; set; }
    public string? Subtext { get; set; }
    public double? Progress { get; set; }

    public string? TemplateKey { get; set; }
    public string? Glyph { get; set; }
    public string? Emoji { get; set; }
    public ThemeColor? Color { get; set; }

    public string? InstrumentKey { get; set; }
    public string? MacroKey { get; set; }
    public string? ProcessId { get; set; }

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
