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
}
