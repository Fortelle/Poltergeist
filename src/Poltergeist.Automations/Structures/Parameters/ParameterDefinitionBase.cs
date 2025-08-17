namespace Poltergeist.Automations.Structures.Parameters;

public abstract class ParameterDefinitionBase
{
    public string Key { get; }

    private string? _displayLabel;
    public string DisplayLabel
    {
        get => _displayLabel ?? Key;
        set => _displayLabel = value;
    }

    public string? Category { get; set; }

    public string? Description { get; set; }

    public ParameterStatus Status { get; set; }

    public bool IsGlobal { get; set; }

    public ParameterDefinitionBase(string key)
    {
        Key = key;
    }

    public static implicit operator string(ParameterDefinitionBase definition) => new(definition.Key);
}
