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
        Text = info.Text;
        Tooltip = info.Tooltip;
        Glyph = info.Glyph;
        Emoji = info.Emoji;
        Color = info.Color;
    }
}
