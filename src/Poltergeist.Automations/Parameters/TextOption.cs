namespace Poltergeist.Automations.Parameters;

public class TextOption : OptionDefinition<string>
{
    public string? Placeholder { get; set; }
    public int MaxLenght { get; set; }
    public bool Multiline { get; set; }
    public Func<string, bool>? Valid { get; set; }
    
    public bool IsValid(string? text)
    {
        return text is null || (Valid?.Invoke(text) ?? true);
    }

    public TextOption(string key, string defaultValue = "") : base(key, defaultValue)
    {
    }
}
