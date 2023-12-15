namespace Poltergeist.Automations.Parameters;

public interface IChoiceOptionItem : IOptionItem
{
    public ChoiceOptionMode Mode { get; }
    public ChoiceEntry[] GetChoices();
}
