using Microsoft.UI.Xaml.Media;
using Poltergeist.Automations.Components.Panels;
using Poltergeist.Automations.Structures;
using Poltergeist.Automations.Structures.Colors;
using Poltergeist.Helpers;

namespace Poltergeist.UI.Controls.Instruments;

public class LabelInstrumentItemViewModel
{
    public string? Label { get; set; }

    public string? Text { get; set; }

    public string? Tooltip { get; set; }

    public SolidColorBrush? Foreground { get; set; }

    public SolidColorBrush? Background { get; set; }

    public IconInfo? Icon { get; set; }

    public LabelInstrumentItemViewModel(LabelInstrumentItem item)
    {
        Tooltip = item.Tooltip;
        Text = item.Text;
        Label = item.Label;
        Icon = item.Icon;

        if (item.Color is not null && ThemeColors.Colors.TryGetValue(item.Color.Value, out var colorset))
        {
            Foreground = new SolidColorBrush(ColorUtil.ToColor(colorset.Foreground));
            Background = new SolidColorBrush(ColorUtil.ToColor(colorset.Background));
        }
    }
}
