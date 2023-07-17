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
}
