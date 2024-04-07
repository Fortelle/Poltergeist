namespace Poltergeist.Automations.Parameters;

public interface IParameterDefinition
{
    public string Key { get; }

    public string? DisplayLabel { get; }
    public string? Category { get; }
    public string? Description { get; }
    public ParameterStatus Status { get; }

    public Type BaseType { get; }

    public object? DefaultValue { get; }

    public bool IsDefault(object? value);
    public string FormatValue(object? value);
}
