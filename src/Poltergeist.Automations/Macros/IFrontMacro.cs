namespace Poltergeist.Automations.Macros;

public interface IFrontMacro : IFrontBackMacro
{
    public string? Category { get; }
    public string? Description { get; }
    public string[]? Details { get; }
    public string[]? Tags { get; }
    public string? Icon { get; }
    public Version? Version { get; }

    public bool IsSingleton { get; }
}
