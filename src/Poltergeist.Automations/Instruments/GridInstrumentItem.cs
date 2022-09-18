namespace Poltergeist.Automations.Instruments;

public class GridInstrumentItem
{
    public ProgressStatus Status { get; set; }

    public string Text { get; set; }

    public string Tooltip { get; set; }

    public GridInstrumentItem(ProgressStatus status)
    {
        Status = status;
    }
}
