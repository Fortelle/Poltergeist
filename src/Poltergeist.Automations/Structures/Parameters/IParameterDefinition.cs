namespace Poltergeist.Automations.Structures.Parameters;

public interface IParameterDefinition
{
    string Key { get; }

    string? DisplayLabel { get; }
    string? Category { get; }
    string? Description { get; }
    ParameterStatus Status { get; }
    
    Type ValueType { get; }

    object? DefaultValue { get; }

    bool IsDefault(object? value);
    string FormatValue(object? value);
}
