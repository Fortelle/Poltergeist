namespace Poltergeist.Automations.Parameters;

public interface IChoiceOption : IParameterDefinition
{
    public ChoiceOptionMode Mode { get; }
    public ChoiceEntry[] GetChoices();
}
