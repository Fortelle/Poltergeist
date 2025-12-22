using Microsoft.UI.Xaml.Media;
using Poltergeist.Automations.Components.Panels;
using Poltergeist.Automations.Structures;
using Poltergeist.Automations.Structures.Colors;
using Poltergeist.Helpers;

namespace Poltergeist.UI.Controls.Instruments;

public class IndicatorInstrumentItemViewModel
{
    public SolidColorBrush? Foreground { get; set; }
    public SolidColorBrush? Background { get; set; }
    public SolidColorBrush? BorderColor { get; set; }

    public IconInfo? Icon { get; set; }

    public string? Tooltip { get; set; }

    public IndicatorMotion? Motion { get; set; }

    public IndicatorInstrumentItemViewModel(IndicatorInstrumentItem item)
    {
        Icon = item.Icon;
        Motion = item.Motion;
        Tooltip = item.Tooltip;

        var color = item.Color ?? ThemeColor.Gray;
        if (ThemeColors.Colors.TryGetValue(color, out var colorset))
        {
            if (item.Filled == true)
            {
                Foreground = new SolidColorBrush(ColorUtil.ToColor(colorset.Foreground));
                Background = new SolidColorBrush(ColorUtil.ToColor(colorset.Background));
                BorderColor = new SolidColorBrush(ColorUtil.ToColor(colorset.Background));
            }
            else if (item.Bordered == true)
            {
                Foreground = new SolidColorBrush(ColorUtil.ToColor(colorset.Color));
                BorderColor = new SolidColorBrush(ColorUtil.ToColor(colorset.Color));
            }
            else
            {
                Foreground = new SolidColorBrush(ColorUtil.ToColor(colorset.Color));
            }
        }
    }
}
