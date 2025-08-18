namespace Poltergeist.Automations.Structures.Parameters;

public interface IChoiceOption : IParameterDefinition
{
    ChoiceOptionMode Mode { get; }
    ChoiceEntry[] GetChoices();
}
