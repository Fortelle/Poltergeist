namespace Poltergeist.Automations.Configs;

public interface IChoiceOptionItem : IOptionItem
{
    public ChoiceOptionMode Mode { get; }
    public ChoiceEntry[] GetChoices();
}
