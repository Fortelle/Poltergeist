using System.Collections.ObjectModel;
using System.Drawing;
using Poltergeist.Automations.Processors;

namespace Poltergeist.Automations.Components.Panels;

public class TextInstrument : InstrumentModel
{
    public Color? BackgroundColor { get; set; }
    public Color? ForegroundColor { get; set; }
    public Dictionary<string, TextLine> Templates { get; } = new();
    public ObservableCollection<TextLine> TextCollection { get; set; } = new();

    public TextInstrument(MacroProcessor processor) : base(processor)
    {
    }

    public void WriteLine(TextLine line)
    {
        var lines = new[] { line };
        WriteLines(lines);
    }

    public void WriteLines(params TextLine[] lines)
    {
        lock (this)
        {
            foreach (var line in lines)
            {
                if (!string.IsNullOrEmpty(line.TemplateKey) && Templates.TryGetValue(line.TemplateKey, out var template))
                {
                    line.TemplateKey = null;
                    ApplyTemplate(line, template);
                }

                TextCollection.Add(line);
            }
        }
    }

    private static void ApplyTemplate(TextLine item, TextLine template)
    {
        item.Text ??= template.Text;
        item.Foreground ??= template.Foreground;
        item.IsBold ??= template.IsBold;
        item.IsItalic ??= template.IsItalic;
        item.TemplateKey ??= template.TemplateKey;
    }
}
