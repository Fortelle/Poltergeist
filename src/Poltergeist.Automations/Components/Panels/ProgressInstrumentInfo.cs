using Poltergeist.Automations.Structures;
using Poltergeist.Automations.Structures.Colors;

namespace Poltergeist.Automations.Components.Panels;

public class ProgressInstrumentInfo
{
    public string? Text { get; set; }
    public string? Subtext { get; set; }
    public string? Tooltip { get; set; }

    public int? ProgressValue { get; set; }
    public int? ProgressMax { get; set; }

    public IconInfo? Icon { get; set; }
    public ThemeColor? Color { get; set; }

    internal ProgressStatus? Status { get; set; }
}
