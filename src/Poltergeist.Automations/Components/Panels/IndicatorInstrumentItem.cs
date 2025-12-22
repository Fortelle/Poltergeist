using System.Diagnostics.CodeAnalysis;
using Poltergeist.Automations.Structures;
using Poltergeist.Automations.Structures.Colors;

namespace Poltergeist.Automations.Components.Panels;

public class IndicatorInstrumentItem
{
    public required string PatternKey { get; set; }

    public IconInfo? Icon { get; set; }

    public string? Tooltip { get; set; }

    public ThemeColor? Color { get; set; }

    public IndicatorMotion? Motion { get; set; }

    public bool? Bordered { get; set; }

    public bool? Filled { get; set; }

    [SetsRequiredMembers]
    public IndicatorInstrumentItem(string patternKey)
    {
        PatternKey = patternKey;
    }
}
