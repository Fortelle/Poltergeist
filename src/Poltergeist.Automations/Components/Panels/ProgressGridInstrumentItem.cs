namespace Poltergeist.Automations.Components.Panels;

public class ProgressGridInstrumentItem : GridInstrumentItem
{
    public ProgressGridInstrumentItem()
    {
    }

    public ProgressGridInstrumentItem(ProgressStatus status)
    {
        TemplateKey = status.ToString();
    }

    public ProgressGridInstrumentItem(ProgressInstrumentInfo info)
    {
        TemplateKey = info.Status?.ToString();
        Text = info.Text;
        Tooltip = info.Tooltip;
        Glyph = info.Glyph;
        Emoji = info.Emoji;
        Color = info.Color;
    }
}
