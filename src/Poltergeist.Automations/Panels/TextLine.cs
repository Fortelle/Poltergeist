using System.Windows.Media;

namespace Poltergeist.Automations.Panels;

public class TextLine
{
    public string Text { get; set; }
    public string Category { get; set; }

    public Color? Color { get; set; }
    public SolidColorBrush Brush { get; set; }
    public bool IsBold { get; set; }
    public bool IsItalic { get; set; }
}
