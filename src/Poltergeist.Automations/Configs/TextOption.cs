namespace Poltergeist.Automations.Configs;

public class TextOption : OptionItem<string>
{
    public string? Placeholder { get; set; }
    public int MaxLenght { get; set; }

    public TextOption(string key, string defaultValue = "") : base(key, defaultValue)
    {
    }
}
