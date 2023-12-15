namespace Poltergeist.Automations.Parameters;

public interface IParameterEntry
{
    public string Key { get; }
    public object? Value { get; set; }

    public string? DisplayLabel { get; }
    public string? DisplayValue { get; }
    public string? Category { get; }
    public string? Description { get; }

    public Type BaseType { get; }

    public bool HasChanged { get; set; }
}
