using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using Poltergeist.Automations.Components.Panels;
using Poltergeist.Automations.Structures;
using Poltergeist.Automations.Structures.Colors;
using Poltergeist.Helpers;

namespace Poltergeist.UI.Controls.Instruments;

public class LabelInstrumentItemViewModel
{
    public string? Tooltip { get; set; }

    public SolidColorBrush? Foreground { get; set; }

    public SolidColorBrush? Background { get; set; }

    public IconInfo? HeaderIcon { get; set; }
    public string? Header { get; set; }

    public string? Text { get; set; }
    public string? Subtext { get; set; }
    public IconInfo? LeftIcon { get; set; }
    public IconInfo? RightIcon { get; set; }
    public string? LeftText { get; set; }
    public string? RightText { get; set; }

    public int HeaderRow { get; set; }

    public FlowDirection HeaderDirection { get; set; }

    public HorizontalAlignment HeaderAlignment { get; set; }

    public HorizontalAlignment TextAlignment { get; set; }

    public LabelInstrumentItemViewModel(LabelInstrumentItem item)
    {
        Tooltip = item.Tooltip;
        Header = item.Header;
        HeaderIcon = item.HeaderIcon;
        Text = item.Text;
        Subtext = item.Subtext;
        LeftIcon = item.LeftIcon;
        RightIcon = item.RightIcon;
        LeftText = item.LeftText;
        RightText = item.RightText;

        if (item.Color is not null && ThemeColors.Colors.TryGetValue(item.Color.Value, out var colorset))
        {
            Foreground = new SolidColorBrush(ColorHelper.ToColor(colorset.Foreground));
            Background = new SolidColorBrush(ColorHelper.ToColor(colorset.Background));
        }
    }
}
