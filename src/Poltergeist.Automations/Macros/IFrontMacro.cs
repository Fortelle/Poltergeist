namespace Poltergeist.Automations.Macros;

public interface IFrontMacro : IMacroBase, IFrontBackMacro
{
    string? Category { get; }
    string? Description { get; }
    string[]? Details { get; }
    string[]? Tags { get; }
    string? Icon { get; }
    Version? Version { get; }
}
