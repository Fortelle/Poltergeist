using System.Drawing;

namespace Poltergeist.Automations.Components.Panels;

public class TextLine
{
    public string Text { get; set; }
    public string? TemplateKey { get; set; }

    public Color? Foreground { get; set; }
    public bool? IsBold { get; set; }
    public bool? IsItalic { get; set; }

    public object? ExtraData { get; set; }

    public TextLine()
    {
        Text = "";
    }

    public TextLine(string text)
    {
        Text = text;
    }
}
