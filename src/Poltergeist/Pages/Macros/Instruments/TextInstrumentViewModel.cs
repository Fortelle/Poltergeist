using Microsoft.UI.Text;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Media;
using Poltergeist.Automations.Components.Panels;
using Windows.UI;

namespace Poltergeist.Pages.Macros.Instruments;

public class TextInstrumentViewModel : IInstrumentViewModel
{
    public SolidColorBrush? Background { get; set; }
    public SolidColorBrush? Foreground { get; set; }
    public RichTextBlock? RichTextBlock { get; set; }
    public string? Title { get; set; }

    private readonly Dictionary<Color, SolidColorBrush> Brushes = new();
    private readonly TextInstrument Model;
    
    public TextInstrumentViewModel(TextInstrument model)
    {
        Model = model;

        Title = model.Title;

        if (model.BackgroundColor.HasValue)
        {
            Background = new SolidColorBrush(model.BackgroundColor.Value);
        }

        if (model.ForegroundColor.HasValue)
        {
            Foreground = new SolidColorBrush(model.ForegroundColor.Value);
        }

        model.TextCollection.CollectionChanged += TextCollection_CollectionChanged;
    }

    private void TextCollection_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        if (RichTextBlock is null)
        {
            return;
        }

        if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
        {
            foreach (var line in e.NewItems!.OfType<TextLine>())
            {
                PushLine(line);
            }
        }
    }

    public void Refresh()
    {
        foreach (var line in Model.TextCollection)
        {
            PushLine(line);
        }
    }

    public void PushLine(TextLine line)
    {
        var run = new Run
        {
            Text = line.Text
        };
        if (line.Foreground.HasValue)
        {
            run.Foreground = GetBrush(line.Foreground.Value);
        }
        if (line.IsBold.HasValue && line.IsBold == true)
        {
            run.FontWeight = FontWeights.Bold;
        }
        if (line.IsItalic.HasValue && line.IsItalic == true)
        {
            run.FontStyle = Windows.UI.Text.FontStyle.Italic;
        }

        var block = new Paragraph();
        block.Inlines.Add(run);

        RichTextBlock!.Blocks.Add(block);
    }

    private SolidColorBrush GetBrush(Color color)
    {
        if(!Brushes.TryGetValue(color, out var brush))
        {
            brush = new SolidColorBrush(color);
            Brushes.Add(color, brush);
        }

        return brush;
    }
}
