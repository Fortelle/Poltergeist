using Microsoft.UI.Xaml.Media;
using Poltergeist.Automations.Components.Panels;
using Poltergeist.Automations.Structures;
using Poltergeist.Automations.Structures.Colors;
using Poltergeist.Helpers;

namespace Poltergeist.UI.Controls.Instruments;

public class TileInstrumentItemViewModel
{
    public IconInfo? Icon { get; set; }

    public string? Tooltip { get; set; }

    public SolidColorBrush? Background { get; set; }

    public SolidColorBrush? Foreground { get; set; }

    public TileInstrumentItemViewModel(TileInstrumentItem item)
    {
        Icon = item.Icon;
        Tooltip = item.Tooltip;

        if (item.Color is not null && ThemeColors.Colors.TryGetValue(item.Color.Value, out var colorset))
        {
            Foreground = new SolidColorBrush(ColorUtil.ToColor(colorset.Foreground));
            Background = new SolidColorBrush(ColorUtil.ToColor(colorset.Background));
        }
    }
}
