namespace Poltergeist.Automations.Structures.Parameters;

public class PasswordOption : OptionDefinition<PasswordValue>
{
    public string? Placeholder { get; set; }
    public int MaxLength { get; set; }

    public PasswordOption(string key) : base(key, new PasswordValue())
    {
    }

    public PasswordOption(string key, string defaultValue) : base(key, new PasswordValue(defaultValue))
    {
    }
}
