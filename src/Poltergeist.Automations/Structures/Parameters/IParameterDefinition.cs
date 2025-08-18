namespace Poltergeist.Automations.Structures.Parameters;

public interface IParameterDefinition
{
    string Key { get; }

    string? DisplayLabel { get; }
    string? Category { get; }
    string? Description { get; }
    ParameterStatus Status { get; }
    bool IsGlobal { get; }
    
    Type BaseType { get; }

    object? DefaultValue { get; }

    bool IsDefault(object? value);
    string FormatValue(object? value);
}
