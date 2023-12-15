namespace Poltergeist.Automations.Parameters;

public class ChoiceEntry
{
    public object? Value { get; set; }
    public string Text { get; set; }

    public ChoiceEntry(object? value, string text)
    {
        Value = value;
        Text = text;
    }

    public ChoiceEntry(object? value)
    {
        Value = value;
        Text = value?.ToString() ?? "";
    }
}
