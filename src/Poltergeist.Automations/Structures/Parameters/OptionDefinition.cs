namespace Poltergeist.Automations.Structures.Parameters;

public class OptionDefinition<T> : ParameterDefinition<T>, IParameterDefinition where T : notnull
{
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
