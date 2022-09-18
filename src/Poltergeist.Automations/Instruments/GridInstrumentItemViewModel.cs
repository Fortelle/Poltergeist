using System.Windows.Media;

namespace Poltergeist.Automations.Instruments;

public class GridInstrumentItemViewModel
{
    public ProgressStatus Status { get; set; }

    public string Text { get; set; }

    public string Tooltip { get; set; }

    public int Size { get; set; }

    public SolidColorBrush Background { get; set; }
    public SolidColorBrush Foreground { get; set; }

    public GridInstrumentItemViewModel(GridInstrumentItem item)
    {
        Status = item.Status;
        Text = item.Text;
        Tooltip = item.Tooltip;
    }
}
