namespace Poltergeist.Automations.Structures.Parameters;

public interface IChoiceOption : IParameterDefinition
{
    public ChoiceOptionMode Mode { get; }
    public ChoiceEntry[] GetChoices();
}
