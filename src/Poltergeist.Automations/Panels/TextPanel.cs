using System;
using System.Collections.Generic;
using System.Windows.Media;
using Poltergeist.Automations.Processors;

namespace Poltergeist.Automations.Panels;

public class TextPanelModel : OutputPanelModel<TextPanelControl, TextPanelViewModel>
{
    public Color? BackgroundColor { get; set; }
    public Color? ForegroundColor { get; set; }
    public Dictionary<string, Color> Colors = new();

    public event Action<TextLine[]> TextRecived;

    public TextPanelModel(MacroProcessor processor) : base(processor)
    {
    }

    public void WriteLine(TextLine line)
    {
        var lines = new[] { line };
        Processor.RaiseAction(() =>
        {
            TextRecived?.Invoke(lines);
        });
    }

    public void WriteLines(params TextLine[] lines)
    {
        Processor.RaiseAction(() =>
        {
            TextRecived?.Invoke(lines);
        });
    }

    public override object CreateViewModel()
    {
        return new TextPanelViewModel(this);
    }
}
