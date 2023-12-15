namespace Poltergeist.Automations.Parameters;

public class PasswordOption : OptionItem<PasswordValue>
{
    public string? Placeholder { get; set; }
    public int MaxLenght { get; set; }

    public PasswordOption(string key, string defaultValue = "") : base(key, new PasswordValue(defaultValue))
    {
    }

}
