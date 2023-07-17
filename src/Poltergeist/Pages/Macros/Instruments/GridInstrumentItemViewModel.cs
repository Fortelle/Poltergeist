using Microsoft.UI.Xaml.Media;
using Poltergeist.Automations.Components.Panels;

namespace Poltergeist.Macros.Instruments;

public class GridInstrumentItemViewModel
{
    public string? Text { get; set; }

    public string? Emoji { get; set; }

    public string? Tooltip { get; set; }

    public SolidColorBrush? Background { get; set; }

    public SolidColorBrush? Foreground { get; set; }

    public string? Glyph { get; set; }

    public GridInstrumentItemViewModel(GridInstrumentItem item)
    {
        Tooltip = item.Tooltip;
        Text = item.Text;
        Emoji = item.Emoji;
        Glyph = item.Glyph;
    }
}
