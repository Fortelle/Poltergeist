namespace Poltergeist.Automations.Components.Panels;

public class ProgressListInstrumentItem : ListInstrumentItem
{
    public ProgressListInstrumentItem()
    {
    }

    public ProgressListInstrumentItem(ProgressStatus status)
    {
        TemplateKey = status.ToString();
    }

    public ProgressListInstrumentItem(ProgressInstrumentInfo info)
    {
        TemplateKey = info.Status?.ToString();
        Text = info.Text;
        Subtext = info.Subtext;
        Glyph = info.Glyph;
        Emoji = info.Emoji;
        Color = info.Color;

        Progress = info.ProgressValue switch
        {
            0 => 0,
            -1 => 1,
            > 0 when info.ProgressMax > 0 => (double)info.ProgressValue / info.ProgressMax,
            _ => null,
        };
    }
}
