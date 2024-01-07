using Poltergeist.Common.Structures.Colors;

namespace Poltergeist.Automations.Components.Panels;

public class ProgressInstrumentInfo
{
    public string? Text { get; set; }
    public string? Subtext { get; set; }
    public string? Tooltip { get; set; }

    public int? ProgressValue { get; set; }
    public int? ProgressMax { get; set; }

    public string? Glyph { get; set; }
    public string? Emoji { get; set; }
    public ThemeColor? Color { get; set; }

    internal ProgressStatus? Status { get; set; }
}
