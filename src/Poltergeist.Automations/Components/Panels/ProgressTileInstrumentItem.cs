namespace Poltergeist.Automations.Components.Panels;

public class ProgressTileInstrumentItem : TileInstrumentItem
{
    public ProgressTileInstrumentItem()
    {
    }

    public ProgressTileInstrumentItem(ProgressStatus status)
    {
        TemplateKey = status.ToString();
    }

    public ProgressTileInstrumentItem(ProgressInstrumentInfo info)
    {
        TemplateKey = info.Status?.ToString();
        Tooltip = info.Tooltip;
        Icon = info.Icon;
        Color = info.Color;
    }
}
