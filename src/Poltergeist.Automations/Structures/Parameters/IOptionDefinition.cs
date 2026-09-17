namespace Poltergeist.Automations.Structures.Parameters;

public interface IOptionDefinition : IParameterDefinition
{
    bool IsGlobal { get; }
    string? Parent { get; }
}
