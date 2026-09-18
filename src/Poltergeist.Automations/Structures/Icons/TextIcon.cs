namespace Poltergeist.Automations.Structures;

public class TextIcon : IconInfo
{
    public string Text { get; }
    public string? Font { get; set; }

    public TextIcon(string text)
    {
        Text = text;
    }

    public override string ToString() => Text;
}
