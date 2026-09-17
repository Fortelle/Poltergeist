namespace Poltergeist.Automations.Structures.Parameters;

public class OptionDefinition<T> : ParameterDefinition<T>, IOptionDefinition where T : notnull
{
    public bool IsGlobal { get; set; }

    public string? Parent { get; set; }

    public OptionDefinition(string key) : base(key)
    {
    }

    public OptionDefinition(string key, T defaultValue) : base(key, defaultValue)
    {
    }

    public OptionDefinition(string key, string title, T defaultValue) : this(key, defaultValue)
    {
        DisplayLabel = title;
    }
}
