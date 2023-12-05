namespace Poltergeist.Automations.Configs;

public class TextOption : OptionItem<string>
{
    public string? Placeholder { get; set; }
    public int MaxLenght { get; set; }
    public bool Multiline { get; set; }
    public Func<string, bool>? Valid { get; set; }

    public bool IsValid => Value is null || (Valid?.Invoke(Value) ?? true);

    public TextOption(string key, string defaultValue = "") : base(key, defaultValue)
    {
    }
}
